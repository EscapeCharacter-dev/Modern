using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression.Statements
{
    /// <summary>
    /// A statement that consists of an expression.
    /// </summary>
    internal sealed class ExpressionStatement : Statement
    {
        public override StatementKind Kind => StatementKind.Expression;

        /// <summary>
        /// The expression.
        /// </summary>
        public CExpression Expression { get; init; }

        public override string ToString()
        {
            if (Expression == null)
                return "";
            return Expression.ToString();
        }
    }
}
