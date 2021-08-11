using mnc.Parsing.Expression;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mnc.Compilation
{
    /// <summary>
    /// A compilation unit (parser, generation and preprocessor)
    /// </summary>
    public sealed class CompilationUnit
    {
        /// <summary>
        /// The source code.
        /// </summary>
        public string Source { get; private set; }

        /// <summary>
        /// Creates a compilation unit.
        /// </summary>
        /// <param name="filepath">The path to the file to compile.</param>
        public CompilationUnit(string filepath)
        {
            Source = File.ReadAllText(filepath);
        }

        /// <summary>
        /// Parses the source file.
        /// </summary>
        /// <returns>The parsed file.</returns>
        internal IEnumerable<Statement> Parse()
        {
            var parser = new Parser(Source);
            var statements = new List<Statement>();
            while (!parser.AllConsumed)
                statements.Add(parser.ParseStatement());
            return statements;
        }
    }
}
