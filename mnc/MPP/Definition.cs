using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.MPP
{
    /// <summary>
    /// A definition.
    /// </summary>
    internal sealed class Definition
    {
        /// <summary>
        /// The name of the definition.
        /// </summary>
        public string Alias { get; init; }

        /// <summary>
        /// The value to paste.
        /// </summary>
        public string Value { get; init; }
    }
}
