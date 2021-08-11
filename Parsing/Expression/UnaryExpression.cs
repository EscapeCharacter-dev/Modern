using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression
{
    /// <summary>
    /// A unary expression (with only one operand)
    /// </summary>
    internal class UnaryExpression : CExpression
    {
        /// <summary>
        /// The operand.
        /// </summary>
        public CExpression Operand { get; init; }

        public override IEnumerable<CExpression> Children
        {
            get
            {
                yield return Operand;
            }
        }
    }
}
