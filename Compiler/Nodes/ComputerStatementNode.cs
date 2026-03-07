namespace Fminusminus
{
    public class ComputerStatementNode : StatementNode
    {
        public string Property { get; set; } = string.Empty;
        public string Operation { get; set; } = string.Empty;
        public List<ExpressionNode> Parameters { get; set; } = new();
        
        public override void Print(int indent = 0)
        {
            Console.WriteLine($"{new string(' ', indent)}COMPUTER.{Property}({Operation})");
            foreach (var param in Parameters)
                param.Print(indent + 2);
        }
    }
}
