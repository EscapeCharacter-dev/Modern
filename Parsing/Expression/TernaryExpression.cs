using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression
{
    /// <summary>
    /// A ternary expression (with three operands)
    /// </summary>
    internal sealed class TernaryExpression : CExpression
    {
        /// <summary>
        /// The left operand.
        /// </summary>
        public CExpression Left { get; init; }

        /// <summary>
        /// The middle operand.
        /// </summary>
        public CExpression Middle { get; init; }

        /// <summary>
        /// The right operand.
        /// </summary>
        public CExpression Right { get; init; }

        public override IEnumerable<CExpression> Children
        {
            get
            {
                yield return Left;
                yield return Middle;
                yield return Right;
            }
        }
    }
}
