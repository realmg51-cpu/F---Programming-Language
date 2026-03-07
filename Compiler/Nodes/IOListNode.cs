namespace Fminusminus
{
    /// <summary>
    /// IO operation node for listfile and other IO operations
    /// </summary>
    public class IOListNode : StatementNode
    {
        public string Operation { get; set; } = "listfile";
        public ExpressionNode? Path { get; set; }
        public bool UseOSPath { get; set; }
        
        public IOListNode() { }
        
        public IOListNode(string operation, int line, int column)
        {
            Operation = operation;
            Line = line;
            Column = column;
        }
        
        public IOListNode(string operation, ExpressionNode path, int line, int column)
        {
            Operation = operation;
            
            // Validate path type
            if (path is not StringLiteralNode and not VariableNode)
                throw new ArgumentException("Path must be a string literal or variable");
                
            Path = path;
            Line = line;
            Column = column;
        }
        
        public override void Print(int indent = 0)
        {
            string indentStr = new string(' ', indent);
            Console.Write($"{indentStr}IO.{Operation.ToUpper()}(");
            
            if (UseOSPath)
            {
                Console.Write("OS.path");
            }
            else if (Path != null)
            {
                if (Path is StringLiteralNode strNode)
                    Console.Write($"\"{strNode.Value}\"");
                else if (Path is VariableNode varNode)
                    Console.Write(varNode.Name);
                else
                    Console.Write("?");
            }
            else
            {
                Console.Write(".");
            }
            
            Console.WriteLine($")");
        }
    }
}
