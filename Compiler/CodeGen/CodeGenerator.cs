using System;
using System.Collections.Generic;
using System.Text;
using Fminusminus.Optimizer;

namespace Fminusminus.CodeGen
{
    /// <summary>
    /// Code Generator for F-- - Translates AST to target code
    /// Supports multiple backends: CIL (.NET), C, JavaScript, etc.
    /// </summary>
    public class CodeGenerator
    {
        private readonly ProgramNode _ast;
        private readonly StringBuilder _output;
        private int _indentLevel;
        private TargetPlatform _targetPlatform;
        private readonly OptimizationLevel _optLevel;
        
        private Dictionary<string, string> _variables = new();
        private int _tempVarCounter;
        private int _labelCounter;

        public enum TargetPlatform
        {
            CIL,        // .NET Common Intermediate Language
            C,          // C language
            JavaScript, // JavaScript
            Python,     // Python
            Fminus      // F-- itself (for debugging)
        }

        public enum OptimizationLevel
        {
            O0, // No optimization
            O1, // Basic optimizations
            O2, // Aggressive optimizations
            O3  // Maximum optimizations
        }

        public CodeGenerator(ProgramNode ast, TargetPlatform target = TargetPlatform.CIL, OptimizationLevel opt = OptimizationLevel.O1)
        {
            _ast = ast;
            _output = new StringBuilder();
            _targetPlatform = target;
            _optLevel = opt;
        }

        public string Generate()
        {
            _output.Clear();
            
            // Apply optimizations if enabled
            var optimizedAst = _ast;
            if (_optLevel > OptimizationLevel.O0)
            {
                var optimizer = new AstOptimizer(_optLevel);
                optimizedAst = optimizer.Optimize(_ast);
            }

            // Generate header based on target platform
            GenerateHeader();

            // Generate code for the program
            GenerateProgram(optimizedAst);

            // Generate footer
            GenerateFooter();

            return _output.ToString();
        }

        private void GenerateHeader()
        {
            switch (_targetPlatform)
            {
                case TargetPlatform.CIL:
                    _output.AppendLine(".assembly extern mscorlib { }");
                    _output.AppendLine(".assembly Fminus { }");
                    _output.AppendLine(".module Fminus.exe");
                    _output.AppendLine();
                    break;

                case TargetPlatform.C:
                    _output.AppendLine("#include <stdio.h>");
                    _output.AppendLine("#include <stdlib.h>");
                    _output.AppendLine("#include <string.h>");
                    _output.AppendLine();
                    break;

                case TargetPlatform.JavaScript:
                    _output.AppendLine("// F-- generated JavaScript");
                    _output.AppendLine("\"use strict\";");
                    _output.AppendLine();
                    break;

                case TargetPlatform.Python:
                    _output.AppendLine("# F-- generated Python");
                    _output.AppendLine("import sys");
                    _output.AppendLine();
                    break;
            }
        }

        private void GenerateFooter()
        {
            switch (_targetPlatform)
            {
                case TargetPlatform.C:
                    _output.AppendLine("    return 0;");
                    _output.AppendLine("}");
                    break;

                case TargetPlatform.JavaScript:
                    _output.AppendLine("})();");
                    break;

                case TargetPlatform.Python:
                    _output.AppendLine("    sys.exit(main())");
                    _output.AppendLine();
                    _output.AppendLine("if __name__ == \"__main__\":");
                    _output.AppendLine("    main()");
                    break;
            }
        }

        private void GenerateProgram(ProgramNode program)
        {
            switch (_targetPlatform)
            {
                case TargetPlatform.CIL:
                    GenerateCILProgram(program);
                    break;
                case TargetPlatform.C:
                    GenerateCProgram(program);
                    break;
                case TargetPlatform.JavaScript:
                    GenerateJavaScriptProgram(program);
                    break;
                case TargetPlatform.Python:
                    GeneratePythonProgram(program);
                    break;
                case TargetPlatform.Fminus:
                    GenerateFminusProgram(program);
                    break;
            }
        }

