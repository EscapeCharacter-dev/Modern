using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing
{
    /// <summary>
    /// This class "tokenizes" a string.
    /// </summary>
    internal class Tokenizer
    {
        public Tokenizer(string input)
        {
            Input = "\n" + input;
        }

        /// <summary>
        /// The input string. Modifiable because of macros.
        /// </summary>
        public string Input { get; private set; }

        /// <summary>
        /// The position in the string.
        /// </summary>
        public int Position { get; private set; } = 0;

        /// <summary>
        /// Peeks at a character in the input string.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public char Peek(int offset = 0)
        {
            var newPos = Position + offset;
            return newPos < Input.Length ? Input[newPos] : '\0';
        }

        /// <summary>
        /// The current character.
        /// </summary>
        public char Current => Peek();

        /// <summary>
        /// The next character.
        /// </summary>
        public char Ahead => Peek(1);

        /// <summary>
        /// Gets the current character and increments the position.
        /// </summary>
        private char Next
        {
            get
            {
                var c = Current;
                Position++;
                return c;
            }
        }
    
        /// <summary>
        /// Gets the position in line and column.
        /// </summary>
        /// <returns>The current line and column.</returns>
        private (int Line, int Column) Get2DPosition()
        {
            var line = 0;
            var col = 0;
            char c = '\0';
            char returnedChar = '\0';
            int strLength = Input.Length;
            
            unsafe
            {
                fixed (char* p = Input)
                {
                    var p1 = p;
                    for (int i = 0; i < Position; i++)
                    {
                        c = *p1++;
                        if (c == '\n')
                        {
                            line++;
                            col = 0;
                        }
                        else
                        {
                            col++;
                        }
                        returnedChar = c;
                    }
                }
            }

            return (line, col + 1);
        }

        /// <summary>
        /// The list of macros.
        /// </summary>
        private readonly Dictionary<string, string> _macros = new Dictionary<string, string>();

        /// <summary>
        /// Resolves an identifier (can be a macro)
        /// </summary>
        /// <param name="ident">The name of the identifier</param>
        /// <returns>The identifier or macro</returns>
        private string ResolveIdentifier(string ident)
        {
            if (_macros.ContainsKey(ident.Trim()))
                return _macros[ident.Trim()];
            return ident;
        }

        /// <summary>
        /// Gets the next token.
        /// </summary>
        /// <returns>Next token.</returns>
        public Token NextToken()
        {
            while (char.IsWhiteSpace(Current)) Position++;
            if (Current == '\0')
                return new Token { Kind = Token.TokenKind.EndOfFile };

            {
                var substr = Input.Substring(0, Position + 1).Trim(' ', '\t');
                var index = substr.LastIndexOf('\n');
                index = index >= 0 ? index : 0;
                var pos = substr.Length - 1 - index;
                var old = Position;
                if (pos == 1 && Current == '#')
                {
                    Position++;
                    var command = new StringBuilder();
                    while (!char.IsWhiteSpace(Current) && Current != '\0')
                    {
                        command.Append(Current);
                        Position++;
                    }
                    switch (command.ToString().ToLower())
                    {
                    case "define":
                        {
                            while (char.IsWhiteSpace(Current)) Position++;
                            var alias = new StringBuilder();
                            while (!char.IsWhiteSpace(Current) && Current != '\n' && Current != '\0')
                            {
                                alias.Append(Current);
                                Position++;
                            }
                            if (string.IsNullOrWhiteSpace(alias.ToString()))
                            {
                                var p2d = Get2DPosition();
                                // TODO: Use DiagnosticHandler
                                Console.WriteLine($"[{p2d.Line},{p2d.Column}] Macro '{alias}' is not valid");
                                break;
                            }
                            if (_macros.ContainsKey(alias.ToString()))
                            {
                                var p2d = Get2DPosition();
                                // TODO: Use DiagnosticHandler
                                Console.WriteLine($"[{p2d.Line},{p2d.Column}] Macro '{alias}' already declared");
                                break;
                            }
                            while (char.IsWhiteSpace(Current)) Position++;
                            var lead = new StringBuilder();
                            while (Current != '\n' && Current != '\0')
                            {
                                lead.Append(Current);
                                Position++;
                            }
                            _macros.Add(alias.ToString().Trim(), lead.ToString().Trim());
                            Console.WriteLine($"{old} {Position} '{alias}' '{lead}'");
                            return NextToken();
                        }
                    case "undef":
                        {
                            while (char.IsWhiteSpace(Current)) Position++;
                            var alias = new StringBuilder();
                            while (!char.IsWhiteSpace(Current) && Current != '\n' && Current != '\0')
                            {
                                alias.Append(Current);
                                Position++;
                            }
                            if (!_macros.ContainsKey(alias.ToString()))
                            {
                                var p2d = Get2DPosition();
                                // TODO: Use DiagnosticHandler
                                Console.WriteLine($"[{p2d.Line},{p2d.Column}] Macro '{alias}' not declared");
                            }
                            _macros.Remove(alias.ToString());
                            return NextToken();
                        }
                    }
                }
            }
            switch (Current)
            {
            case '+':
                if (Ahead == '+')
                {
                    var p2d = Get2DPosition();
                    Position += 2;
                    return new Token
                    {
                        Kind = Token.TokenKind.PlusPlus,
                        Source = "++",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
                else if (Ahead == '=')
                {
                    var p2d = Get2DPosition();
                    Position += 2;
                    return new Token
                    {
                        Kind = Token.TokenKind.PlusEqual,
                        Source = "+=",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
                else
                {
                    var p2d = Get2DPosition();
                    Position++;
                    return new Token
                    {
                        Kind = Token.TokenKind.Plus,
                        Source = "+",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
            case '-':
                if (Ahead == '-')
                {
                    var p2d = Get2DPosition();
                    Position += 2;
                    return new Token
                    {
                        Kind = Token.TokenKind.MinusMinus,
                        Source = "--",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
                else if (Ahead == '=')
                {
                    var p2d = Get2DPosition();
                    Position += 2;
                    return new Token
                    {
                        Kind = Token.TokenKind.MinusEqual,
                        Source = "-=",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
                else if (Ahead == '>')
                {
                    var p2d = Get2DPosition();
                    Position += 2;
                    return new Token
                    {
                        Kind = Token.TokenKind.Arrow,
                        Source = "->",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
                else
                {
                    var p2d = Get2DPosition();
                    Position++;
                    return new Token
                    {
                        Kind = Token.TokenKind.Minus,
                        Source = "-",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
            case '&':
                if (Ahead == '&')
                {
                    var p2d = Get2DPosition();
                    Position += 2;
                    return new Token
                    {
                        Kind = Token.TokenKind.Ampersands,
                        Source = "&&",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
                else if (Ahead == '=')
                {
                    var p2d = Get2DPosition();
                    Position += 2;
                    return new Token
                    {
                        Kind = Token.TokenKind.AmpersandEqual,
                        Source = "&=",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
                else
                {
                    var p2d = Get2DPosition();
                    Position++;
                    return new Token
                    {
                        Kind = Token.TokenKind.Ampersand,
                        Source = "&",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
            case '^':
                if (Ahead == '^')
                {
                    var p2d = Get2DPosition();
                    Position += 2;
                    return new Token
                    {
                        Kind = Token.TokenKind.Carets,
                        Source = "^^",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
                else if (Ahead == '=')
                {
                    var p2d = Get2DPosition();
                    Position += 2;
                    return new Token
                    {
                        Kind = Token.TokenKind.CaretEqual,
                        Source = "^=",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
                else
                {
                    var p2d = Get2DPosition();
                    Position++;
                    return new Token
                    {
                        Kind = Token.TokenKind.Caret,
                        Source = "^",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
            case '|':
                if (Ahead == '|')
                {
                    var p2d = Get2DPosition();
                    Position += 2;
                    return new Token
                    {
                        Kind = Token.TokenKind.Pipes,
                        Source = "||",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
                else if (Ahead == '=')
                {
                    var p2d = Get2DPosition();
                    Position += 2;
                    return new Token
                    {
                        Kind = Token.TokenKind.PipeEqual,
                        Source = "|=",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
                else
                {
                    var p2d = Get2DPosition();
                    Position++;
                    return new Token
                    {
                        Kind = Token.TokenKind.Ampersand,
                        Source = "&",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
            case '*':
                if (Ahead == '=')
                {
                    var p2d = Get2DPosition();
                    Position += 2;
                    return new Token
                    {
                        Kind = Token.TokenKind.StarEqual,
                        Source = "*=",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
                else
                {
                    var p2d = Get2DPosition();
                    Position++;
                    return new Token
                    {
                        Kind = Token.TokenKind.Star,
                        Source = "*",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
            case '/':
                if (Ahead == '*')
                {
                    var p2d = Get2DPosition();
                    Position += 2;
                    while (!(Current == '*' && Ahead == '/'))
                    {
                        if (Current == '\0')
                        {
                            // TODO: Use DiagnosticHandler
                            Console.WriteLine($"[{p2d.Line},{p2d.Column}] Unmatched comment, expected */");
                            return null;
                        }
                        Position++;
                    }
                    Position += 2;
                    return NextToken();
                }
                else if (Ahead == '/')
                {
                    var p2d = Get2DPosition();
                    Position += 2;
                    while (Current != '\n' && Current != '\0')
                        Position++;
                    Position++;
                    return NextToken();
                }
                else if (Ahead == '=')
                {
                    var p2d = Get2DPosition();
                    Position += 2;
                    return new Token
                    {
                        Kind = Token.TokenKind.SlashEqual,
                        Source = "/=",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
                else
                {
                    var p2d = Get2DPosition();
                    Position++;
                    return new Token
                    {
                        Kind = Token.TokenKind.Slash,
                        Source = "/",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
            case '%':
                if (Ahead == '=')
                {
                    var p2d = Get2DPosition();
                    Position += 2;
                    return new Token
                    {
                        Kind = Token.TokenKind.PercentEqual,
                        Source = "%=",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
                else
                {
                    var p2d = Get2DPosition();
                    Position++;
                    return new Token
                    {
                        Kind = Token.TokenKind.Percent,
                        Source = "%",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
            case '=':
                if (Ahead == '=')
                {
                    var p2d = Get2DPosition();
                    Position += 2;
                    return new Token
                    {
                        Kind = Token.TokenKind.Equals,
                        Source = "==",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
                else
                {
                    var p2d = Get2DPosition();
                    Position++;
                    return new Token
                    {
                        Kind = Token.TokenKind.Equal,
                        Source = "=",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
            case '!':
                if (Ahead == '=')
                {
                    var p2d = Get2DPosition();
                    Position += 2;
                    return new Token
                    {
                        Kind = Token.TokenKind.ExclamationEqual,
                        Source = "!=",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
                else
                {
                    var p2d = Get2DPosition();
                    Position++;
                    return new Token
                    {
                        Kind = Token.TokenKind.Exclamation,
                        Source = "!",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
            case '<':
                if (Ahead == '<')
                {
                    Position++;
                    if (Ahead == '=')
                    {
                        var p2d = Get2DPosition();
                        Position += 2;
                        return new Token
                        {
                            Kind = Token.TokenKind.LeftArrowsEqual,
                            Source = "<<=",
                            Line = p2d.Line,
                            Column = p2d.Column
                        };
                    }
                    else
                    {
                        var p2d = Get2DPosition();
                        Position++;
                        return new Token
                        {
                            Kind = Token.TokenKind.LeftArrows,
                            Source = "<<",
                            Line = p2d.Line,
                            Column = p2d.Column
                        };
                    }
                }
                else if (Ahead == '=')
                {
                    var p2d = Get2DPosition();
                    Position += 2;
                    return new Token
                    {
                        Kind = Token.TokenKind.LeftArrowEqual,
                        Source = "<=",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
                else
                {
                    var p2d = Get2DPosition();
                    Position++;
                    return new Token
                    {
                        Kind = Token.TokenKind.LeftArrow,
                        Source = "<",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
            case '>':
                if (Ahead == '>')
                {
                    Position++;
                    if (Ahead == '=')
                    {
                        var p2d = Get2DPosition();
                        Position += 2;
                        return new Token
                        {
                            Kind = Token.TokenKind.RightArrowsEqual,
                            Source = ">>=",
                            Line = p2d.Line,
                            Column = p2d.Column
                        };
                    }
                    else
                    {
                        var p2d = Get2DPosition();
                        Position++;
                        return new Token
                        {
                            Kind = Token.TokenKind.RightArrows,
                            Source = ">>",
                            Line = p2d.Line,
                            Column = p2d.Column
                        };
                    }
                }
                else if (Ahead == '=')
                {
                    var p2d = Get2DPosition();
                    Position += 2;
                    return new Token
                    {
                        Kind = Token.TokenKind.RightArrowEqual,
                        Source = ">=",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
                else
                {
                    var p2d = Get2DPosition();
                    Position++;
                    return new Token
                    {
                        Kind = Token.TokenKind.RightArrow,
                        Source = ">",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
            case '.':
                {
                    var p2d = Get2DPosition();
                    Position++;
                    if (Current == '.')
                    {
                        Position++;
                        if (Current == '.')
                        {
                            Position++;
                            return new Token
                            {
                                Kind = Token.TokenKind.DotDotDot,
                                Source = "...",
                                Line = p2d.Line,
                                Column = p2d.Column
                            };
                        }
                        else
                            return new Token
                            {
                                Kind = Token.TokenKind.DotDot,
                                Source = "..",
                                Line = p2d.Line,
                                Column = p2d.Column
                            };
                    }
                    return new Token
                    {
                        Kind = Token.TokenKind.Dot,
                        Source = ".",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
            case '~':
                {
                    var p2d = Get2DPosition();
                    Position++;
                    return new Token
                    {
                        Kind = Token.TokenKind.Tilde,
                        Source = "~",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
            case '?':
                {
                    var p2d = Get2DPosition();
                    Position++;
                    return new Token
                    {
                        Kind = Token.TokenKind.QuestionMark,
                        Source = "?",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
            case ':':
                {
                    var p2d = Get2DPosition();
                    Position++;
                    if (Current == ':')
                    {
                        Position++;
                        return new Token
                        {
                            Kind = Token.TokenKind.Cube,
                            Source = "::",
                            Line = p2d.Line,
                            Column = p2d.Column
                        };
                    }
                    return new Token
                    {
                        Kind = Token.TokenKind.Colon,
                        Source = ":",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
            case ';':
                {
                    var p2d = Get2DPosition();
                    Position++;
                    return new Token
                    {
                        Kind = Token.TokenKind.Semicolon,
                        Source = ";",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
            case '(':
                {
                    var p2d = Get2DPosition();
                    Position++;
                    return new Token
                    {
                        Kind = Token.TokenKind.OpenParenthesis,
                        Source = "(",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
            case ')':
                {
                    var p2d = Get2DPosition();
                    Position++;
                    return new Token
                    {
                        Kind = Token.TokenKind.ClosingParenthesis,
                        Source = ")",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
            case '[':
                {
                    var p2d = Get2DPosition();
                    Position++;
                    return new Token
                    {
                        Kind = Token.TokenKind.OpenSquareBracket,
                        Source = "[",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
            case ']':
                {
                    var p2d = Get2DPosition();
                    Position++;
                    return new Token
                    {
                        Kind = Token.TokenKind.ClosingSquareBracket,
                        Source = "]",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
            case '{':
                {
                    var p2d = Get2DPosition();
                    Position++;
                    return new Token
                    {
                        Kind = Token.TokenKind.OpenBracket,
                        Source = "{",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
            case '}':
                {
                    var p2d = Get2DPosition();
                    Position++;
                    return new Token
                    {
                        Kind = Token.TokenKind.ClosingBracket,
                        Source = "}",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
            case ',':
                {
                    var p2d = Get2DPosition();
                    Position++;
                    return new Token
                    {
                        Kind = Token.TokenKind.Comma,
                        Source = ",",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
            case '@':
                {
                    var p2d = Get2DPosition();
                    Position++;
                    return new Token
                    {
                        Kind = Token.TokenKind.At,
                        Source = "@",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
            case '$':
                {
                    var p2d = Get2DPosition();
                    Position++;
                    return new Token
                    {
                        Kind = Token.TokenKind.Dollar,
                        Source = "$",
                        Line = p2d.Line,
                        Column = p2d.Column
                    };
                }
            default:
                {
                    if (char.IsDigit(Current))
                    {
                        var p2d = Get2DPosition();
                        var sb = new StringBuilder();
                        sb.Append(Current);
                        Position++;
                        if (sb[0] == '0')
                        {
                            switch (char.ToLower(Current))
                            {
                            case 'x':
                                sb.Append(Current);
                                Position++;
                                while (char.IsDigit(Current)
                                    || char.ToLower(Current) >= 'a'
                                    && char.ToLower(Current) <= 'f'
                                    )
                                {
                                    sb.Append(Current);
                                    Position++;
                                }
                                if (ulong.TryParse(sb.ToString().Substring(2), NumberStyles.AllowHexSpecifier, null, out var hexU64Result))
                                    return new Token
                                    {
                                        Kind = Token.TokenKind.IntLiteral,
                                        Source = sb.ToString(),
                                        Data = hexU64Result,
                                        Line = p2d.Line,
                                        Column = p2d.Column
                                    };
                                else
                                {
                                    // TODO: Use DiagnosticHandler
                                    Console.WriteLine($"Invalid hex number '{sb}'");
                                    return new Token
                                    {
                                        Kind = Token.TokenKind.Invalid
                                    };
                                }
                            case 'b':
                                {
                                    sb.Append(Current);
                                    Position++;
                                    ulong binary = 0;
                                    while (Current == '0' || Current == '1')
                                    {
                                        sb.Append(Current);
                                        binary <<= 1;
                                        binary |= (byte)(Current - '0');
                                        Position++;
                                    }
                                    return new Token
                                    {
                                        Kind = Token.TokenKind.IntLiteral,
                                        Source = sb.ToString(),
                                        Data = binary,
                                        Line = p2d.Line,
                                        Column = p2d.Column
                                    };
                                }
                            case '0':
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                                {
                                    sb.Append(Current);
                                    ulong octal = 0;
                                    while (Current >= '0' && Current <= '7')
                                    {
                                        sb.Append(Current);
                                        octal <<= 3;
                                        octal |= (byte)(Current - '0');
                                        Position++;
                                    }
                                    return new Token
                                    {
                                        Kind = Token.TokenKind.IntLiteral,
                                        Source = sb.ToString(),
                                        Data = octal,
                                        Line = p2d.Line,
                                        Column = p2d.Column
                                    };
                                }
                            }
                        }
                        char last = '\0';
                        while (char.IsDigit(Current) || Current == '.' || char.ToLower(Current) == 'e')
                        {
                            if (Current == '.' && last == '.')
                            {
                                var backup = sb.ToString();
                                sb.Clear();
                                sb.Append(backup.TrimEnd('.'));
                                Position--;
                                break;
                            }
                            sb.Append(Current);
                            last = Current;
                            Position++;
                        }
                        if (ulong.TryParse(sb.ToString(), out var u64Result))
                            return new Token
                            {
                                Kind = Token.TokenKind.IntLiteral,
                                Source = sb.ToString(),
                                Data = u64Result,
                                Line = p2d.Line,
                                Column = p2d.Column
                            };
                        else if (decimal.TryParse(sb.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var f128Result))
                            return new Token
                            {
                                Kind = Token.TokenKind.FloatLiteral,
                                Source = sb.ToString(),
                                Data = f128Result,
                                Line = p2d.Line,
                                Column = p2d.Column
                            };
                        else
                        {
                            // TODO: Replace with DiagonisticHandler
                            Console.WriteLine($"{sb} is not a valid integer or floating point value");
                            return new Token { Kind = Token.TokenKind.Invalid };
                        }
                    }
                    else if (Current == '\'')
                    {
                        var p2d = Get2DPosition();
                        var sb = new StringBuilder();
                        sb.Append(Current);
                        Position++;
                        while (Current != '\'')
                        {
                            sb.Append(Current);
                            Position++;
                        }
                        Position++;
                        var osb = new StringBuilder();
                        for (int i = 1; i < sb.Length; i++)
                        {
                            if (sb[i] != '\\')
                                osb.Append(sb[i]);
                            else
                            {
                                switch (char.ToLower(sb[++i]))
                                {
                                case 'a': osb.Append((char)0x07); break;
                                case 'b': osb.Append((char)0x08); break;
                                case 'e': osb.Append((char)0x1B); break;
                                case 'f': osb.Append((char)0x0C); break;
                                case 'n': osb.Append((char)0x0A); break;
                                case 'r': osb.Append((char)0x0D); break;
                                case 't': osb.Append((char)0x09); break;
                                case 'v': osb.Append((char)0x0B); break;
                                case '\\':osb.Append((char)0x5C); break;
                                case '\'':osb.Append((char)0x27); break;
                                case '"': osb.Append((char)0x22); break;
                                case '?': osb.Append((char)0x3F); break;
                                case '0':
                                case '1':
                                case '2':
                                case '3':
                                case '4':
                                case '5':
                                case '6':
                                case '7':
                                    {
                                        var octalBuf = new char[3];
                                        octalBuf[0] = sb[i];
                                        if (i + 1 >= sb.Length)
                                            goto skip;
                                        if (sb[++i] >= '0' && sb[i] <= '7')
                                            octalBuf[1] = sb[i++];
                                        if (i >= sb.Length)
                                            goto skip;
                                        if (sb[i] >= '0' && sb[i] <= '7')
                                            octalBuf[2] = sb[i];
                                        skip:
                                        var str = new string(octalBuf);
                                        str = str.Trim('\0');
                                        try
                                        {
                                            var converted = Convert.ToByte(str, 8);
                                            osb.Append((char)converted);
                                        }
                                        catch (Exception e)
                                        {
                                            if (e is not FormatException && e is not OverflowException)
                                                throw;
                                            Console.WriteLine($"'{str}' is not a valid octal byte");
                                            return new Token { Line = p2d.Line, Column = p2d.Column, Kind = Token.TokenKind.Invalid };
                                        }
                                        break;
                                    }
                                case 'x':
                                    {
                                        var hexBuf = new char[2];
                                        if (sb[++i] >= '0' && sb[i] <= '9' || char.ToLower(sb[i]) >= 'a' && char.ToLower(sb[i]) <= 'f')
                                            hexBuf[0] = sb[i++];
                                        if (i >= sb.Length)
                                            goto skip;
                                        if (sb[i] >= '0' && sb[i] <= '9' || char.ToLower(sb[i]) >= 'a' && char.ToLower(sb[i]) <= 'f')
                                            hexBuf[1] = sb[i++];
                                        skip:
                                        var str = new string(hexBuf);
                                        str = str.Trim('\0');
                                        try
                                        {
                                            var converted = Convert.ToByte(str, 0x10);
                                            osb.Append((char)converted);
                                        }
                                        catch (FormatException)
                                        {
                                            Console.WriteLine($"'{str}' is not a valid octal byte");
                                            return new Token { Line = p2d.Line, Column = p2d.Column, Kind = Token.TokenKind.Invalid };
                                        }
                                        break;
                                    }
                                default: osb.Append((char)0x3F); break;
                                }
                            }
                        }
                        ulong result = 0;
                        for (int i = 0; i < osb.Length; i++)
                        {
                            result <<= 8;
                            result |= osb[i];
                        }
                        return new Token
                        {
                            Kind = Token.TokenKind.IntLiteral,
                            Source = sb.Append('\'').ToString(),
                            Data = result,
                            Line = p2d.Line,
                            Column = p2d.Column
                        };
                    }
                    else if (Current == '"')
                    {
                        var p2d = Get2DPosition();
                        var sb = new StringBuilder();
                        sb.Append(Current);
                        Position++;
                        while (Current != '"')
                        {
                            sb.Append(Current);
                            Position++;
                        }
                        Position++;
                        var osb = new StringBuilder();
                        for (int i = 1; i < sb.Length; i++)
                        {
                            if (sb[i] != '\\')
                                osb.Append(sb[i]);
                            else
                            {
                                switch (char.ToLower(sb[++i]))
                                {
                                case 'a': osb.Append((char)0x07); break;
                                case 'b': osb.Append((char)0x08); break;
                                case 'e': osb.Append((char)0x1B); break;
                                case 'f': osb.Append((char)0x0C); break;
                                case 'n': osb.Append((char)0x0A); break;
                                case 'r': osb.Append((char)0x0D); break;
                                case 't': osb.Append((char)0x09); break;
                                case 'v': osb.Append((char)0x0B); break;
                                case '\\': osb.Append((char)0x5C); break;
                                case '\'': osb.Append((char)0x27); break;
                                case '"': osb.Append((char)0x22); break;
                                case '?': osb.Append((char)0x3F); break;
                                default: osb.Append((char)0x3F); break;
                                }
                            }
                        }
                        return new Token
                        {
                            Kind = Token.TokenKind.StringLiteral,
                            Source = sb.Append('"').ToString(),
                            Data = sb.ToString().Trim('"'),
                            Line = p2d.Line,
                            Column = p2d.Column
                        };
                    }
                    else if (char.IsLetter(Current) || Current == '_')
                    {
                        var p2d = Get2DPosition();
                        var sb = new StringBuilder();
                        sb.Append(Current);
                        Position++;
                        while (char.IsLetterOrDigit(Current) || Current == '_')
                        {
                            sb.Append(Current);
                            Position++;
                        }

                        switch (sb.ToString())
                        {
                        case "void": return new Token(Token.TokenKind.Void, p2d);
                        case "byte": return new Token(Token.TokenKind.Byte, p2d);
                        case "sbyte": return new Token(Token.TokenKind.SByte, p2d);
                        case "bool": return new Token(Token.TokenKind.Bool, p2d);
                        case "char": return new Token(Token.TokenKind.Char, p2d);
                        case "uchar": return new Token(Token.TokenKind.UChar, p2d);
                        case "wchar": return new Token(Token.TokenKind.WChar, p2d);
                        case "short": return new Token(Token.TokenKind.Short, p2d);
                        case "ushort": return new Token(Token.TokenKind.UShort, p2d);
                        case "int": return new Token(Token.TokenKind.Int, p2d);
                        case "uint": return new Token(Token.TokenKind.UInt, p2d);
                        case "long": return new Token(Token.TokenKind.Long, p2d);
                        case "ulong": return new Token(Token.TokenKind.ULong, p2d);
                        case "huge": return new Token(Token.TokenKind.Huge, p2d);
                        case "uhuge": return new Token(Token.TokenKind.UHuge, p2d);
                        case "float": return new Token(Token.TokenKind.Float, p2d);
                        case "double": return new Token(Token.TokenKind.Double, p2d);
                        case "ldouble": return new Token(Token.TokenKind.LDouble, p2d);
                        case "function": return new Token(Token.TokenKind.Function, p2d);
                        case "break": return new Token(Token.TokenKind.Break, p2d);
                        case "case": return new Token(Token.TokenKind.Case, p2d);
                        case "const": return new Token(Token.TokenKind.Const, p2d);
                        case "continue": return new Token(Token.TokenKind.Continue, p2d);
                        case "default": return new Token(Token.TokenKind.Default, p2d);
                        case "do": return new Token(Token.TokenKind.Do, p2d);
                        case "else": return new Token(Token.TokenKind.Else, p2d);
                        case "enum": return new Token(Token.TokenKind.Enum, p2d);
                        case "extern": return new Token(Token.TokenKind.Extern, p2d);
                        case "for": return new Token(Token.TokenKind.For, p2d);
                        case "goto": return new Token(Token.TokenKind.Goto, p2d);
                        case "if": return new Token(Token.TokenKind.If, p2d);
                        case "return": return new Token(Token.TokenKind.Return, p2d);
                        case "sizeof": return new Token(Token.TokenKind.Sizeof, p2d);
                        case "struct": return new Token(Token.TokenKind.Struct, p2d);
                        case "switch": return new Token(Token.TokenKind.Switch, p2d);
                        case "union": return new Token(Token.TokenKind.Union, p2d);
                        case "volatile": return new Token(Token.TokenKind.Volatile, p2d);
                        case "while": return new Token(Token.TokenKind.While, p2d);
                        case "start": return new Token(Token.TokenKind.Start, p2d);
                        case "end": return new Token(Token.TokenKind.End, p2d);
                        case "between": return new Token(Token.TokenKind.Between, p2d);
                        case "namespace": return new Token(Token.TokenKind.Namespace, p2d);
                        case "using": return new Token(Token.TokenKind.Using, p2d);
                        case "ctor": return new Token(Token.TokenKind.Ctor, p2d);
                        case "public": return new Token(Token.TokenKind.Public, p2d);
                        case "typeof": return new Token(Token.TokenKind.Typeof, p2d);
                        default:
                            {
                                var identifier = ResolveIdentifier(sb.ToString());
                                if (identifier != sb.ToString())
                                {
                                    Input = Input.Insert(Position, identifier);
                                    return NextToken();
                                }
                                return new Token(Token.TokenKind.Identifier, p2d)
                                {
                                    Source = sb.ToString(),
                                    Data = sb.ToString()
                                };
                            }
                        }
                    }
                    var _p2d = Get2DPosition();
                    return new Token
                    {
                        Source = $"{Current}",
                        Kind = Token.TokenKind.Invalid,
                        Line = _p2d.Line,
                        Column = _p2d.Column
                    }; // return new Token {...};
                } // } (attached to default)
            } // switch
        } // function
    } // class
} // namespace
