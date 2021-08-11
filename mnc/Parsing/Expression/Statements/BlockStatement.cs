using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression.Statements
{
    /// <summary>
    /// A block statement (a group of statements between { and })
    /// </summary>
    internal sealed class BlockStatement : Statement
    {
        public override StatementKind Kind => StatementKind.Block;

        /// <summary>
        /// The children statements.
        /// </summary>
        public IEnumerable<Statement> Block { get; init; } = Enumerable.Empty<Statement>();

        public override string ToString()
        {
            var ret = base.ToString() + $"{{";
            foreach (var statement in Block)
                ret += statement + ",";
            return ret.TrimEnd(',') + "}";
        }
    }
}
