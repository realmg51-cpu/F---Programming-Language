using System;
using System.Collections.Generic;

namespace Fminusminus
{
    /// <summary>
    /// Base class cho tất cả AST nodes
    /// </summary>
    public abstract class AstNode
    {
        public int Line { get; set; }
        public int Column { get; set; }
        
        public virtual void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}{GetType().Name}");
        }
    }

    /// <summary>
    /// Chương trình F-- hoàn chỉnh
    /// </summary>
    public class ProgramNode : AstNode
    {
        public bool HasImportComputer { get; set; }
        public StartBlockNode StartBlock { get; set; }
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}Program");
            if (HasImportComputer)
                Console.WriteLine($"{new string(' ', indent + 2)}IMPORT computer");
            StartBlock?.Print(indent + 2);
        }
    }

    /// <summary>
    /// Start block: start() { ... }
    /// </summary>
    public class StartBlockNode : AstNode
    {
        public List<StatementNode> Statements { get; set; } = new();
        public bool HasReturn { get; set; }
        public bool HasEnd { get; set; }
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}StartBlock");
            foreach (var stmt in Statements)
                stmt.Print(indent + 2);
            
            if (HasReturn)
                Console.WriteLine($"{new string(' ', indent + 2)}RETURN");
            if (HasEnd)
                Console.WriteLine($"{new string(' ', indent + 2)}END");
        }
    }

    /// <summary>
    /// Base class cho statements
    /// </summary>
    public abstract class StatementNode : AstNode { }

    /// <summary>
    /// Print statement (có newline)
    /// </summary>
    public class PrintlnStatementNode : StatementNode
    {
        public ExpressionNode Expression { get; set; }
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}PRINTLN");
            Expression?.Print(indent + 2);
        }
    }

    /// <summary>
    /// Print statement (không newline)
    /// </summary>
    public class PrintStatementNode : StatementNode
    {
        public ExpressionNode Expression { get; set; }
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}PRINT");
            Expression?.Print(indent + 2);
        }
    }

    /// <summary>
    /// Return statement
    /// </summary>
    public class ReturnStatementNode : StatementNode
    {
        public int ReturnCode { get; set; }
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}RETURN {ReturnCode}");
        }
    }

    /// <summary>
    /// End statement
    /// </summary>
    public class EndStatementNode : StatementNode
    {
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}END");
        }
    }

    /// <summary>
    /// Assignment: variable = value
    /// </summary>
    public class AssignmentNode : StatementNode
    {
        public string VariableName { get; set; }
        public ExpressionNode Value { get; set; }
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}ASSIGN {VariableName} =");
            Value?.Print(indent + 2);
        }
    }

    /// <summary>
    /// IO operations
    /// </summary>
    public class IOStatementNode : StatementNode
    {
        public string Operation { get; set; }  // cfile, println, save, etc
        public List<ExpressionNode> Parameters { get; set; } = new();
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}IO.{Operation}");
            foreach (var param in Parameters)
                param.Print(indent + 2);
        }
    }

    /// <summary>
    /// At block: at "file.txt" { ... }
    /// </summary>
    public class AtBlockNode : StatementNode
    {
        public ExpressionNode FileName { get; set; }
        public List<StatementNode> Statements { get; set; } = new();
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}AT");
            FileName?.Print(indent + 2);
            Console.WriteLine($"{new string(' ', indent + 2)}BLOCK");
            foreach (var stmt in Statements)
                stmt.Print(indent + 4);
        }
    }

    /// <summary>
    /// Memory access: memory.memoryleft
    /// </summary>
    public class MemoryStatementNode : StatementNode
    {
        public string Property { get; set; }  // memoryleft, memoryused, memorytotal
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}MEMORY.{Property}");
        }
    }

    /// <summary>
    /// Base class cho expressions
    /// </summary>
    public abstract class ExpressionNode : AstNode { }

    /// <summary>
    /// String literal
    /// </summary>
    public class StringLiteralNode : ExpressionNode
    {
        public string Value { get; set; }
        public bool IsInterpolated { get; set; }
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}STRING: \"{Value}\" {(IsInterpolated ? "(interpolated)" : "")}");
        }
    }

    /// <summary>
    /// Number literal
    /// </summary>
    public class NumberLiteralNode : ExpressionNode
    {
        public double Value { get; set; }
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}NUMBER: {Value}");
        }
    }

    /// <summary>
    /// Variable reference
    /// </summary>
    public class VariableNode : ExpressionNode
    {
        public string Name { get; set; }
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}VARIABLE: {Name}");
        }
    }
}
