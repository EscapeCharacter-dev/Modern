using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression
{
    /// <summary>
    /// A statement.
    /// </summary>
    internal abstract class Statement
    {
        /// <summary>
        /// The kind of a statement.
        /// </summary>
        public enum StatementKind
        {
            Empty,
            Block,
            Expression,
            Declaration,
            If,
            While,
            DoWhile,
            For,
            Return,
            Continue,
            Break,
            Goto,
            Switch, // TODO: Add switch support
            Using,
            Namespace,
        }

        public Statement()
            => Scope = Parser.CurrentParserInstance.CurrentScope;

        /// <summary>
        /// The kind of the statement.
        /// </summary>
        public abstract StatementKind Kind { get; }

        /// <summary>
        /// The scope of the statement.
        /// </summary>
        public Scope Scope { get; init; }

        public override string ToString()
        {
            return $"{Kind}";
        }
    }
}
