using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression
{
    /// <summary>
    /// A function call
    /// </summary>
    internal class FunctionCallExpression : CExpression
    {
        /// <summary>
        /// The operand.
        /// </summary>
        public CExpression Operand { get; init; }

        /// <summary>
        /// The parameters.
        /// </summary>
        public IEnumerable<CExpression> ChildrenExpressions { get; init; } = Enumerable.Empty<CExpression>();

        public FunctionCallExpression()
        {
            Kind = ExpressionKind.FunctionCall;
        }

        public override IEnumerable<CExpression> Children
        {
            get
            {
                yield return Operand;
                foreach (var child in ChildrenExpressions)
                    yield return child;
            }
        }
    }
}
