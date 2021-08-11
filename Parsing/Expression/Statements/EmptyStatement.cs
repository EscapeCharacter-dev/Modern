using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression.Statements
{
    /// <summary>
    /// An empty statement.
    /// </summary>
    internal sealed class EmptyStatement : Statement
    {
        public override StatementKind Kind => StatementKind.Empty;

        public override string ToString()
        {
            return "(empty)";
        }
    }
}
