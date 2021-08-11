using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression.Statements
{
    /// <summary>
    /// A for(...;...;...) statement
    /// </summary>
    internal sealed class ForStatement : Statement
    {
        public override StatementKind Kind => StatementKind.For;

        /// <summary>
        /// The statement inside the parentheses in the for loop: for([here];...;...)
        /// </summary>
        public Statement Init { get; init; }

        /// <summary>
        /// The condition for iteration: for(...;[here];...)
        /// </summary>
        public CExpression Condition { get; init; }

        /// <summary>
        /// The code to execute just before the loop iterates again: for(...;...;[here])
        /// </summary>
        public CExpression Iteration { get; init; }

        /// <summary>
        /// The code to execute.
        /// </summary>
        public Statement Do { get; init; }

        public override string ToString()
        {
            return base.ToString() + $" [{Init}], while [{Condition}], do [{Do}], then [{Iteration}]";
        }
    }
}
