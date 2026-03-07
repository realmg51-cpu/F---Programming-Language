using System;
using System.IO;
using Fminusminus.Optimizer;

namespace Fminusminus.CodeGen
{
    /// <summary>
    /// Driver for code generation - handles multiple output formats and file writing
    /// </summary>
    public class CodeGenDriver
    {
        private readonly ProgramNode _ast;
        private readonly CodeGenerator.TargetPlatform _target;
        private readonly CodeGenerator.OptimizationLevel _optLevel;
        private readonly bool _saveToFile;
        private readonly string _outputFilename;

        public CodeGenDriver(ProgramNode ast, 
                            CodeGenerator.TargetPlatform target = CodeGenerator.TargetPlatform.CIL,
                            CodeGenerator.OptimizationLevel opt = CodeGenerator.OptimizationLevel.O1,
                            bool saveToFile = true,
                            string outputFilename = null)
        {
            _ast = ast ?? throw new ArgumentNullException(nameof(ast), "AST cannot be null");
            _target = target;
            _optLevel = opt;
            _saveToFile = saveToFile;
            _outputFilename = outputFilename;
        }

        public string Generate()
        {
            try
            {
                Console.WriteLine($"🚀 Generating code for {_target} (optimization level {_optLevel})...");

                var generator = new CodeGenerator(_ast, _target, _optLevel);
                var code = generator.Generate();

                if (_saveToFile)
                {
                    SaveToFile(code);
                }
                else
                {
                    Console.WriteLine("📋 Code generated (not saved to file)");
                }

                PrintStats();
                return code;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Code generation failed: {ex.Message}");
                throw; // Re-throw để caller xử lý
            }
        }

        private void SaveToFile(string code)
        {
            try
            {
                string filename = GetFilename();
                
                // Kiểm tra kích thước code
                if (code.Length > 10_000_000) // 10MB
                {
                    Console.WriteLine("⚠ Warning: Generated code is very large (>10MB)");
                }
                
                // Tạo thư mục nếu cần
                string directory = Path.GetDirectoryName(filename);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // Backup file cũ nếu tồn tại
                if (File.Exists(filename))
                {
                    string backup = filename + ".bak";
                    File.Copy(filename, backup, true);
                    Console.WriteLine($"💾 Backup created: {backup}");
                }
                
                File.WriteAllText(filename, code);
                Console.WriteLine($"💾 Saved to {filename}");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine($"❌ Cannot write to file: Access denied");
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine($"❌ Directory not found");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"❌ IO Error: {ex.Message}");
            }
        }

        private string GetFilename()
        {
            // Nếu có tên file được chỉ định, dùng nó
            if (!string.IsNullOrEmpty(_outputFilename))
                return _outputFilename;
                
            // Xác định extension dựa trên target platform
            string extension = _target switch
            {
                CodeGenerator.TargetPlatform.CIL => ".il",
                CodeGenerator.TargetPlatform.C => ".c",
                CodeGenerator.TargetPlatform.JavaScript => ".js",
                CodeGenerator.TargetPlatform.Python => ".py",
                CodeGenerator.TargetPlatform.Fminus => ".f--",
                _ => ".txt"
            };

            // Tạo thư mục output nếu chưa có
            string outputDir = "fminus_output";
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // Tạo tên file với timestamp để tránh ghi đè
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return Path.Combine(outputDir, $"output_{timestamp}{extension}");
        }

        public void PrintStats()
        {
            Console.WriteLine($"📊 Target: {_target}");
            Console.WriteLine($"📊 Optimization: {_optLevel}");
            Console.WriteLine($"📊 Save to file: {_saveToFile}");
            if (!string.IsNullOrEmpty(_outputFilename))
                Console.WriteLine($"📊 Output file: {_outputFilename}");
        }
    }
}
