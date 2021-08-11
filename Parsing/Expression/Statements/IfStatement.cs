using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression.Statements
{
    /// <summary>
    /// An if statement.
    /// </summary>
    internal sealed class IfStatement : Statement
    {
        public override StatementKind Kind => StatementKind.If;

        /// <summary>
        /// The expression between the parentheses.
        /// </summary>
        public CExpression Condition { get; init; }

        /// <summary>
        /// Is this if statement a constant one? (If so, resolve expression at compile-time)
        /// </summary>
        public bool IsConst { get; init; } = false;

        /// <summary>
        /// This statement will get executed if true.
        /// </summary>
        public Statement Then { get; init; }

        /// <summary>
        /// This statement will get executed if false.
        /// </summary>
        public Statement Else { get; init; } = null;

        public override string ToString()
        {
            return base.ToString() + $" if [{Condition}] then [{Then}] {(Else is not null ? $"else [{Else}]" : "")}";
        }
    }
}
