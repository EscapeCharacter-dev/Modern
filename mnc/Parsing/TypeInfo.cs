using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mnc.Parsing.Expression;

namespace mnc.Parsing
{
    /// <summary>
    /// Type info.
    /// </summary>
    internal sealed class TypeInfo
    {
        public enum TypeKind
        {
            Void,
            Byte,
            SByte,
            Bool,
            Char,
            UChar,
            WChar,
            Short,
            UShort,
            Int,
            UInt,
            Long,
            ULong,
            Huge,
            UHuge,
            Float,
            Double,
            LDouble,
            Pointer,
            Function,
            FixedLengthArray,
            DynamicLengthArray,
            Structure,
            Union
        }

        public override string ToString()
        {
            var baseStr = $"{Kind + (Expression != null ? $"[{Expression}]" : "")}(";
            foreach (var child in Children)
                baseStr += child + ",";
            return baseStr.TrimEnd(',') + ")";
        }

        /// <summary>
        /// The kind of the type.
        /// </summary>
        public TypeKind Kind { get; init; }

        /// <summary>
        /// An optional expression.
        /// </summary>
        public CExpression Expression { get; init; }

        /// <summary>
        /// The children types.
        /// </summary>
        public IEnumerable<TypeInfo> Children { get; init; } = Enumerable.Empty<TypeInfo>();
    }
}
