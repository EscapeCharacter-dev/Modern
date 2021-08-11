using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression.Statements
{
    /// <summary>
    /// A while(...) statement
    /// </summary>
    internal sealed class WhileStatement : Statement
    {
        public override StatementKind Kind => StatementKind.While;

        /// <summary>
        /// This expression will get evaluated each iteration.
        /// </summary>
        public CExpression Condition { get; init; }

        /// <summary>
        /// This statement will get executed each iteration.
        /// </summary>
        public Statement Then { get; init; }

        public override string ToString()
        {
            return base.ToString() + $" while [{Condition}], do [{Then}]";
        }
    }
}
