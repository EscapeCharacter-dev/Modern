using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression.Statements
{
    /// <summary>
    /// The declaration statement.
    /// </summary>
    internal sealed class DeclarationStatement : Statement
    {
        public override StatementKind Kind => StatementKind.Declaration;

        /// <summary>
        /// The declaration.
        /// </summary>
        public Declaration Declaration { get; init; }

        public override string ToString()
        {
            return $"{Declaration}";
        }
    }
}
