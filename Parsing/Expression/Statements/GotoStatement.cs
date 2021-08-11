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
    internal sealed class GotoStatement : Statement
    {
        public override StatementKind Kind => StatementKind.Goto;

        /// <summary>
        /// The expression to go to. Can be computed or a label.
        /// </summary>
        public CExpression Destination { get; init; } = null;

        public override string ToString()
        {
            return base.ToString() + $" {Destination}";
        }
    }
}
