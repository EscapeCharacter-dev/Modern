using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression.Statements
{
    /// <summary>
    /// A continue statement.
    /// </summary>
    internal sealed class BreakStatement : Statement
    {
        public override StatementKind Kind => StatementKind.Break;
    }
}
