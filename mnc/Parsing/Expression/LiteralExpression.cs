using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression
{
    /// <summary>
    /// An expression wth a token associated.
    /// </summary>
    internal sealed class LiteralExpression : CExpression
    {
        /// <summary>
        /// The token.
        /// </summary>
        public Token Token { get; init; }

        public LiteralExpression()
        {
            Kind = ExpressionKind.Literal;
        }

        public override string ToString()
        {
            return $"{Token.Representation}";
        }

        public override IEnumerable<CExpression> Children => Enumerable.Empty<CExpression>();
    }
}
