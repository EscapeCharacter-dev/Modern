using mnc.Parsing.Expression.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression
{
    /// <summary>
    /// The statement parser.
    /// </summary>
    internal sealed partial class Parser
    {
        /// <summary>
        /// Parses a statement.
        /// </summary>
        /// <returns>The statement.</returns>
        public Statement ParseStatement()
        {
            Statement ret;
            if (Current == null)
            {
                if (Before == null) return null;
                // TODO: Use DiagnosticHandler
                Console.WriteLine($"[{Before.Line},{Before.Column}] Premature EOF, expected statement");
                return null;
            }
            switch (Current.Kind)
            {
            case Token.TokenKind.Namespace: ret = Namespace(); break;
            case Token.TokenKind.OpenBracket: ret = Block(); break;
            case Token.TokenKind.Break:
                {
                    Position++;
                    if (!CurrentScope.IsLoopOrSwitch)
                    {
                        // TODO: Use DiagnosticHandler
                        Console.WriteLine($"[{Before.Line},{Before.Column}] break can only be used in a loop or a switch statement");
                        return null;
                    }
                    ret = new BreakStatement();
                    if (!Match(Token.TokenKind.Semicolon)) return null;
                    Position++;
                    break;
                }
            case Token.TokenKind.Continue:
                {
                    Position++;
                    if (!CurrentScope.IsLoopOrSwitch)
                    {
                        // TODO: Use DiagnosticHandler
                        Console.WriteLine($"[{Before.Line},{Before.Column}] break can only be used in a loop or a switch statement");
                        return null;
                    }
                    ret = new ContinueStatement();
                    if (!Match(Token.TokenKind.Semicolon)) return null;
                    Position++;
                    break;
                }
            case Token.TokenKind.Do: ret = DoWhile(); break;
            case Token.TokenKind.Semicolon: Position++; ret = new EmptyStatement(); break;
            case Token.TokenKind.Goto: ret = Goto(); break;
            case Token.TokenKind.For: ret = For(); break;
            case Token.TokenKind.If: ret = If(); break;
            case Token.TokenKind.Switch: ret = Switch(); break;
            case Token.TokenKind.While: ret = While(); break;
            case Token.TokenKind.Return: ret = Return(); break;
            case Token.TokenKind.Const: // Constant/Function/Variable declarations
            case Token.TokenKind.Void:
            case Token.TokenKind.Function:
            case Token.TokenKind.Byte:
            case Token.TokenKind.SByte:
            case Token.TokenKind.Char:
            case Token.TokenKind.UChar:
            case Token.TokenKind.Bool:
            case Token.TokenKind.Short:
            case Token.TokenKind.UShort:
            case Token.TokenKind.WChar:
            case Token.TokenKind.Int:
            case Token.TokenKind.UInt:
            case Token.TokenKind.Long:
            case Token.TokenKind.ULong:
            case Token.TokenKind.Huge:
            case Token.TokenKind.UHuge:
            case Token.TokenKind.Float:
            case Token.TokenKind.Double:
            case Token.TokenKind.LDouble:
            case Token.TokenKind.Extern:
            case Token.TokenKind.Public:
            case Token.TokenKind.Identifier:
                {
                    var pos = (Current.Line, Current.Column);
                    if (Current.Kind == Token.TokenKind.Identifier &&
                        !CurrentScope.TryLookupT(out var _, CurrentScope.CurrentNamespace, Current.Representation))
                    {
                        var expr = ParseExpression();
                        if (!Match(Token.TokenKind.Semicolon)) return null;
                        Position++;
                        return new ExpressionStatement { Expression = expr };
                    }
                    var decl = ParseDeclaration();
                    if (decl == null)
                    {
                        // TODO: Use DiagnosticHandler
                        Console.WriteLine($"[{pos.Line},{pos.Column}] Invalid declaration");
                        return null;
                    }
                    if (!CurrentScope.TryDeclare(decl))
                    {
                        // TODO: Use DiagnosticHandler
                        Console.WriteLine($"[{pos.Line},{pos.Column}] Declaration failed");
                        return null;
                    }
                    ret = new DeclarationStatement { Declaration = decl };
                    break;
                }
            case Token.TokenKind.Enum: // Templated declarations
            case Token.TokenKind.Struct:
            case Token.TokenKind.Union:
                {
                    var pos = (Current.Line, Current.Column);
                    var tDecl = ParseDeclaration();
                    if (tDecl == null)
                    {
                        // TODO: Use DiagnosticHandler
                        Console.WriteLine($"[{pos.Line},{pos.Column}] Invalid enum declaration");
                        return null;
                    }
                    if (!CurrentScope.TryDeclareT(tDecl))
                    {
                        // TODO: Use DiagnosticHandler
                        Console.WriteLine($"[{pos.Line},{pos.Column}] Templated declaration failed");
                        return null;
                    }
                    ret = new DeclarationStatement { Declaration = tDecl };
                    break;
                }
            default:
                {
                    ret = new ExpressionStatement { Expression = ParseExpression() };
                    if (!Match(Token.TokenKind.Semicolon)) return null;
                    Position++;
                    break;
                }
            }
            if (ret is not DeclarationStatement && ret is not NamespaceStatement && CurrentScope.Parent == null)
            {
                // TODO: Use DiagnosticHandler
                Console.WriteLine($"[{Before.Line},{Before.Column}] Statements can only appear in functions");
                return null;
            }
            return ret;
        }

        /// <summary>
        /// Parses an if statement.
        /// </summary>
        /// <returns>The if statement.</returns>
        private IfStatement If()
        {
            Position++; // if keyword
            var constant = false;
            if (Match(Token.TokenKind.Const, false))
            {
                constant = true;
                Position++;
            }
            if (!Match(Token.TokenKind.OpenParenthesis)) return null;
            Position++;
            var expression = ParseExpression();
            if (!Match(Token.TokenKind.ClosingParenthesis)) return null;
            Position++;
            var statement = ParseStatement();
            if (Match(Token.TokenKind.Else, false))
            {
                Position++;
                var elseStatement = ParseStatement();
                return new IfStatement { Condition = expression, Then = statement, Else = elseStatement, IsConst = constant };
            }
            return new IfStatement { Condition = expression, Then = statement, IsConst = constant };
        }
    
        /// <summary>
        /// Parses a while statement.
        /// </summary>
        /// <returns>The while statement.</returns>
        private WhileStatement While()
        {
            Position++; // while keyword
            if (!Match(Token.TokenKind.OpenParenthesis)) return null;
            Position++;
            var expression = ParseExpression();
            if (!Match(Token.TokenKind.ClosingParenthesis)) return null;
            Position++;
            ScopePush(true);
            var statement = ParseStatement();
            ScopePop();
            return new WhileStatement { Condition = expression, Then = statement };
        }
    
        /// <summary>
        /// Parses a do...while(...) statement
        /// </summary>
        /// <returns>The do while statement.</returns>
        private DoWhileStatement DoWhile()
        {
            Position++; // do keyword
            ScopePush(true);
            var statement = ParseStatement();
            ScopePop();
            if (!Match(Token.TokenKind.While)) return null;
            Position++;
            if (!Match(Token.TokenKind.OpenParenthesis)) return null;
            Position++;
            var expr = ParseExpression();
            if (!Match(Token.TokenKind.ClosingParenthesis)) return null;
            Position++;
            return new DoWhileStatement { Condition = expr, Do = statement };
        }
    
        /// <summary>
        /// Block statement
        /// </summary>
        /// <returns>The block statement.</returns>
        private BlockStatement Block()
        {
            ScopePush();
            Position++; // {
            var list = new List<Statement>();
            while (!Match(Token.TokenKind.ClosingBracket, false))
            {
                if (Current == null)
                {
                    // TODO: Use DiagnosticHandler
                    Console.WriteLine("Expected matching closing bracket");
                    return null;
                }
                list.Add(ParseStatement());
            }
            Position++;
            ScopePop();
            return new BlockStatement { Block = list };
        }

        /// <summary>
        /// Goto statement
        /// </summary>
        /// <returns>The goto statement.</returns>
        private GotoStatement Goto()
        {
            Position++; // goto keyword
            var expression = ParseExpression();
            if (!Match(Token.TokenKind.Semicolon)) return null;
            Position++;
            return new GotoStatement { Destination = expression };
        }

        /// <summary>
        /// Return statement
        /// </summary>
        /// <returns>The return statement.</returns>
        private ReturnStatement Return()
        {
            Position++; // return keyword
            if (Match(Token.TokenKind.Semicolon, false))
            {
                Position++;
                return new ReturnStatement { ReturnExpression = null };
            }
            var expression = ParseExpression();
            if (!Match(Token.TokenKind.Semicolon)) return null;
            Position++;
            return new ReturnStatement { ReturnExpression = expression };
        }

        /// <summary>
        /// The using statement.
        /// </summary>
        /// <returns>An instance to a parsed using statement.</returns>
        private UsingStatement Using()
        {
            Position++; // using keyword
            if (!Match(Token.TokenKind.Identifier)) return null;
            var moduleName = Current.Representation;
            Position++;
            // TODO: Use CompilationUnit
            return null;
        }

        /// <summary>
        /// The namespace statement.
        /// </summary>
        /// <returns>An instance to a parsed namespace statement.</returns>
        private NamespaceStatement Namespace()
        {
            if (CurrentScope.Parent != null)
            {
                // TODO: Use DiagnosticHandler
                Console.WriteLine($"[{Current.Line},{Current.Column}] Namespace statements must be scopeless");
                return null;
            }
            Position++;
            var lnae = PN(true);
            if (lnae is not LinearNamespaceAccessExpression)
            {
                // TODO: Use DiagnosticHandler
                Console.WriteLine($"[{Current.Line},{Current.Column}] Expected a namespace, got {lnae.Kind}");
                return null;
            }
            var @namespace = (lnae as LinearNamespaceAccessExpression).Namespace;
            CurrentScope.CurrentNamespace = @namespace;
            if (!Match(Token.TokenKind.Semicolon)) return null;
            Position++;
            return new NamespaceStatement { Namespace = @namespace };
        }
    
        /// <summary>
        /// Parses a switch statement.
        /// </summary>
        /// <returns>The switch statement instance.</returns>
        private SwitchStatement Switch()
        {
            Position++; // switch keyword
            if (!Match(Token.TokenKind.OpenParenthesis)) return null;
            Position++;
            var forExpr = ParseExpression();
            if (!Match(Token.TokenKind.ClosingParenthesis)) return null;
            Position++;
            if (!Match(Token.TokenKind.OpenBracket)) return null;
            Position++;
            ScopePush(true);
            var list = new List<(CExpression Against, IEnumerable<Statement> Statements)>();
            var startIndex = -1;
            var endIndex = -1;
            var subswitchIndex = new List<(int, int)>();
            while (Current.Kind != Token.TokenKind.ClosingBracket)
            {
                if (Current.Kind == Token.TokenKind.Case)
                {
                    Position++; // case keyword
                    var caseExpr = ParseExpression();
                    if (!Match(Token.TokenKind.Colon)) return null;
                    Position++;
                    var statements = new List<Statement>();
                    while (!Current.Kind.Matches(
                        Token.TokenKind.Case, Token.TokenKind.Default, 
                        Token.TokenKind.Start, Token.TokenKind.End,
                        Token.TokenKind.ClosingBracket))
                        statements.Add(ParseStatement());
                    list.Add((caseExpr, statements));
                }
                else if (Current.Kind == Token.TokenKind.Default)
                {
                    Position++; // default keyword
                    if (!Match(Token.TokenKind.Colon)) return null;
                    Position++;
                    var statements = new List<Statement>();
                    while (!Current.Kind.Matches(
                        Token.TokenKind.Case, Token.TokenKind.Default,
                        Token.TokenKind.Start, Token.TokenKind.End,
                        Token.TokenKind.ClosingBracket))
                        statements.Add(ParseStatement());
                    list.Add((null, statements));
                }
                else if (Current.Kind == Token.TokenKind.Start)
                {
                    Position++; // start keyword
                    if (!Match(Token.TokenKind.Colon)) return null;
                    Position++;
                    var statements = new List<Statement>();
                    while (!Current.Kind.Matches(
                        Token.TokenKind.Case, Token.TokenKind.Default,
                        Token.TokenKind.Start, Token.TokenKind.End,
                        Token.TokenKind.ClosingBracket))
                        statements.Add(ParseStatement());
                    startIndex = list.Count == 0 ? 0 : list.Count - 1;
                    list.Add((null, statements));
                }
                else if (Current.Kind == Token.TokenKind.End)
                {
                    Position++; // end keyword
                    endIndex = list.Count == 0 ? 0 : list.Count - 1;
                    if (startIndex == -1)
                    {
                        // TODO: Use DiagnosticHandler
                        Console.WriteLine($"[{Before.Line},{Before.Column}] Cannot find matching subswitch start");
                        return null;
                    }
                    if (!Match(Token.TokenKind.Colon)) return null;
                    Position++;
                    var statements = new List<Statement>();
                    while (!Current.Kind.Matches(
                        Token.TokenKind.Case, Token.TokenKind.Default,
                        Token.TokenKind.Start, Token.TokenKind.End,
                        Token.TokenKind.ClosingBracket))
                        statements.Add(ParseStatement());
                    list.Add((null, statements));
                    subswitchIndex.Add((startIndex, endIndex));
                    startIndex = -1;
                    endIndex = -1;
                }
            }
            Position++;
            if (startIndex != -1 && endIndex == -1)
            {
                // TODO: Use DiagnosticHandler
                Console.WriteLine($"[{Before.Line},{Before.Column}] Cannot find matching subswitch end");
                return null;
            }
            ScopePop();
            return new SwitchStatement
            {
                Switch = list,
                SubSwitches = subswitchIndex,
                Source = forExpr
            };
        }

        /// <summary>
        /// For statement
        /// </summary>
        /// <returns>The for statement.</returns>
        private ForStatement For()
        {
            Position++; // for keyword
            if (!Match(Token.TokenKind.OpenParenthesis)) return null;
            Position++;
            ScopePush(true);
            var init = ParseStatement();
            var cond = ParseExpression();
            if (!Match(Token.TokenKind.Semicolon)) return null;
            Position++;
            var then = ParseExpression();
            if (!Match(Token.TokenKind.ClosingParenthesis)) return null;
            Position++;
            var stat = ParseStatement();
            ScopePop();
            return new ForStatement { Condition = cond, Init = init, Do = stat, Iteration = then };
        }
    }
}
