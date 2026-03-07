using System;
using System.Collections.Generic;

namespace Fminusminus
{
    public abstract class AstNode
    {
        public int Line { get; set; }
        public int Column { get; set; }
        
        public virtual void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}{GetType().Name}");
        }
    }

    public class ProgramNode : AstNode
    {
        public bool HasImportComputer { get; set; }
        public StartBlockNode? StartBlock { get; set; }
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}Program");
            if (HasImportComputer)
                Console.WriteLine($"{new string(' ', indent + 2)}IMPORT computer");
            StartBlock?.Print(indent + 2);
        }
    }

    public class StartBlockNode : AstNode
    {
        public List<StatementNode> Statements { get; set; } = new();
        public bool HasReturn { get; set; }
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}StartBlock");
            foreach (var stmt in Statements)
                stmt.Print(indent + 2);
        }
    }

    public abstract class StatementNode : AstNode { }

    public class PrintlnStatementNode : StatementNode
    {
        public ExpressionNode? Expression { get; set; }
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}PRINTLN");
            Expression?.Print(indent + 2);
        }
    }

    public class PrintStatementNode : StatementNode
    {
        public ExpressionNode? Expression { get; set; }
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}PRINT");
            Expression?.Print(indent + 2);
        }
    }

    public class ReturnStatementNode : StatementNode
    {
        public int ReturnCode { get; set; }
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}RETURN {ReturnCode}");
        }
    }

    // 👇 ĐÃ XÓA EndStatementNode

    public class AssignmentNode : StatementNode
    {
        public string VariableName { get; set; } = string.Empty;
        public ExpressionNode? Value { get; set; }
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}ASSIGN {VariableName} =");
            Value?.Print(indent + 2);
        }
    }

    public class ComputerCallNode : StatementNode
    {
        public string Method { get; set; } = string.Empty;
        public List<ExpressionNode> Arguments { get; set; } = new();
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}COMPUTER.{Method}()");
            foreach (var arg in Arguments)
                arg.Print(indent + 2);
        }
    }

    public class AtBlockNode : StatementNode
    {
        public ExpressionNode? FileName { get; set; }
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

    public abstract class ExpressionNode : AstNode { }

    public class StringLiteralNode : ExpressionNode
    {
        public string Value { get; set; } = string.Empty;
        public bool IsInterpolated { get; set; }
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}STRING: \"{Value}\" {(IsInterpolated ? "(interpolated)" : "")}");
        }
    }

    public class NumberLiteralNode : ExpressionNode
    {
        public double Value { get; set; }
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}NUMBER: {Value}");
        }
    }

    public class VariableNode : ExpressionNode
    {
        public string Name { get; set; } = string.Empty;
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}VARIABLE: {Name}");
        }
    }
}
