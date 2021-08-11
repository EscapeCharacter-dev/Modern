using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression
{
    /// <summary>
    /// Sizeof expression.
    /// </summary>
    internal sealed class SizeofExpression : CExpression
    {
        public override IEnumerable<CExpression> Children => Enumerable.Empty<CExpression>();

        public SizeofExpression()
        {
            Kind = ExpressionKind.SizeOf;
        }

        /// <summary>
        /// The type.
        /// </summary>
        public TypeInfo Type { get; init; }
    }
}
