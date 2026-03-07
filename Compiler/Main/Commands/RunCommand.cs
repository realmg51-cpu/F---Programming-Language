using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Fminusminus.Main.UI;

namespace Fminusminus.Main.Commands
{
    /// <summary>
    /// Handle 'run' command
    /// </summary>
    public static class RunCommand
    {
        public static int Execute(string[] args)
        {
            string filename = "";
            List<string> programArgs = new();
            bool helpRequested = false;
            bool verbose = false;

            // Parse command line arguments
            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i].ToLower())
                {
                    case "--help":
                    case "-h":
                        helpRequested = true;
                        break;
                        
                    case "--verbose":
                    case "-v":
                        verbose = true;
                        break;
                        
                    case "--":
                        // Everything after -- is program arguments
                        for (int j = i + 1; j < args.Length; j++)
                            programArgs.Add(args[j]);
                        i = args.Length;
                        break;
                        
                    default:
                        if (filename == "" && !args[i].StartsWith("-"))
                        {
                            filename = args[i];
                        }
                        else if (args[i].StartsWith("-"))
                        {
                            ErrorHandler.Warning($"Unknown option: {args[i]}");
                        }
                        else
                        {
                            programArgs.Add(args[i]);
                        }
                        break;
                }
            }

            if (helpRequested || filename == "")
            {
                ShowHelp();
                return helpRequested ? 0 : 1;
            }

            // Validate file
            if (!File.Exists(filename))
            {
                ErrorHandler.Warning($"File not found: {filename}");
                return 3; // File not found exit code
            }

            if (!filename.EndsWith(".f--", StringComparison.OrdinalIgnoreCase))
            {
                ErrorHandler.Warning($"File must have .f-- extension: {filename}");
                return 1;
            }

            // Check file size
            var fileInfo = new FileInfo(filename);
            if (fileInfo.Length > 10 * 1024 * 1024) // 10MB limit
            {
                ErrorHandler.Warning($"File too large ({fileInfo.Length / 1024 / 1024} MB). Maximum allowed: 10 MB");
                return 1;
            }

            // Setup Ctrl+C handler
            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("\n\n⚠ Interrupted by user");
                e.Cancel = true;
                Environment.Exit(130);
            };

            ErrorHandler.Info($"Running: {filename}");
            if (programArgs.Count > 0)
            {
                Console.WriteLine($"   Arguments: {string.Join(" ", programArgs)}");
            }
            Console.WriteLine();

            // Read source code
            string code = File.ReadAllText(filename);
            if (verbose)
            {
                Console.WriteLine($"📄 Source size: {code.Length} characters");
            }

            var stopwatch = Stopwatch.StartNew();
            long memoryBefore = GC.GetTotalMemory(false);

            // Lexer
            if (verbose) Console.WriteLine("\u001b[33m[1/4] Lexing...\u001b[0m");
            List<Token> tokens;
            try
            {
                var lexer = new Lexer(code);
                tokens = lexer.ScanTokens();
                if (verbose) ErrorHandler.Success($"Found {tokens.Count} tokens");
            }
            catch (AggregateException ex)
            {
                ErrorHandler.DisplayMultiple(ex);
                return 2; // Syntax error
            }
            catch (Exception ex)
            {
                ErrorHandler.Display(ex);
                return 1;
            }

            // Parser
            if (verbose) Console.WriteLine("\u001b[33m[2/4] Parsing...\u001b[0m");
            ProgramNode ast;
            try
            {
                var parser = new Parser(tokens);
                ast = parser.Parse();
                if (verbose) ErrorHandler.Success($"AST generated");
            }
            catch (AggregateException ex)
            {
                ErrorHandler.DisplayMultiple(ex);
                return 2; // Syntax error
            }
            catch (Exception ex)
            {
                ErrorHandler.Display(ex);
                return 1;
            }

            // Show AST in verbose mode
            if (verbose)
            {
                Console.WriteLine("\n\u001b[36mAST Preview:\u001b[0m");
                ast.Print();
                Console.WriteLine();
            }

            // Interpreter
            if (verbose) Console.WriteLine("\u001b[33m[3/4] Interpreting...\u001b[0m");
            int result;
            try
            {
                var interpreter = new Interpreter(programArgs.ToArray());
                
                // Redirect output if needed
                // (could add option to capture output)
                
                result = interpreter.Execute(ast);
            }
            catch (Exception ex)
            {
                ErrorHandler.Display(ex);
                return 1;
            }

            // Done
            stopwatch.Stop();
            long memoryAfter = GC.GetTotalMemory(false);

            if (verbose) Console.WriteLine("\u001b[33m[4/4] Done!\u001b[0m");
            
            // Show results
            if (result == 0)
            {
                ErrorHandler.Success($"Program completed with exit code: {result}");
            }
            else
            {
                ErrorHandler.Warning($"Program completed with exit code: {result}");
            }

            // Show statistics
            Console.WriteLine($"\n📊 Execution Statistics:");
            Console.WriteLine($"   • Time: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"   • Memory: {(memoryAfter - memoryBefore) / 1024} KB");
            Console.WriteLine($"   • GC Collections: GC.CollectionCount(0)");
            if (programArgs.Count > 0)
            {
                Console.WriteLine($"   • Arguments: {programArgs.Count}");
            }

            return result;
        }

        private static void ShowHelp()
        {
            Console.WriteLine("run - Execute an F-- program");
            Console.WriteLine("=============================");
            Console.WriteLine();
            Console.WriteLine("Usage: fminus run <filename> [options] [--] [program arguments]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  -v, --verbose    Show detailed execution info");
            Console.WriteLine("  -h, --help       Show this help");
            Console.WriteLine("  --               Separate F-- options from program arguments");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("  fminus run hello.f--");
            Console.WriteLine("  fminus run hello.f-- -v");
            Console.WriteLine("  fminus run hello.f-- -- arg1 arg2");
            Console.WriteLine();
            Console.WriteLine("Exit Codes:");
            Console.WriteLine("  0   Success");
            Console.WriteLine("  1   General error");
            Console.WriteLine("  2   Syntax error");
            Console.WriteLine("  3   File not found");
            Console.WriteLine("  130 Interrupted by user");
        }
    }
}
