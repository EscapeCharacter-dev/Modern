using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression
{
    /// <summary>
    /// Represents a linear namespace expression (only one node, not a tree)
    /// </summary>
    internal sealed class LinearNamespaceAccessExpression : UnaryExpression
    {
        /// <summary>
        /// The namespace.
        /// </summary>
        public Namespace Namespace { get; init; }

        public LinearNamespaceAccessExpression() { Kind = ExpressionKind.NAE; }

        /// <summary>
        /// Gets the LNAE identifier.
        /// </summary>
        public string Identifier => Operand.ToString();

        public override string ToString()
        {
            return $"{Namespace}::{Operand}";
        }
    }
}
