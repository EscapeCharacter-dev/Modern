using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression
{
    /// <summary>
    /// A namespace.
    /// </summary>
    internal sealed class Namespace
    {
        /// <summary>
        /// The name of this node.
        /// </summary>
        public string NodeName { get; init; }

        /// <summary>
        /// The parent node.
        /// </summary>
        public Namespace Parent { get; init; }

        public override string ToString() => Parent == null ? NodeName : Parent.ToString() + $"::{NodeName}";
    }
}
