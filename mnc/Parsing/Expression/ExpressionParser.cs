using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression
{
    internal partial class Parser
    {
        /// <summary>
        /// Primary node parser
        /// <paramref name="undefinedDontCare">Whether it should ignore undefined symbols.</paramref>
        /// </summary>
        /// <returns>Returns the expression</returns>
        private CExpression Primary(bool undefinedDontCare = false)
        {
            switch (Current.Kind)
            {
            case Token.TokenKind.IntLiteral:
            case Token.TokenKind.FloatLiteral:
                if (Ahead != null && Ahead.Kind == Token.TokenKind.DotDot)
                {
                    var left = Current;
                    Position += 2;
                    var right = Current;
                    Position++;
                    return new RangeExpression { LeftToken = left, RightToken = right };
                }
                goto case Token.TokenKind.StringLiteral;
            case Token.TokenKind.StringLiteral:
            case Token.TokenKind.Identifier:
                {
                    if (Current.Kind == Token.TokenKind.Identifier)
                    {
                        var str = Current.Representation;
                        if (CurrentScope.TryLookupT(out var _, CurrentScope.CurrentNamespace, str)) { }
                        else if (!CurrentScope.TryLookup(out var _, CurrentScope.CurrentNamespace, str) && !undefinedDontCare)
                        {
                            // TODO: Use DiagnosticHandler
                            Console.WriteLine($"[{Current.Line},{Current.Column}] '{str}' is undefined");
                            Position++; // avoiding deadlocks
                            return null;
                        }
                        Position++;
                        return new LiteralExpression { Token = Before };
                    }
                    Position++;
                    return new LiteralExpression { Token = Before };
                }
            case Token.TokenKind.OpenParenthesis:
                {
                    Position++;
                    var child = ParseExpression();
                    if (!Match(Token.TokenKind.ClosingParenthesis)) return null;
                    Position++;
                    return child;
                }
            default:
                // TODO: Use DiagnosticHandler
                Console.WriteLine($"[{Current.Line},{Current.Column}] Invalid");
                return null;
            }
        }

        /// <summary>
        /// Parses an expression.
        /// </summary>
        /// <returns>The expression.</returns>
        public CExpression ParseExpression()
            => P10();

        /// <summary>
        /// Makes a namespace out of an expression.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns>The namespace.</returns>
        private Namespace FromExpression(CExpression expression)
        {
            if (expression is not BinaryExpression)
                return new Namespace { NodeName = ((LiteralExpression)expression).Token.Representation, Parent = null };
            var bexpression = expression as BinaryExpression;
            return  new Namespace { NodeName = ((LiteralExpression)bexpression.Right)
                .Token.Representation, Parent = FromExpression(bexpression.Left) };
        }

        /// <summary>
        /// Precedence level 0, namespaces
        /// </summary>
        /// <returns>Scope resolved primary</returns>
        private CExpression PN(bool isDeclaring = false)
        {
            var left = Primary(isDeclaring);
            if (isDeclaring && Current.Kind != Token.TokenKind.Cube)
                return new LinearNamespaceAccessExpression
                {
                    Namespace = FromExpression(left)
                };
            if (Current == null || Current.Kind != Token.TokenKind.Cube
                || !(left.Kind == CExpression.ExpressionKind.Literal
                && ((LiteralExpression)left).Token.Kind == Token.TokenKind.Identifier))
                return left;
            while (Current.Kind == Token.TokenKind.Cube)
            {
                Position++;
                if (!Match(Token.TokenKind.Identifier)) return null;
                var right = Primary(isDeclaring);
                left = new BinaryExpression
                {
                    Left = left,
                    Right = right,
                    Kind = CExpression.ExpressionKind.NAE
                };
                if (Current == null || Current.IsTerminator)
                    goto outOfTheLoop;
            }
        outOfTheLoop:
            return new LinearNamespaceAccessExpression
            {
                Operand = ((BinaryExpression)left).Right,
                Namespace = FromExpression(((BinaryExpression)left).Left)
            };
        }

        /// <summary>
        /// Precedence level 1
        /// </summary>
        /// <returns>Parsed expression</returns>
        private CExpression P1()
        {
            var operand = PN();
            if (Current == null || Current.IsTerminator)
                return operand;
            switch (Current.Kind)
            {
            case Token.TokenKind.PlusPlus:
                Position++;
                return new UnaryExpression { Operand = operand, Kind = CExpression.ExpressionKind.PostfixIncrement };
            case Token.TokenKind.MinusMinus:
                Position++;
                return new UnaryExpression { Operand = operand, Kind = CExpression.ExpressionKind.PostfixDecrement };
            case Token.TokenKind.OpenParenthesis:
                Position++;
                {
                    var list = new List<CExpression>();
                    while (Current.Kind != Token.TokenKind.ClosingParenthesis)
                    {
                        if (Current.Kind == Token.TokenKind.Comma) // default argument
                        {
                            list.Add(new LiteralExpression { Kind = CExpression.ExpressionKind.Nothing, Token = Current });
                            Position++;
                            goto endOfLoop;
                        }
                        var expr = PF(); // Compounds require parentheses
                        list.Add(expr);
                        if (Current.Kind == Token.TokenKind.Comma)
                        {
                            Position++;
                            if (Current.Kind == Token.TokenKind.ClosingParenthesis)
                            {
                                // TODO: Use DiagnosticHandler
                                Console.WriteLine($"[{Current.Line},{Current.Column}] A comma cannot be followed by a closing parenthesis");
                                return null;
                            }
                        }

endOfLoop:
                        if (Current == null || Current.IsTerminator)
                            break;
                    }
                    Position++;
                    return new FunctionCallExpression { ChildrenExpressions = list, Operand = operand };
                }
            case Token.TokenKind.Dot:
                {
                    while (Current.Kind == Token.TokenKind.Dot)
                    {
                        Position++;
                        var right = Primary(true);
                        operand = new BinaryExpression
                        {
                            Left = operand,
                            Right = right,
                            Kind = CExpression.ExpressionKind.MemberAccess
                        };
                        if (Current == null || Current.IsTerminator)
                            goto outOfLoop;
                    }
                outOfLoop:
                    var bOp = operand as BinaryExpression;
                    if (bOp.Left is LiteralExpression l && bOp.Right is LiteralExpression r)
                    {
                        // maybe enum?
                        if (CurrentScope.TryLookupT(out var @enum, CurrentScope.CurrentNamespace, l.Token.Representation))
                        {
                            if (@enum.Kind == Declaration.DeclarationKind.Enum)
                            {
                                var enumDecl = @enum.GetEnumDecl(r.Token.Representation);
                                if (enumDecl == (EnumDecl)null)
                                {
                                    // TODO: Use DiagnosticHandler
                                    Console.WriteLine($"[{Before.Line},{Before.Column}] Enum member '{r.Token.Representation}' couldn't be found");
                                    return null;
                                }
                                return new LiteralExpression
                                {
                                    Token = new Token
                                    {
                                        Source = "ENUM",
                                        Kind = Token.TokenKind.IntLiteral,
                                        Data = enumDecl.Index
                                    }
                                };
                            }
                        }
                    }
                    return operand;
                }
            case Token.TokenKind.Arrow:
                {
                    while (Current.Kind == Token.TokenKind.Arrow)
                    {
                        Position++;
                        var right = Primary(true);
                        operand = new BinaryExpression
                        {
                            Left = operand,
                            Right = right,
                            Kind = CExpression.ExpressionKind.DereferencedMemberAccess
                        };
                        if (Current == null || Current.IsTerminator)
                            return operand;
                    }
                    return operand;
                }
            default:
                return operand;
            }
        }
        
        /// <summary>
        /// Precedence level 2
        /// </summary>
        /// <returns>Parsed expression</returns>
        private CExpression P2()
        {
            switch (Current.Kind)
            {
            case Token.TokenKind.PlusPlus:
                Position++;
                return new UnaryExpression { Kind = CExpression.ExpressionKind.PrefixIncrement, Operand = P2() };
            case Token.TokenKind.MinusMinus:
                Position++;
                return new UnaryExpression { Kind = CExpression.ExpressionKind.PrefixDecrement, Operand = P2() };
            case Token.TokenKind.Plus:
                Position++;
                return new UnaryExpression { Kind = CExpression.ExpressionKind.Plus, Operand = P2() };
            case Token.TokenKind.Minus:
                Position++;
                return new UnaryExpression { Kind = CExpression.ExpressionKind.Minus, Operand = P2() };
            case Token.TokenKind.Exclamation:
                Position++;
                return new UnaryExpression { Kind = CExpression.ExpressionKind.LogicalNot, Operand = P2() };
            case Token.TokenKind.Tilde:
                Position++;
                return new UnaryExpression { Kind = CExpression.ExpressionKind.BitwiseNot, Operand = P2() };
            case Token.TokenKind.Sizeof:
                Position++;
                return new SizeofExpression { Type = ParseType() };
            case Token.TokenKind.Typeof:
                Position++;
                return new UnaryExpression { Kind = CExpression.ExpressionKind.TypeOf, Operand = P2() };
            case Token.TokenKind.At:
                Position++;
                return new UnaryExpression { Kind = CExpression.ExpressionKind.AddressOf, Operand = P2() };
            case Token.TokenKind.Dollar:
                Position++;
                return new UnaryExpression { Kind = CExpression.ExpressionKind.Dereference, Operand = P2() };
            case Token.TokenKind.OpenParenthesis:
                if (Ahead.Kind.Matches(
                    Token.TokenKind.Void,
                    Token.TokenKind.Byte,
                    Token.TokenKind.SByte,
                    Token.TokenKind.Char,
                    Token.TokenKind.UChar,
                    Token.TokenKind.Bool,
                    Token.TokenKind.Short,
                    Token.TokenKind.UShort,
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
                    Token.TokenKind.Function
                ))
                {
                    Position++;
                    var type = ParseType();
                    if (!Match(Token.TokenKind.ClosingParenthesis)) return null;
                    Position++;
                    return new CastExpression
                    {
                        Operand = P2(),
                        Type = type
                    };
                }
                return P1();
            default:
                return P1();
            }
        }
    
        /// <summary>
        /// Precedence level 3
        /// </summary>
        /// <returns>Parsed expression</returns>
        private CExpression P3()
        {
            var left = P2();
            if (Current == null || Current.IsTerminator)
                return left;

            while (Current.Kind.Matches(Token.TokenKind.Star, Token.TokenKind.Slash, Token.TokenKind.Percent))
            {
                var op = Current.Kind == Token.TokenKind.Star ? CExpression.ExpressionKind.Multiply :
                         Current.Kind == Token.TokenKind.Slash ? CExpression.ExpressionKind.Divide :
                         CExpression.ExpressionKind.Remainder;
                Position++;
                var right = P2();
                left = new BinaryExpression { Kind = op, Left = left, Right = right };

                if (Current == null || Current.IsTerminator)
                    break;
            }
            return left;
        }

        /// <summary>
        /// Precedence level 4
        /// </summary>
        /// <returns>Parsed expression</returns>
        private CExpression P4()
        {
            var left = P3();
            if (Current == null || Current.IsTerminator)
                return left;

            while (Current.Kind == Token.TokenKind.Plus || Current.Kind == Token.TokenKind.Minus)
            {
                var op = Current.Kind == Token.TokenKind.Plus ? CExpression.ExpressionKind.Add : CExpression.ExpressionKind.Sub;
                Position++;
                var right = P3();
                left = new BinaryExpression { Kind = op, Left = left, Right = right };

                if (Current == null || Current.IsTerminator)
                    break;
            }
            return left;
        }

        /// <summary>
        /// Precedence level 5
        /// </summary>
        /// <returns>Parsed expression</returns>
        private CExpression P5()
        {
            var left = P4();
            if (Current == null || Current.IsTerminator)
                return left;

            while (Current.Kind == Token.TokenKind.LeftArrows || Current.Kind == Token.TokenKind.RightArrows)
            {
                var op = Current.Kind == Token.TokenKind.LeftArrows ? CExpression.ExpressionKind.BitwiseLeftShift :
                    CExpression.ExpressionKind.BitwiseRightShift;
                Position++;
                var right = P4();
                left = new BinaryExpression { Kind = op, Left = left, Right = right };

                if (Current == null || Current.IsTerminator)
                    break;
            }
            return left;
        }

        /// <summary>
        /// Precedence level 6
        /// </summary>
        /// <returns>Parsed expression</returns>
        private CExpression P6()
        {
            var left = P5();
            if (Current == null || Current.IsTerminator)
                return left;

            while (Current.Kind.Matches(Token.TokenKind.LeftArrow, Token.TokenKind.RightArrow,
                                        Token.TokenKind.LeftArrowEqual, Token.TokenKind.RightArrowEqual,
                                        Token.TokenKind.Between))
            {
                var op = Current.Kind == Token.TokenKind.LeftArrow ? CExpression.ExpressionKind.Lower :
                         Current.Kind == Token.TokenKind.RightArrow ? CExpression.ExpressionKind.Greater :
                         Current.Kind == Token.TokenKind.LeftArrowEqual ? CExpression.ExpressionKind.LowerEqual :
                         Current.Kind == Token.TokenKind.Between ? CExpression.ExpressionKind.Between :
                         CExpression.ExpressionKind.GreaterEqual;
                Position++;
                CExpression right;
                if (op == CExpression.ExpressionKind.Between)
                    right = PN();
                else
                    right = P5();
                left = new BinaryExpression { Kind = op, Left = left, Right = right };
                if (Current == null || Current.IsTerminator)
                    return left;
            }

            return left;
        }

        /// <summary>
        /// Precedence level 7
        /// </summary>
        /// <returns>Parsed expression</returns>
        private CExpression P7()
        {
            var left = P6();
            if (Current == null || Current.IsTerminator)
                return left;

            while (Current.Kind == Token.TokenKind.Equals || Current.Kind == Token.TokenKind.ExclamationEqual)
            {
                var op = Current.Kind == Token.TokenKind.Equals ? CExpression.ExpressionKind.Equal :
                    CExpression.ExpressionKind.NotEqual;
                Position++;
                var right = P6();
                left = new BinaryExpression { Kind = op, Left = left, Right = right };

                if (Current == null || Current.IsTerminator)
                    break;
            }
            return left;
        }

        /// <summary>
        /// Precedence level 8
        /// </summary>
        /// <returns>Parsed expression</returns>
        private CExpression P8()
        {
            var left = P7();
            if (Current == null || Current.IsTerminator)
                return left;

            while (Current.Kind == Token.TokenKind.Ampersand)
            {
                Position++;
                var right = P7();
                left = new BinaryExpression { Kind = CExpression.ExpressionKind.BitwiseAnd, Left = left, Right = right };

                if (Current == null || Current.IsTerminator)
                    break;
            }
            return left;
        }

        /// <summary>
        /// Precedence level 9
        /// </summary>
        /// <returns>Parsed expression</returns>
        private CExpression P9()
        {
            var left = P8();
            if (Current == null || Current.IsTerminator)
                return left;

            while (Current.Kind == Token.TokenKind.Caret)
            {
                Position++;
                var right = P9();
                left = new BinaryExpression { Kind = CExpression.ExpressionKind.BitwiseXOr, Left = left, Right = right };

                if (Current == null || Current.IsTerminator)
                    break;
            }
            return left;
        }

        /// <summary>
        /// Precedence level 10
        /// </summary>
        /// <returns>Parsed expression</returns>
        private CExpression PA()
        {
            var left = P9();
            if (Current == null || Current.IsTerminator)
                return left;

            while (Current.Kind == Token.TokenKind.Pipe)
            {
                Position++;
                var right = P9();
                left = new BinaryExpression { Kind = CExpression.ExpressionKind.BitwiseOr, Left = left, Right = right };

                if (Current == null || Current.IsTerminator)
                    break;
            }
            return left;
        }

        /// <summary>
        /// Precedence level 11
        /// </summary>
        /// <returns>Parsed expression</returns>
        private CExpression PB()
        {
            var left = PA();
            if (Current == null || Current.IsTerminator)
                return left;

            while (Current.Kind == Token.TokenKind.Ampersands)
            {
                Position++;
                var right = PA();
                left = new BinaryExpression { Kind = CExpression.ExpressionKind.LogicalAnd, Left = left, Right = right };

                if (Current == null || Current.IsTerminator)
                    break;
            }
            return left;
        }

        private CExpression PC()
        {
            var left = PB();
            if (Current == null || Current.IsTerminator)
                return left;

            while (Current.Kind == Token.TokenKind.Carets)
            {
                Position++;
                var right = PB();
                left = new BinaryExpression { Kind = CExpression.ExpressionKind.LogicalXor, Left = left, Right = right };

                if (Current == null || Current.IsTerminator)
                    break;
            }
            return left;
        }

        /// <summary>
        /// Precedence level 12
        /// </summary>
        /// <returns>Parsed expression</returns>
        private CExpression PD()
        {
            var left = PC();
            if (Current == null || Current.IsTerminator)
                return left;

            while (Current.Kind == Token.TokenKind.Pipes)
            {
                Position++;
                var right = PC();
                left = new BinaryExpression { Kind = CExpression.ExpressionKind.LogicalOr, Left = left, Right = right };

                if (Current == null || Current.IsTerminator)
                    break;
            }
            return left;
        }

        /// <summary>
        /// Precedence level 14
        /// </summary>
        /// <returns>Parsed expression</returns>
        private CExpression PE()
        {
            var left = PD();
            if (Current == null || Current.IsTerminator)
                return left;

            while (Current.Kind == Token.TokenKind.QuestionMark)
            {
                Position++;
                var middle = PE();
                if (!Match(Token.TokenKind.Colon, false)) return null;
                Position++;
                var right = PE();
                left = new TernaryExpression { Left = left, Middle = middle, Right = right, Kind = CExpression.ExpressionKind.Ternary };

                if (Current == null || Current.IsTerminator)
                    break;
            }
            return left;
        }

        /// <summary>
        /// Precedence level 15
        /// </summary>
        /// <returns>Parsed expression</returns>
        private CExpression PF()
        {
            var left = PE();
            if (Current == null || Current.IsTerminator)
                return left;

            while (Current.Kind.Matches(
                Token.TokenKind.Equal, Token.TokenKind.PlusEqual, Token.TokenKind.MinusEqual, Token.TokenKind.StarEqual,
                Token.TokenKind.SlashEqual, Token.TokenKind.PercentEqual, Token.TokenKind.LeftArrowsEqual,
                Token.TokenKind.RightArrowsEqual, Token.TokenKind.AmpersandEqual, Token.TokenKind.CaretEqual,
                Token.TokenKind.PipeEqual
                ))
            {
                var op = Current.Kind == Token.TokenKind.Equal ? CExpression.ExpressionKind.Assign :
                         Current.Kind == Token.TokenKind.PlusEqual ? CExpression.ExpressionKind.AddAssign :
                         Current.Kind == Token.TokenKind.MinusEqual ? CExpression.ExpressionKind.SubAssign :
                         Current.Kind == Token.TokenKind.StarEqual ? CExpression.ExpressionKind.MulAssign :
                         Current.Kind == Token.TokenKind.SlashEqual ? CExpression.ExpressionKind.DivAssign :
                         Current.Kind == Token.TokenKind.PercentEqual ? CExpression.ExpressionKind.ModAssign :
                         Current.Kind == Token.TokenKind.LeftArrowsEqual ? CExpression.ExpressionKind.BitwiseLeftShiftAssign :
                         Current.Kind == Token.TokenKind.RightArrowsEqual ? CExpression.ExpressionKind.BitwiseRightShiftAssign :
                         Current.Kind == Token.TokenKind.AmpersandEqual ? CExpression.ExpressionKind.AndAssign :
                         Current.Kind == Token.TokenKind.CaretEqual ? CExpression.ExpressionKind.XOrAssign :
                         CExpression.ExpressionKind.OrAssign;
                Position++;
                var right = PF();
                left = new BinaryExpression { Left = left, Right = right, Kind = op };

                if (Current == null || Current.IsTerminator)
                    break;
            }
            return left;
        }

        /// <summary>
        /// Precedence level 16
        /// </summary>
        /// <returns>Parsed expression</returns>
        private CExpression P10()
        {
            var left = PF();
            if (Current == null || Current.IsTerminator)
                return left;

            while (Current.Kind == Token.TokenKind.Comma)
            {
                Position++;
                var right = PF();
                left = new BinaryExpression { Kind = CExpression.ExpressionKind.Compound, Left = left, Right = right };

                if (Current == null || Current.IsTerminator)
                    break;
            }
            return left;
        }
    }
}
