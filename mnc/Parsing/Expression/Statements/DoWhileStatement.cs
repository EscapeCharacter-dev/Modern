using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression.Statements
{
    /// <summary>
    /// A do ... while(...) statement
    /// </summary>
    internal sealed class DoWhileStatement : Statement
    {
        public override StatementKind Kind => StatementKind.DoWhile;

        /// <summary>
        /// The expression. If false, the loop will stop.
        /// </summary>
        public CExpression Condition { get; init; }

        /// <summary>
        /// The statement to execute.
        /// </summary>
        public Statement Do { get; init; }

        public override string ToString()
        {
            return base.ToString() + $" do [{Do}] while [{Condition}]";
        }
    }
}
