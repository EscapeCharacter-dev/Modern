using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression.Statements
{
    /// <summary>
    /// A using statement.
    /// </summary>
    internal sealed class UsingStatement : Statement
    {
        public override StatementKind Kind => StatementKind.Using;
        /// <summary>
        /// The module name
        /// </summary>
        public string ModuleName { get; init; }
    }
}
