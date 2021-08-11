using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression
{
    /// <summary>
    /// An expression.
    /// </summary>
    internal abstract class CExpression
    {
        /// <summary>
        /// The kind of expression.
        /// </summary>
        public enum ExpressionKind
        {
            Nothing,
            Literal,
            Range,

            NAE,

            PostfixIncrement,
            PostfixDecrement,
            FunctionCall,
            ArraySubscript,
            MemberAccess,
            DereferencedMemberAccess,

            PrefixIncrement,
            PrefixDecrement,
            Plus,
            Minus,
            LogicalNot,
            BitwiseNot,
            Cast,
            Dereference,
            AddressOf,
            SizeOf,
            TypeOf,

            Multiply,
            Divide,
            Remainder,

            Add,
            Sub,

            BitwiseLeftShift,
            BitwiseRightShift,

            Lower,
            LowerEqual,
            Greater,
            GreaterEqual,
            Between,

            Equal,
            NotEqual,

            BitwiseAnd,

            BitwiseXOr,

            BitwiseOr,

            LogicalAnd,

            LogicalXor,
            
            LogicalOr,

            Ternary,

            Assign,
            AddAssign,
            SubAssign,
            MulAssign,
            DivAssign,
            ModAssign,
            BitwiseLeftShiftAssign,
            BitwiseRightShiftAssign,
            AndAssign,
            XOrAssign,
            OrAssign,
            Compound,
        }

        /// <summary>
        /// The kind of the expression.
        /// </summary>
        public ExpressionKind Kind { get; init; }

        public override string ToString()
        {
            var result = $"{Kind}{{";
            foreach (var expr in Children)
                result += expr + ",";
            result = result.TrimEnd(',');
            result += '}';
            return result;
        }

        /// <summary>
        /// Child nodes.
        /// </summary>
        public abstract IEnumerable<CExpression> Children { get; }
    }
}
