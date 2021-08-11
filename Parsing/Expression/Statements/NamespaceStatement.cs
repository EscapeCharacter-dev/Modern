using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression.Statements
{
    /// <summary>
    /// A namespace statement.
    /// </summary>
    internal sealed class NamespaceStatement : Statement
    {
        public override StatementKind Kind => StatementKind.Namespace;
        /// <summary>
        /// The namespace
        /// </summary>
        public Namespace Namespace { get; init; }

        public override string ToString()
        {
            return $"NAMESPACE<{Namespace}>";
        }
    }
}
