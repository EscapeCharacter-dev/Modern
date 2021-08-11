using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing
{
    /// <summary>
    /// This class provides extension methods for the enum Token.TokenKind.
    /// </summary>
    internal static class ExtendedTokenKindFunctions
    {
        /// <summary>
        /// Gets the current prefix unary operator precedence.
        /// </summary>
        /// <param name="tokenKind">The token kind.</param>
        /// <returns>The precedence of the operator.</returns>
        public static int GetPrefixUnaryPrecedence(this Token.TokenKind tokenKind)
        {
            switch (tokenKind)
            {
            case Token.TokenKind.PlusPlus:
            case Token.TokenKind.MinusMinus:
            case Token.TokenKind.Plus:
            case Token.TokenKind.Minus:
            case Token.TokenKind.Exclamation:
            case Token.TokenKind.Tilde:
            case Token.TokenKind.At:
            case Token.TokenKind.Dollar:
            case Token.TokenKind.Sizeof:
                return 11;
            default:
                return -1;
            }
        }

        public static int GetPrefixBinaryPrecedence(this Token.TokenKind tokenKind)
        {
            switch (tokenKind)
            {
            case Token.TokenKind.Star:
            case Token.TokenKind.Slash:
            case Token.TokenKind.Percent:
                return 10;
            case Token.TokenKind.Plus:
            case Token.TokenKind.Minus:
                return 9;
            case Token.TokenKind.LeftArrows:
            case Token.TokenKind.RightArrows:
                return 8;
            default:
                return 0;
            }
        }

        /// <summary>
        /// Checks if the token matches any of the other ones.
        /// </summary>
        /// <param name="kind">The kind to check for.</param>
        /// <param name="others">The kinds to check against for.</param>
        /// <returns>True if matched.</returns>
        public static bool Matches(this Token.TokenKind kind, params Token.TokenKind[] others) => others.Contains(kind);
    }
}