        #region CIL Code Generation
        private void GenerateCILProgram(ProgramNode program)
        {
            _output.AppendLine(".class private auto ansi Program");
            _output.AppendLine("{");
            _output.AppendLine("    .method static hidebysig void main() cil managed");
            _output.AppendLine("    {");
            _output.AppendLine("        .entrypoint");
            _output.AppendLine("        .maxstack 16");

            if (program.StartBlock != null)
            {
                foreach (var stmt in program.StartBlock.Statements)
                {
                    GenerateCILStatement(stmt);
                }
            }

            _output.AppendLine("        ret");
            _output.AppendLine("    }");
            _output.AppendLine("}");
        }

        private void GenerateCILStatement(StatementNode stmt)
        {
            switch (stmt)
            {
                case PrintlnStatementNode println:
                    if (println.Expression is StringLiteralNode str)
                    {
                        _output.AppendLine($"        ldstr \"{str.Value}\"");
                        _output.AppendLine("        call void [mscorlib]System.Console::WriteLine(string)");
                    }
                    else if (println.Expression is VariableNode var)
                    {
                        _output.AppendLine($"        ldloc '{var.Name}'");
                        _output.AppendLine("        call void [mscorlib]System.Console::WriteLine(object)");
                    }
                    break;

                case AssignmentNode assign:
                    if (assign.Value is StringLiteralNode strVal)
                    {
                        _output.AppendLine($"        ldstr \"{strVal.Value}\"");
                        _output.AppendLine($"        stloc '{assign.VariableName}'");
                    }
                    else if (assign.Value is NumberLiteralNode numVal)
                    {
                        _output.AppendLine($"        ldc.i4 {numVal.Value}");
                        _output.AppendLine($"        stloc '{assign.VariableName}'");
                    }
                    break;
            }
        }
        #endregion

        #region C Code Generation
        private void GenerateCProgram(ProgramNode program)
        {
            _output.AppendLine("int main() {");

            if (program.StartBlock != null)
            {
                foreach (var stmt in program.StartBlock.Statements)
                {
                    GenerateCStatement(stmt);
                }
            }

            _output.AppendLine("    return 0;");
            _output.AppendLine("}");
        }

        private void GenerateCStatement(StatementNode stmt)
        {
            switch (stmt)
            {
                case PrintlnStatementNode println:
                    if (println.Expression is StringLiteralNode str)
                    {
                        _output.AppendLine($"    printf(\"{str.Value}\\n\");");
                    }
                    else if (println.Expression is VariableNode var)
                    {
                        _output.AppendLine($"    printf(\"%s\\n\", {var.Name});");
                    }
                    break;

                case PrintStatementNode print:
                    if (print.Expression is StringLiteralNode str)
                    {
                        _output.AppendLine($"    printf(\"{str.Value}\");");
                    }
                    break;

                case AssignmentNode assign:
                    if (assign.Value is StringLiteralNode strVal)
                    {
                        _output.AppendLine($"    char* {assign.VariableName} = \"{strVal.Value}\";");
                    }
                    else if (assign.Value is NumberLiteralNode numVal)
                    {
                        _output.AppendLine($"    int {assign.VariableName} = {numVal.Value};");
                    }
                    break;

                case MemoryStatementNode memory:
                    _output.AppendLine($"    printf(\"{memory.Property}: %ld MB\\n\", 1024);");
                    break;
            }
        }
        #endregion

        #region JavaScript Code Generation
        private void GenerateJavaScriptProgram(ProgramNode program)
        {
            _output.AppendLine("(function() {");

            if (program.StartBlock != null)
            {
                foreach (var stmt in program.StartBlock.Statements)
                {
                    GenerateJavaScriptStatement(stmt);
                }
            }

            _output.AppendLine("})();");
        }

