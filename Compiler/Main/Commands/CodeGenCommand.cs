using System;
using System.IO;
using System.Collections.Generic;
using Fminusminus.CodeGen;
using Fminusminus.Optimizer;
using Fminusminus.Main.UI;

namespace Fminusminus.Main.Commands
{
    /// <summary>
    /// Handle 'codegen' command
    /// </summary>
    public static class CodeGenCommand
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
            string target = args.Length > 2 ? args[2] : "cil";
            string optLevel = args.Length > 3 ? args[3] : "o1";
            
            if (!File.Exists(filename))
            {
                ErrorHandler.Warning($"File not found: {filename}");
                return 1;
            }

            string code = File.ReadAllText(filename);
            
            ErrorHandler.Info($"Generating code for {filename}\n");
            
            Console.WriteLine("\u001b[33m[1/4] Lexing...\u001b[0m");
            var lexer = new Lexer(code);
            var tokens = lexer.ScanTokens();
            ErrorHandler.Success($"Found {tokens.Count} tokens");
            
            Console.WriteLine("\u001b[33m[2/4] Parsing...\u001b[0m");
            var parser = new Parser(tokens);
            var ast = parser.Parse();
            ErrorHandler.Success($"AST generated");

            var targetMap = new Dictionary<string, CodeGenerator.TargetPlatform>(StringComparer.OrdinalIgnoreCase)
            {
                ["cil"] = CodeGenerator.TargetPlatform.CIL,
                ["il"] = CodeGenerator.TargetPlatform.CIL,
                ["c"] = CodeGenerator.TargetPlatform.C,
                ["js"] = CodeGenerator.TargetPlatform.JavaScript,
                ["javascript"] = CodeGenerator.TargetPlatform.JavaScript,
                ["py"] = CodeGenerator.TargetPlatform.Python,
                ["python"] = CodeGenerator.TargetPlatform.Python,
                ["f--"] = CodeGenerator.TargetPlatform.Fminus,
                ["fminus"] = CodeGenerator.TargetPlatform.Fminus
            };

            var optMap = new Dictionary<string, AstOptimizer.OptimizationLevel>(StringComparer.OrdinalIgnoreCase)
            {
                ["o0"] = AstOptimizer.OptimizationLevel.O0,
                ["o1"] = AstOptimizer.OptimizationLevel.O1,
                ["o2"] = AstOptimizer.OptimizationLevel.O2,
                ["o3"] = AstOptimizer.OptimizationLevel.O3,
                ["none"] = AstOptimizer.OptimizationLevel.O0,
                ["basic"] = AstOptimizer.OptimizationLevel.O1,
                ["aggressive"] = AstOptimizer.OptimizationLevel.O2,
                ["max"] = AstOptimizer.OptimizationLevel.O3
            };

            if (!targetMap.ContainsKey(target))
            {
                ErrorHandler.Warning($"Unknown target: {target}");
                Console.WriteLine("   Available targets: cil, c, js, py, f--");
                return 1;
            }

            if (!optMap.ContainsKey(optLevel))
            {
                ErrorHandler.Warning($"Unknown optimization level: {optLevel}");
                Console.WriteLine("   Available levels: o0, o1, o2, o3");
                return 1;
            }

            var targetEnum = targetMap[target];
            var optEnum = optMap[optLevel];

            Console.WriteLine($"\u001b[33m[3/4] Optimizing (level {optLevel})...\u001b[0m");
            Console.WriteLine($"\u001b[33m[4/4] Generating {target} code...\u001b[0m");

            var driver = new CodeGenDriver(ast, targetEnum, optEnum, saveToFile: true);
            var generatedCode = driver.Generate();
            
            ErrorHandler.Success($"Code generation successful!");
            
            Console.WriteLine("\n\u001b[36m=== Generated Code Preview ===\u001b[0m");
            var lines = generatedCode.Split('\n');
            int previewLines = Math.Min(10, lines.Length);
            for (int i = 0; i < previewLines; i++)
            {
                Console.WriteLine($"  {lines[i]}");
            }
            if (lines.Length > 10)
            {
                Console.WriteLine($"  ... ({lines.Length - 10} more lines)");
            }

            return 0;
        }
    }
}
