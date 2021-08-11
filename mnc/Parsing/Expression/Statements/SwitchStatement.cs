using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression.Statements
{
    /// <summary>
    /// A switch statement.
    /// </summary>
    internal sealed class SwitchStatement : Statement
    {
        public override StatementKind Kind => StatementKind.Switch;

        /// <summary>
        /// The switch cases.
        /// </summary>
        public IEnumerable<(CExpression Against, IEnumerable<Statement> Statements)> Switch { get; init; } =
            Enumerable.Empty<(CExpression, IEnumerable<Statement>)>();

        /// <summary>
        /// The sub switches.
        /// </summary>
        public IEnumerable<(int, int)> SubSwitches { get; init; } =
            Enumerable.Empty<(int, int)>();

        /// <summary>
        /// The expression to check.
        /// </summary>
        public CExpression Source { get; init; }

        public override string ToString()
        {
            var @base = $"SWITCH on[{Source}] {{";
            foreach (var clause in Switch)
            {
                if (clause.Against == null)
                    @base += "DEFAULT/START/END: ";
                else
                    @base += $"CASE {clause.Against}: ";
                foreach (var stat in clause.Statements)
                    @base += stat;
                @base += ";;";
            }
            @base += "}(";
            foreach (var startEnd in SubSwitches)
                @base += $"{startEnd.Item1} to {startEnd.Item2},";
            return @base.Trim(',') + ")";
        }
    }
}
