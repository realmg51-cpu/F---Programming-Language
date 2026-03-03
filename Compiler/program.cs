using System;
using System.IO;
using System.Collections.Generic;

namespace FSharpMinus.Compiler
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine(@"
   ______
  |  ___|
  | |_ __ _ _ __ ___   __ _ 
  |  _/ _` | '_ ` _ \ / _` |
  | || (_| | | | | | | (_| |
  \_| \__,_|_| |_| |_|\__,_|
            ");
            Console.WriteLine("F-- Compiler v1.4 - The backward step of humanity\n");

            if (args.Length == 0)
            {
                Console.WriteLine("Usage: fminus <file.f--> [options]");
                Console.WriteLine("Options:");
                Console.WriteLine("  --run     Run the program");
                Console.WriteLine("  --ast     Show AST");
                Console.WriteLine("  --version Show version");
                return 1;
            }

            string filePath = args[0];
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"fmm004: File not found - {filePath}");
                return 1;
            }

            string code = File.ReadAllText(filePath);
            var interpreter = new FSharpMinusInterpreter();
            
            try
            {
                interpreter.Interpret(code);
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"fmm001: {ex.Message}");
                return 1;
            }
        }
    }

    public class FSharpMinusInterpreter
    {
        private Dictionary<string, object> variables = new();
        private string currentFile = "";
        private long memoryLeft = 1024; // Simulate 1GB memory

        public void Interpret(string code)
        {
            var lines = code.Split('\n');
            bool inStart = false;
            bool inFileBlock = false;

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                
                if (line.StartsWith("start()"))
                {
                    inStart = true;
                    continue;
                }

                if (inStart && line.Contains("{"))
                    continue;
                    
                if (inStart && line.Contains("}"))
                {
                    inStart = false;
                    continue;
                }

                if (!inStart) continue;

                // Parse F-- commands
                if (line.Contains("println"))
                {
                    ParsePrintln(line);
                }
                else if (line.Contains("memory.memoryleft"))
                {
                    Console.WriteLine($"Memory left: {memoryLeft} MB");
                }
                else if (line.Contains("io.cfile"))
                {
                    ParseCreateFile(line);
                }
                else if (line.Contains("at "))
                {
                    inFileBlock = ParseAtBlock(line);
                }
                else if (line.Contains("io.save()"))
                {
                    if (!string.IsNullOrEmpty(currentFile))
                        File.WriteAllText(currentFile, "content saved by F--");
                }
                else if (line.Contains("return"))
                {
                    ParseReturn(line);
                }
            }
        }

        private void ParsePrintln(string line)
        {
            if (line.Contains("$\""))
            {
                // Handle interpolation
                int start = line.IndexOf('$') + 2;
                int end = line.LastIndexOf('"');
                string template = line.Substring(start, end - start);
                // Simple interpolation - replace {var} with value
                Console.WriteLine(template);
            }
            else
            {
                var parts = line.Split('"');
                if (parts.Length > 1)
                    Console.WriteLine(parts[1]);
            }
        }

        private void ParseCreateFile(string line)
        {
            var parts = line.Split('"');
            if (parts.Length > 1)
            {
                string fileName = parts[1];
                currentFile = fileName.EndsWith(".txt") ? fileName : fileName + ".txt";
                Console.WriteLine($"Created file: {currentFile}");
            }
        }

        private bool ParseAtBlock(string line)
        {
            var parts = line.Split('"');
            if (parts.Length > 1)
            {
                currentFile = parts[1];
                Console.WriteLine($"Working in file: {currentFile}");
            }
            return true;
        }

        private void ParseReturn(string line)
        {
            var match = System.Text.RegularExpressions.Regex.Match(line, @"return\((\d+)\)");
            if (match.Success)
            {
                int code = int.Parse(match.Groups[1].Value);
                if (code == 0)
                    Console.WriteLine("Program completed successfully!");
                else
                    Console.WriteLine($"Program exited with code: {code}");
            }
        }
    }
}
