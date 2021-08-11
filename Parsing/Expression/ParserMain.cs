using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Parsing.Expression
{
    /// <summary>
    /// The parser.
    /// </summary>
    internal sealed partial class Parser
    {
        /// <summary>
        /// The current parser.
        /// </summary>
        internal static Parser CurrentParserInstance { get; private set; }
        public Parser(string toTokenize)
        {
            var tokenizer = new Tokenizer(toTokenize);
            var list = new List<Token>();
            Token token;
            while (true)
            {
                token = tokenizer.NextToken();

                if (token.Kind == Token.TokenKind.EndOfFile)
                    break;

                list.Add(token);
            }
            // Arrays are faster than System.Collections.Generic
            Tokens = list.ToArray();
            ScopePush(); // Global scope
            CurrentParserInstance = this;
        }

        /// <summary>
        /// The list of scopes.
        /// </summary>
        private readonly List<Scope> _scopes = new List<Scope>();

        /// <summary>
        /// The current scope.
        /// </summary>
        internal Scope CurrentScope => _scopes.Count > 0 ? _scopes[_scopes.Count - 1] : null;

        /// <summary>
        /// Makes a new scope.
        /// </summary>
        private void ScopePush(bool loopOrSwitch = false)
        {
            var scope = new Scope(loopOrSwitch) { Parent = CurrentScope };
            _scopes.Add(scope);
        }

        /// <summary>
        /// Pops the current scope.
        /// </summary>
        private void ScopePop()
        {
            if (_scopes.Count <= 0)
                throw new OverflowException("Underflow in scope stack");
            _scopes.RemoveAt(_scopes.Count - 1);
        }

        /// <summary>
        /// The tokens.
        /// </summary>
        private Token[] Tokens { get; }

        /// <summary>
        /// The current position.
        /// </summary>
        private int Position = 0;

        /// <summary>
        /// Peeks in the token array.
        /// </summary>
        /// <param name="offset">The offset (base = Position)</param>
        /// <returns>The token.</returns>
        private Token Peek(int offset = 0)
        {
            var newPos = Position + offset;
            return newPos < Tokens.Length ? Tokens[newPos] : null;
        }


        /// <summary>
        /// Whether all the tokens were consumed by the parser.
        /// </summary>
        public bool AllConsumed => Position >= Tokens.Length;

        /// <summary>
        /// The current token.
        /// </summary>
        private Token Current => Peek();

        /// <summary>
        /// The next token.
        /// </summary>
        private Token Ahead => Peek(1);

        /// <summary>
        /// The previous token.
        /// </summary>
        private Token Before => Position >= 1 ? Peek(-1) : null;

        /// <summary>
        /// Tries to match a token.
        /// </summary>
        /// <param name="kind">The kind to match for.</param>
        /// <param name="throwOnFalse">Wheter it should generate an error if not found.</param>
        /// <returns>True if the token is matched.</returns>
        private bool Match(Token.TokenKind kind, bool throwOnFalse = true)
        {
            if (Current == null)
            {
                // TODO: Use DiagnosticHandler
                if (throwOnFalse)
                    Console.WriteLine($"[{Before.Line},{Before.Column}] Unexpected end of file");
                return false;
            }
            if (Current.Kind != kind)
            {
                // TODO: Use DiagnosticHandler
                if (throwOnFalse)
                {
                    Console.WriteLine($"[{Current.Line},{Current.Column}] Expected {kind}, got {Current.Kind}");
                    Position++;
                }
                return false;
            }
            return true;
        }
    }
}
