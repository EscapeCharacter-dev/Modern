using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression.Statements
{
    /// <summary>
    /// A return statement.
    /// </summary>
    internal sealed class ReturnStatement : Statement
    {
        public override StatementKind Kind => StatementKind.Return;

        /// <summary>
        /// The expression to return.
        /// </summary>
        public CExpression ReturnExpression { get; init; } = null;

        public override string ToString()
        {
            return base.ToString() + $" {ReturnExpression}";
        }
    }
}
