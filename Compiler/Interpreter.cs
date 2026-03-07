using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Fminusminus.Errors;
using Fminusminus.Utils;

namespace Fminusminus
{
    public class Interpreter
    {
        private Dictionary<string, object> _variables = new();
        private string _currentFile = null;
        private bool _inFileBlock = false;
        private List<string> _fileContent = new();
        private SystemInfo _systemInfo = null;
        private readonly List<FileError> _fileErrors = new();

        // Memory simulation
        private long _totalMemory = 1024;
        private long _usedMemory = 256;
        private long _memoryLeft => _totalMemory - _usedMemory;

        public int Execute(ProgramNode program)
        {
            if (!program.HasImportComputer)
                throw new Exception("Missing 'import computer'");
            
            _systemInfo = new SystemInfo();
            
            try
            {
                foreach (var statement in program.StartBlock.Statements)
                {
                    ExecuteStatement(statement);
                }
            }
            catch (FileError ex)
            {
                _fileErrors.Add(ex);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ {ex.Message}");
                Console.ResetColor();
                return 1;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Unexpected error: {ex.Message}");
                Console.ResetColor();
                return 1;
            }
            
            return 0;
        }

        private void ExecuteStatement(StatementNode statement)
        {
            switch (statement)
            {
                case PrintlnStatementNode println:
                    ExecutePrintln(println);
                    break;
                    
                case PrintStatementNode print:
                    ExecutePrint(print);
                    break;
                    
                case ReturnStatementNode ret:
                    // Handled by caller
                    break;
                    
                case EndStatementNode end:
                    // Just marks the end
                    break;
                    
                case AssignmentNode assign:
                    ExecuteAssignment(assign);
                    break;
                    
                case IOStatementNode io:
                    ExecuteIO(io);
                    break;
                    
                case ComputerStatementNode computer:
                    ExecuteComputer(computer);
                    break;
                    
                case AtBlockNode atBlock:
                    ExecuteAtBlock(atBlock);
                    break;
                    
                case MemoryStatementNode memory:
                    ExecuteMemory(memory);
                    break;
                    
                default:
                    throw new Exception($"Unknown statement type: {statement?.GetType().Name}");
            }
        }

        private void ExecutePrintln(PrintlnStatementNode println)
        {
            string output = EvaluateExpression(println.Expression);
            
            if (_inFileBlock && _currentFile != null)
            {
                _fileContent.Add(output);
            }
            else
            {
                Console.WriteLine(output);
            }
        }

        private void ExecutePrint(PrintStatementNode print)
        {
            string output = EvaluateExpression(print.Expression);
            
            if (_inFileBlock && _currentFile != null)
            {
                _fileContent.Add(output);
            }
            else
            {
                Console.Write(output);
            }
        }

        private void ExecuteAssignment(AssignmentNode assign)
        {
            string value = EvaluateExpression(assign.Value);
            _variables[assign.VariableName] = value;
        }

        private void ExecuteComputer(ComputerStatementNode computer)
        {
            if (computer.Property == "systeminfo" && computer.Operation == "get")
            {
                _variables["systeminfo"] = _systemInfo;
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(_systemInfo.ToString());
                Console.ResetColor();
            }
        }

        private void ExecuteIO(IOStatementNode io)
        {
            switch (io.Operation)
            {
                case "cfile":
                    ExecuteCreateFile(io);
                    break;
                    
                case "println":
                case "print":
                    ExecuteFilePrint(io);
                    break;
                    
                case "save":
                    ExecuteFileSave(io);
                    break;
                    
                case "listfile":
                    ExecuteListFile(io);
                    break;
                    
                default:
                    throw new Exception($"Unknown IO operation: {io.Operation}");
            }
        }

        private void ExecuteCreateFile(IOStatementNode io)
        {
            if (io.Parameters.Count < 1)
                throw new Exception("io.cfile requires filename parameter");
            
            string fileName = EvaluateExpression(io.Parameters[0]);
            
            // Validate filename
            FileError.ValidateFilename(fileName);
            
            // Xử lý path parameter nếu có
            string path = "";
            if (io.Parameters.Count > 1 && io.Parameters[1] is VariableNode pathVar && pathVar.Name == "path")
            {
                if (io.Parameters.Count > 2)
                    path = EvaluateExpression(io.Parameters[2]);
            }
            
            // Mặc định lưu ở thư mục hiện tại
            _currentFile = path == "" ? fileName : Path.Combine(path, fileName);
            if (!_currentFile.EndsWith(".txt"))
                _currentFile += ".txt";
            
            Console.WriteLine($"Created file: {_currentFile}");
        }

        private void ExecuteFilePrint(IOStatementNode io)
        {
            if (_currentFile == null)
                throw new Exception("No file opened. Use io.cfile first.");
            
            if (io.Parameters.Count < 1)
                throw new Exception($"io.{io.Operation} requires content parameter");
            
            string content = EvaluateExpression(io.Parameters[0]);
            _fileContent.Add(content);
        }

