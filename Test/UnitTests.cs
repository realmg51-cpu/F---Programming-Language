using Xunit;
using System.IO;
using System;

namespace Fminusminus.Tests
{
    public class LexerTests
    {
        [Fact]
        public void TestImportComputer()
        {
            string code = "import computer";
            var lexer = new Lexer(code);
            var tokens = lexer.ScanTokens();
            
            Assert.Equal(TokenType.IMPORT, tokens[0].Type);
            Assert.Equal(TokenType.COMPUTER, tokens[1].Type);
        }

        [Fact]
        public void TestPrintAndPrintln()
        {
            string code = @"
import computer
start()
{
    print(""Hello"")
    println(""World"")
    return(0)
    end()
}";
            var lexer = new Lexer(code);
            var tokens = lexer.ScanTokens();
            var parser = new Parser(tokens);
            var ast = parser.Parse();
            
            Assert.NotNull(ast.StartBlock);
            Assert.Equal(4, ast.StartBlock.Statements.Count); // print, println, return, end
        }

        [Fact]
        public void TestRequiredReturnAndEnd()
        {
            string code = @"
import computer
start()
{
    println(""Missing return and end"")
}";
            var lexer = new Lexer(code);
            var tokens = lexer.ScanTokens();
            var parser = new Parser(tokens);
            
            Assert.Throws<Exception>(() => parser.Parse());
        }
    }
}
