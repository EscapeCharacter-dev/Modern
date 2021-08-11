using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression
{
    internal partial class Parser
    {
        private TypeInfo ParseType()
        {
            if (!Current.Kind.Matches(
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
                Token.TokenKind.Function,
                Token.TokenKind.Identifier
                ))
                return null;
            TypeInfo.TypeKind kind;
            Declaration _oDeclaration = null;
            if (Current.Kind == Token.TokenKind.Identifier)
            {
                if (!CurrentScope.TryLookupT(out var decl, CurrentScope.CurrentNamespace, Current.Representation))
                {
                    // TODO: Use DiagnosticHandler
                    Console.WriteLine($"[{Current.Line},{Current.Column}] Cannot find struct/union '{Current.Representation}'");
                    Position++;
                    return null;
                }
                if (decl.Kind != Declaration.DeclarationKind.Struct && decl.Kind != Declaration.DeclarationKind.Union)
                {
                    // TODO: Use DiagnosticHandler
                    Console.WriteLine($"[{Current.Line},{Current.Column}] '{Current.Representation}' is not a valid structure or union");
                    Position++;
                    return null;
                }
                kind = decl.Kind == Declaration.DeclarationKind.Union ? TypeInfo.TypeKind.Union : TypeInfo.TypeKind.Structure;
                _oDeclaration = decl;
            }
            else
                kind = (TypeInfo.TypeKind)(Current.Kind - Token.TokenKind.Void);
            kind = Current.Kind == Token.TokenKind.Function ? TypeInfo.TypeKind.Function : kind;
            var type = new TypeInfo
            {
                Kind = kind,
                Expression = kind == TypeInfo.TypeKind.Union || kind == TypeInfo.TypeKind.Structure ?
                new LiteralExpression
                {
                    Kind = CExpression.ExpressionKind.Literal,
                    Token = new Token 
                    {
                        Kind = Token.TokenKind.Identifier,
                        Source = "TYPE_RESOLVE_STRUCT_UNION",
                        Data = _oDeclaration.Identifier
                    }
                } : null
            };
            if (kind == TypeInfo.TypeKind.Function)
            {
                Position++;
                if (Current == null || Current.IsTerminator)
                {
                    // TODO: Use DiagnosticHandler
                    Console.WriteLine($"[{Current.Line},{Current.Column}] Expected return type");
                    return null;
                }
                var returnType = ParseType();
                if (Current.Kind != Token.TokenKind.OpenParenthesis)
                {
                    // TODO: Use DiagnosticHandler
                    Console.WriteLine($"[{Current.Line},{Current.Column}] Expected an open parenthesis");
                    return null;
                }
                Position++;
                var args = new List<TypeInfo>();
                while (Current.Kind != Token.TokenKind.ClosingParenthesis)
                {
                    args.Add(ParseType());
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
                type = new TypeInfo
                {
                    Kind = TypeInfo.TypeKind.Function,
                    Children = new[] { returnType }.Union(args)
                };
            }
            Position++;
            while (Current.Kind == Token.TokenKind.At || Current.Kind == Token.TokenKind.OpenSquareBracket)
            {
                if (Current.Kind == Token.TokenKind.OpenSquareBracket)
                {
                    Position++;
                    var expression = ParseExpression();
                    if (Current.Kind != Token.TokenKind.ClosingSquareBracket)
                    {
                        // TODO: Use DiagnosticHandler
                        Console.WriteLine($"[{Current.Line},{Current.Column}] Expected ]");
                        return null;
                    }
                    Position++;
                    type = new TypeInfo
                    {
                        Expression = expression,
                        Kind = expression.Kind == CExpression.ExpressionKind.Literal ? TypeInfo.TypeKind.FixedLengthArray :
                            TypeInfo.TypeKind.DynamicLengthArray,
                        Children = new[] { type }
                    };
                }
                else
                {
                    Position++;
                    type = new TypeInfo
                    {
                        Kind = TypeInfo.TypeKind.Pointer,
                        Children = new[] { type }
                    };
                }
            }
            return type;
        }
    }
}