        private void GenerateJavaScriptStatement(StatementNode stmt)
        {
            switch (stmt)
            {
                case PrintlnStatementNode println:
                    if (println.Expression is StringLiteralNode str)
                    {
                        _output.AppendLine($"    console.log(\"{str.Value}\");");
                    }
                    else if (println.Expression is VariableNode var)
                    {
                        _output.AppendLine($"    console.log({var.Name});");
                    }
                    break;

                case AssignmentNode assign:
                    if (assign.Value is StringLiteralNode strVal)
                    {
                        _output.AppendLine($"    let {assign.VariableName} = \"{strVal.Value}\";");
                    }
                    else if (assign.Value is NumberLiteralNode numVal)
                    {
                        _output.AppendLine($"    let {assign.VariableName} = {numVal.Value};");
                    }
                    break;

                case MemoryStatementNode memory:
                    _output.AppendLine($"    console.log(\"{memory.Property}: 1024 MB\");");
                    break;
            }
        }
        #endregion

        #region Python Code Generation
        private void GeneratePythonProgram(ProgramNode program)
        {
            _output.AppendLine("def main():");

            if (program.StartBlock != null)
            {
                foreach (var stmt in program.StartBlock.Statements)
                {
                    GeneratePythonStatement(stmt);
                }
            }

            _output.AppendLine("    return 0");
        }

        private void GeneratePythonStatement(StatementNode stmt)
        {
            switch (stmt)
            {
                case PrintlnStatementNode println:
                    if (println.Expression is StringLiteralNode str)
                    {
                        _output.AppendLine($"    print(\"{str.Value}\")");
                    }
                    else if (println.Expression is VariableNode var)
                    {
                        _output.AppendLine($"    print({var.Name})");
                    }
                    break;

                case PrintStatementNode print:
                    if (print.Expression is StringLiteralNode str)
                    {
                        _output.AppendLine($"    print(\"{str.Value}\", end=\"\")");
                    }
                    break;

                case AssignmentNode assign:
                    if (assign.Value is StringLiteralNode strVal)
                    {
                        _output.AppendLine($"    {assign.VariableName} = \"{strVal.Value}\"");
                    }
                    else if (assign.Value is NumberLiteralNode numVal)
                    {
                        _output.AppendLine($"    {assign.VariableName} = {numVal.Value}");
                    }
                    break;

                case MemoryStatementNode memory:
                    _output.AppendLine($"    print(\"{memory.Property}: 1024 MB\")");
                    break;
            }
        }
        #endregion

        #region F-- Code Generation (Self-hosting)
        private void GenerateFminusProgram(ProgramNode program)
        {
            _output.AppendLine("import computer");
            _output.AppendLine("start() {");

            if (program.StartBlock != null)
            {
                foreach (var stmt in program.StartBlock.Statements)
                {
                    GenerateFminusStatement(stmt);
                }
            }

            _output.AppendLine("    return(0)");
            _output.AppendLine("    end()");
            _output.AppendLine("}");
        }

        private void GenerateFminusStatement(StatementNode stmt)
        {
            switch (stmt)
            {
                case PrintlnStatementNode println:
                    if (println.Expression is StringLiteralNode str)
                    {
                        _output.AppendLine($"    println(\"{str.Value}\")");
                    }
                    else if (println.Expression is VariableNode var)
                    {
                        _output.AppendLine($"    println({var.Name})");
                    }
                    break;

                case AssignmentNode assign:
                    if (assign.Value is StringLiteralNode strVal)
                    {
                        _output.AppendLine($"    {assign.VariableName} = \"{strVal.Value}\"");
                    }
                    else if (assign.Value is NumberLiteralNode numVal)
                    {
                        _output.AppendLine($"    {assign.VariableName} = {numVal.Value}");
                    }
                    break;
            }
        }
        #endregion

        #region Helpers
        private string NewTempVar() => $"_t{_tempVarCounter++}";
        private string NewLabel() => $"L{_labelCounter++}";

        private void Indent()
        {
            _indentLevel++;
        }

        private void Dedent()
        {
            if (_indentLevel > 0) _indentLevel--;
        }

        private string GetIndent() => new string(' ', _indentLevel * 4);
        #endregion
    }
}
