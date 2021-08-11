using mnc.Compilation;
using mnc.Parsing;
using mnc.Parsing.Expression;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace mnc
{
    class Program
    {
        static void Main(string[] args)
        {
            var thread = new Thread(() => _Main(args), 33554432);
            thread.Start();
            thread.Join();
        }

        static void _Main(string[] args)
        {
#if DEBUG
            Console.ResetColor();
#endif
            Environment.CurrentDirectory = new DirectoryInfo(Environment.CurrentDirectory).Parent.Parent.Parent.FullName;
            while (true)
            {
                Console.Write($"PWD {Environment.CurrentDirectory}> ");
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                    continue;
                var compilationUnit = new CompilationUnit(input);
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var parsed = compilationUnit.Parse();
                stopwatch.Stop();
                foreach (var parsedStatement in parsed)
                    Console.WriteLine(parsedStatement);
                Console.WriteLine($"{stopwatch.Elapsed.TotalMilliseconds} ms");
                GC.Collect();
            }
        }
    }
}
