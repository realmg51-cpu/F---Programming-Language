using System;
using System.IO;
using Fminusminus.Main.UI;

namespace Fminusminus.Main.Commands
{
    /// <summary>
    /// Handle 'ast' command
    /// </summary>
    public static class AstCommand
    {
        public static int Execute(string[] args)
        {
            if (args.Length < 2)
            {
                ErrorHandler.Warning("Missing filename");
                HelpCommand.Show();
                return 1;
            }

            string filename = args[1];
            
            if (!File.Exists(filename))
            {
                ErrorHandler.Warning($"File not found: {filename}");
                return 1;
            }

            string code = File.ReadAllText(filename);
            
            var lexer = new Lexer(code);
            var tokens = lexer.ScanTokens();
            
            var parser = new Parser(tokens);
            var ast = parser.Parse();
            
            Console.WriteLine("\n\u001b[36m=== Abstract Syntax Tree ===\u001b[0m\n");
            ast.Print();
            
            return 0;
        }
    }
}
