using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression
{
    internal sealed partial class Parser
    {
        /// <summary>
        /// Parses an enum declaration.
        /// </summary>
        /// <returns>The enum declaration object.</returns>
        private void ParseEnumDecl(ref List<EnumDecl> enumDecls)
        {
            if (!Match(Token.TokenKind.Identifier)) return;
            var identifier = Current.Representation;
            Position++;
            foreach (var decl in enumDecls)
            {
                if (decl == identifier)
                {
                    // TODO: Use DiagnosticHandler
                    Console.WriteLine($"[{Current.Line},{Current.Column}] '{identifier}' is already declared in this enum");
                    return;
                }
            }
            if (Match(Token.TokenKind.Equal, false))
            {
                Position++;
                if (!Match(Token.TokenKind.IntLiteral)) return;
                var position = Current;
                Position++;
                enumDecls.Add(new EnumDecl(identifier, (ulong)position.Data));
                return;
            }
            enumDecls.Add(new EnumDecl(identifier, enumDecls.Count > 0 ? enumDecls.Last().Index + 1 : 0));
        }

        /// <summary>
        /// Parses an enum.
        /// </summary>
        /// <returns>The enum.</returns>
        private Declaration ParseEnum()
        {
            Position++; // enum keyword
            if (!Match(Token.TokenKind.Identifier)) { return null; }
            var lident = Current;
            Position++;
            var ident = lident.Representation;
            if (CurrentScope.TryLookup(out var _, CurrentScope.CurrentNamespace, ident)
                || CurrentScope.TryLookupT(out var _, CurrentScope.CurrentNamespace, ident))
            {
                // TODO: Use DiagnosticHandler
                Console.WriteLine($"[{Current.Line},{Current.Column}] '{ident}' is already declared in this scope");
                return null;
            }
            if (!Match(Token.TokenKind.OpenBracket)) { return null; }
            Position++;
            var enumDecl = new List<EnumDecl>();
            while (!Match(Token.TokenKind.ClosingBracket, false))
            {
                ParseEnumDecl(ref enumDecl);
                if (Current.Kind == Token.TokenKind.Comma)
                {
                    Position++;
                    if (Current.Kind == Token.TokenKind.ClosingBracket)
                    {
                        // TODO: Use DiagnosticHandler
                        Console.WriteLine($"[{Current.Line},{Current.Column}] A , cannot be followed by }}");
                        return null;
                    }
                    continue;
                }
                else if (Current.Kind != Token.TokenKind.ClosingBracket)
                {
                    // TODO: Use DiagnosticHandler
                    Console.WriteLine($"[{Current.Line},{Current.Column}] Expected , or }}");
                    return null;
                }
            }
            Position++;
            return new Declaration
            {
                Kind = Declaration.DeclarationKind.Enum,
                Identifier = ident,
                DefaultValue = new(null, null, enumDecl, null)
            };
        }

        private Declaration.DeclarationKind ResolveDeclarationKind(bool @public, bool @external, Declaration.DeclarationKind @base)
        {
            var kind = @base;
            switch (kind)
            {
            case Declaration.DeclarationKind.Variable when @public:
                kind = Declaration.DeclarationKind.PublicVariable;
                break;
            case Declaration.DeclarationKind.Variable when @external:
                kind = Declaration.DeclarationKind.ExternalVariable;
                break;
            case Declaration.DeclarationKind.Constant when @public:
                kind = Declaration.DeclarationKind.PublicConstant;
                break;
            case Declaration.DeclarationKind.Constant when @external:
                kind = Declaration.DeclarationKind.ExternalConstant;
                break;
            case Declaration.DeclarationKind.Function when @public:
                kind = Declaration.DeclarationKind.PublicFunction;
                break;
            case Declaration.DeclarationKind.Function when @external:
                kind = Declaration.DeclarationKind.ExternalFunction;
                break;
            case Declaration.DeclarationKind.Struct when @public:
                kind = Declaration.DeclarationKind.PublicStruct;
                break;
            case Declaration.DeclarationKind.Struct when @external:
                kind = Declaration.DeclarationKind.ExternalStruct;
                break;
            case Declaration.DeclarationKind.Union when @public:
                kind = Declaration.DeclarationKind.PublicUnion;
                break;
            case Declaration.DeclarationKind.Union when @external:
                kind = Declaration.DeclarationKind.ExternalUnion;
                break;
            case Declaration.DeclarationKind.Enum when @public:
                kind = Declaration.DeclarationKind.PublicEnum;
                break;
            }
            return kind;
        }

        /// <summary>
        /// Parses a structure.
        /// </summary>
        /// <returns>The structure.</returns>
        private Declaration ParseStruct(bool isUnion = false)
        {
            Position++; // struct/union keyword
            if (!Match(Token.TokenKind.Identifier)) { return null; }
            var lident = Current;
            Position++;
            var ident = lident.Representation;
            if (CurrentScope.TryLookup(out var _, CurrentScope.CurrentNamespace, ident)
                || CurrentScope.TryLookupT(out var _, CurrentScope.CurrentNamespace, ident))
            {
                // TODO: Use DiagnosticHandler
                Console.WriteLine($"[{Current.Line},{Current.Column}] '{ident}' is already declared in this scope");
                return null;
            }
            if (!Match(Token.TokenKind.OpenBracket)) { return null; }
            Position++;

            var sdecl = new Declaration
            {
                Kind = isUnion ? Declaration.DeclarationKind.Union : Declaration.DeclarationKind.Struct,
                Identifier = ident
            };

            ScopePush();
            CurrentScope.TryDeclareT(sdecl);
            var members = new List<Declaration>();
            while (!Match(Token.TokenKind.ClosingBracket, false))
            {
                var decl = ParseDeclaration(true, false, false);
                if (decl == null)
                {
                    // TODO: Use DiagnosticHandler
                    Console.WriteLine($"[{Before.Line},{Before.Column}] Declaration failed");
                    return null;
                }
                if (!CurrentScope.TryDeclare(decl))
                {
                    // TODO: Use DiagnosticHandler
                    Console.WriteLine($"[{Before.Line},{Before.Column}] Declaration failed, symbol is already existing");
                    return null;
                }
                members.Add(decl);
            }
            ScopePop();
            Position++;
            sdecl.DefaultValue = new(null, null, null, members);
            return sdecl;
        }

        /// <summary>
        /// Parses a declaration.
        /// </summary>
        /// <param name="otherThanVariable">Whether this this declaration can be something else than a constant/variable</param>
        /// <param name="noSemiCheck">Whether the declaration end with a semicolon</param>
        /// <returns>The declaration.</returns>
        private Declaration ParseDeclaration(bool otherThanVariable = true, bool noSemiCheck = false,
            bool equalAllowed = true, IEnumerable<string> blacklist = null)
        {
            if (Current == null)
            {
                // TODO: Replace with DiagnosticHandler
                Console.WriteLine($"[{Before.Line},{Before.Column}] Expected a declaration");
                return null;
            }
            var @public = false;
            var @extern = false;
            if (Current.Kind == Token.TokenKind.Public)
            {
                @public = true;
                Position++;
            }
            else if (Current.Kind == Token.TokenKind.Extern)
            {
                @extern = true;
                Position++;
            }
            var constant = false;
            if (Current.Kind == Token.TokenKind.Const)
            {
                constant = true;
                Position++;
            }
            if (Current.Kind == Token.TokenKind.Enum && otherThanVariable)
                return ParseEnum();
            if ((Current.Kind == Token.TokenKind.Struct || Current.Kind == Token.TokenKind.Union) && otherThanVariable)
                return ParseStruct(Current.Kind == Token.TokenKind.Union);
            if (Current == null)
            {
                // TODO: Replace with DiagnosticHandler
                Console.WriteLine($"[{Before.Line},{Before.Column}] Expected a declaration");
                Position++;
                return null;
            }
            if (!Current.Kind.Matches(
                Token.TokenKind.Void,
                Token.TokenKind.Byte,
                Token.TokenKind.SByte,
                Token.TokenKind.Char,
                Token.TokenKind.UChar,
                Token.TokenKind.Short,
                Token.TokenKind.UShort,
                Token.TokenKind.Bool,
                Token.TokenKind.WChar,
                Token.TokenKind.Int,
                Token.TokenKind.UInt,
                Token.TokenKind.Long,
                Token.TokenKind.ULong,
                Token.TokenKind.Huge,
                Token.TokenKind.UHuge,
                Token.TokenKind.Float,
                Token.TokenKind.Double,
                Token.TokenKind.LDouble,
                Token.TokenKind.Function,
                Token.TokenKind.Identifier
                ))
            {
                Console.WriteLine($"[{Current.Line},{Current.Column}] Expected a type");
                return null;
            }

            var type = ParseType();

            if (!Match(Token.TokenKind.Identifier)) { return null; }
            var lident = Current;
            Position++;
            var ident = lident.Representation;
            var pinnedAt = 0ul;
            if (Match(Token.TokenKind.At, false))
            {
                Position++;
                if (!Match(Token.TokenKind.IntLiteral)) return null;
                pinnedAt = (ulong)Current.Data;
                Position++;
            }
            if (blacklist != null && blacklist.Contains(ident) || CurrentScope.TryLookup(out var _, CurrentScope.CurrentNamespace, ident))
            {
                // TODO: Use DiagnosticHandler
                Console.WriteLine($"[{Current.Line},{Current.Column}] '{ident}' is already declared in this scope");
                Position++;
                return null;
            }

            if (Match(Token.TokenKind.OpenParenthesis, false) && otherThanVariable)
            {
                // function declaration
                Position++;
                ScopePush();
                var paramList = new List<Declaration>();
                while (!Match(Token.TokenKind.ClosingParenthesis, false))
                {
                    paramList.Add(ParseDeclaration(false, true));
                    if (Current.Kind == Token.TokenKind.Comma)
                    {
                        Position++;
                        if (Current.Kind == Token.TokenKind.ClosingParenthesis)
                        {
                            // TODO: Use DiagnosticHandler
                            Console.WriteLine($"[{Current.Line},{Current.Column}] A comma cannot be followed by a closing parenthesis");
                            return null;
                        }
                        continue;
                    }
                    else if (Current.Kind != Token.TokenKind.ClosingParenthesis)
                    {
                        // TODO: Use DiagnosticHandler
                        Console.WriteLine($"[{Current.Line},{Current.Column}] Expected , or )");
                        return null;
                    }
                }
                foreach (var param in paramList)
                    if (!CurrentScope.TryDeclare(param))
                    {
                        // TODO: Use DiagnosticHandler
                        Console.WriteLine($"[{Current.Line},{Current.Column}] '{ident}' is already declared in this scope");
                        return null;
                    }
                Position++;
                var totalType = new[] { type }.ToList().Union(paramList.Select(x => x.Type));
                if (!CurrentScope.TryDeclare(new Declaration
                {
                    Kind = ResolveDeclarationKind(@public, @extern, Declaration.DeclarationKind.Function),
                    Identifier = ident,
                    Type = new TypeInfo
                    {
                        Kind = TypeInfo.TypeKind.Function,
                        Children = new[] { type }.ToList().Union(paramList.Select(x => x.Type))
                    }
                }
                ))
                {
                    // TODO: Use DiagnosticHandler
                    Console.WriteLine($"[{Before.Line},{Before.Column}] Function is already declared");
                    return null;
                }
                if (@extern)
                {
                    if (!Match(Token.TokenKind.Semicolon)) return null;
                    Position++;
                    ScopePop();
                    return new Declaration
                    {
                        DefaultValue = new(null, null, null, null),
                        Identifier = ident,
                        Kind = ResolveDeclarationKind(@public, @extern, Declaration.DeclarationKind.Function),
                        Type = new TypeInfo
                        {
                            Kind = TypeInfo.TypeKind.Function,
                            Children = new[] { type }.ToList().Union(paramList.Select(x => x.Type))
                        },
                        PinnedAt = pinnedAt
                    };
                }
                if (Current.Kind == Token.TokenKind.Colon)
                {
                    Position++;
                    var expr = ParseExpression();
                    if (!Match(Token.TokenKind.Semicolon)) return null;
                    Position++;
                    ScopePop();
                    return new Declaration
                    {
                        DefaultValue = new (expr, null, null, null),
                        Identifier = ident,
                        Kind = ResolveDeclarationKind(@public, @extern, Declaration.DeclarationKind.Function),
                        Type = new TypeInfo
                        {
                            Kind = TypeInfo.TypeKind.Function,
                            Children = new[] { type }.ToList().Union(paramList.Select(x => x.Type))
                        },
                        PinnedAt = pinnedAt
                    };
                }
                else if (Current.Kind == Token.TokenKind.OpenBracket)
                {
                    var stat = ParseStatement();
                    ScopePop();
                    return new Declaration
                    {
                        DefaultValue = new (null, stat, null, null),
                        Identifier = ident,
                        Kind = ResolveDeclarationKind(@public, @extern, Declaration.DeclarationKind.Function),
                        Type = new TypeInfo
                        {
                            Kind = TypeInfo.TypeKind.Function,
                            Children = new[] { type }.ToList().Union(paramList.Select(x => x.Type))
                        },
                        PinnedAt = pinnedAt
                    };
                }
                else
                {
                    // TODO: Use DiagnosticHandler
                    Console.WriteLine($"[{Current.Line},{Current.Column}] Expected : or {{");
                    return null;
                }
            }

            if (Match(Token.TokenKind.Semicolon, false) && !noSemiCheck)
            {
                Position++;
                return new Declaration
                {
                    Identifier = ident,
                    Kind = ResolveDeclarationKind(@public, @extern, 
                    constant ? Declaration.DeclarationKind.Constant : Declaration.DeclarationKind.Variable),
                    Type = type
                };
            }
            else if (Match(Token.TokenKind.Equal, false) && equalAllowed && !@extern)
            {
                Position++;
                var expr = PF();
                if (!noSemiCheck && !Match(Token.TokenKind.Semicolon)) return null;
                if (!noSemiCheck) Position++;
                return new Declaration
                {
                    Identifier = ident,
                    DefaultValue = new (expr, null, null, null),
                    Kind = ResolveDeclarationKind(@public, @extern,
                        constant ? Declaration.DeclarationKind.Constant : Declaration.DeclarationKind.Variable),
                    Type = type,
                    PinnedAt = pinnedAt,
                };
            }
            else if (!noSemiCheck)
            {
                // TODO: Use DiagnosticHandler
                Console.WriteLine($"[{Current.Line},{Current.Column}] Expected ; or =");
                return null;
            }

            return new Declaration
            {
                Identifier = ident,
                Kind = ResolveDeclarationKind(@public, @extern, 
                    constant ? Declaration.DeclarationKind.Constant : Declaration.DeclarationKind.Variable),
                Type = type,
                PinnedAt = pinnedAt
            };
        }
    }
}