        private void ExecuteFileSave(IOStatementNode io)
        {
            if (_currentFile == null)
                throw new Exception("No file to save. Use io.cfile first.");
            
            // Xử lý path parameter nếu có
            string savePath = _currentFile;
            if (io.Parameters.Count > 0)
            {
                string path = EvaluateExpression(io.Parameters[0]);
                if (Directory.Exists(path))
                {
                    savePath = Path.Combine(path, Path.GetFileName(_currentFile));
                }
                else
                {
                    savePath = path;
                }
            }
            
            try
            {
                // Validate save path
                string directory = Path.GetDirectoryName(savePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllLines(savePath, _fileContent);
                Console.WriteLine($"File saved: {savePath}");
                
                // Clear file content after save
                _fileContent.Clear();
            }
            catch (UnauthorizedAccessException)
            {
                throw FileError.AccessDenied(savePath);
            }
            catch (PathTooLongException)
            {
                throw FileError.PathTooLong(savePath);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving file: {ex.Message}");
            }
        }

        private void ExecuteListFile(IOStatementNode io)
        {
            string path = ".";
            
            // Parse parameters
            if (io.Parameters.Count > 0)
            {
                if (io.Parameters[0] is VariableNode var && var.Name == "path")
                {
                    if (io.Parameters.Count > 1)
                    {
                        if (io.Parameters[1] is StringLiteralNode strNode)
                        {
                            path = strNode.Value;
                        }
                        else if (io.Parameters[1] is VariableNode osVar && osVar.Name == "OS" && 
                                 io.Parameters.Count > 2 && io.Parameters[2] is VariableNode pathVar && pathVar.Name == "path")
                        {
                            path = SystemInfo.GetOSPath();
                        }
                    }
                }
            }
            
            // Validate path
            if (string.IsNullOrWhiteSpace(path))
                throw FileError.NotFound(path);
            
            // Check for invalid characters
            if (FileError.HasInvalidCharacters(path))
                throw FileError.InvalidCharacters(path);
            
            // Check if directory exists
            if (!Directory.Exists(path))
                throw FileError.NotFound(path);
            
            // Check access permission
            try
            {
                Directory.GetFiles(path);
            }
            catch (UnauthorizedAccessException)
            {
                throw FileError.AccessDenied(path);
            }
            
            var files = Directory.GetFiles(path);
            var dirs = Directory.GetDirectories(path);
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n📁 Contents of '{path}':");
            Console.WriteLine($"   Total: {dirs.Length} folders, {files.Length} files\n");
            Console.ResetColor();
            
            foreach (var dir in dirs)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"   📂 {Path.GetFileName(dir)}/");
                Console.ResetColor();
            }
            
            foreach (var file in files)
            {
                var info = new FileInfo(file);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"   📄 {Path.GetFileName(file)} ({info.Length} bytes)");
                Console.ResetColor();
            }
                
            Console.WriteLine();
        }

        private void ExecuteAtBlock(AtBlockNode atBlock)
        {
            string fileName = EvaluateExpression(atBlock.FileName);
            string previousFile = _currentFile;
            bool previousInBlock = _inFileBlock;
            var previousContent = _fileContent;
            
            _currentFile = fileName;
            _inFileBlock = true;
            _fileContent = new List<string>();
            
            foreach (var statement in atBlock.Statements)
            {
                ExecuteStatement(statement);
            }
            
            // Restore previous state
            _currentFile = previousFile;
            _inFileBlock = previousInBlock;
            _fileContent = previousContent;
        }

        private void ExecuteMemory(MemoryStatementNode memory)
        {
            long value = memory.Property switch
            {
                "memoryleft" => _memoryLeft,
                "memoryused" => _usedMemory,
                "memorytotal" => _totalMemory,
                _ => throw new Exception($"Unknown memory property: {memory.Property}")
            };
            
            if (_inFileBlock && _currentFile != null)
            {
                _fileContent.Add(value.ToString());
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"{memory.Property}: {value} MB");
                Console.ResetColor();
            }
            
            // Simulate memory usage
            if (memory.Property == "memoryused")
                _usedMemory = Math.Min(_totalMemory, _usedMemory + 1);
        }

        private string EvaluateExpression(ExpressionNode expr)
        {
            switch (expr)
            {
                case StringLiteralNode str:
                    if (str.IsInterpolated)
                        return InterpolateString(str.Value);
                    return str.Value;
                    
                case NumberLiteralNode num:
                    return num.Value.ToString();
                    
                case VariableNode var:
                    if (_variables.TryGetValue(var.Name, out object value))
                        return value.ToString();
                    throw new Exception($"Undefined variable: {var.Name}");
                    
                default:
                    throw new Exception($"Cannot evaluate expression: {expr?.GetType().Name}");
            }
        }

        private string InterpolateString(string template)
        {
            var result = new System.Text.StringBuilder();
            int i = 0;
            
            while (i < template.Length)
            {
                if (template[i] == '{')
                {
                    int j = i + 1;
                    int braceCount = 1;
                    
                    while (j < template.Length && braceCount > 0)
                    {
                        if (template[j] == '{') braceCount++;
                        if (template[j] == '}') braceCount--;
                        j++;
                    }
                    
                    if (braceCount == 0)
                    {
                        string varName = template.Substring(i + 1, j - i - 2).Trim();
                        
                        if (_variables.TryGetValue(varName, out object value))
                        {
                            result.Append(value.ToString());
                        }
                        else if (varName == "systeminfo" && _systemInfo != null)
                        {
                            result.Append(_systemInfo.ToString());
                        }
                        else
                        {
                            result.Append($"{{{varName}}}");
                        }
                        
                        i = j;
                    }
                    else
                    {
                        result.Append(template[i]);
                        i++;
                    }
                }
                else
                {
                    result.Append(template[i]);
                    i++;
                }
            }
            
            return result.ToString();
        }
    }
}
