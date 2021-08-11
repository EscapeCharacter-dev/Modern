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
    internal sealed class RangeExpression : CExpression
    {
        /// <summary>
        /// The left token.
        /// </summary>
        public Token LeftToken { get; init; }

        /// <summary>
        /// The right token.
        /// </summary>
        public Token RightToken { get; init; }

        public RangeExpression()
        {
            Kind = ExpressionKind.Range;
        }

        public override string ToString()
        {
            return $"{LeftToken.Representation}..{RightToken.Representation}";
        }

        public override IEnumerable<CExpression> Children => Enumerable.Empty<CExpression>();
    }
}
