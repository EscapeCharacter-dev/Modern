using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression
{
    /// <summary>
    /// A unary expression (with only one operand) with a type
    /// </summary>
    internal sealed class CastExpression : CExpression
    {
        /// <summary>
        /// The operand.
        /// </summary>
        public CExpression Operand { get; init; }

        /// <summary>
        /// The type.
        /// </summary>
        public TypeInfo Type { get; init; }

        public CastExpression()
        {
            Kind = ExpressionKind.Cast;
        }

        public override string ToString()
        {
            return $"({Type})" + base.ToString();
        }

        public override IEnumerable<CExpression> Children
        {
            get
            {
                yield return Operand;
            }
        }
    }
}
