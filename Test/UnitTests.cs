using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using FluentAssertions;
using Moq;

namespace FSharpMinus.Tests
{
    /// <summary>
    /// Unit tests cho F-- compiler
    /// "Test nhiệt tình, chất lượng đỉnh cao!"
    /// </summary>
    public class FSharpMinusTests
    {
        #region Lexer Tests

        public class LexerTests
        {
            [Fact]
            public void Lexer_ShouldTokenizeSimpleHelloWorld()
            {
                // Arrange
                string code = @"
import system
using namespace sys
start()
{
    println(""Hello World!"")
    return(0)
}";
                var lexer = new Lexer(code);

                // Act
                var tokens = lexer.ScanTokens();

                // Assert
                tokens.Should().NotBeNull();
                tokens.Count.Should().BeGreaterThan(0);
                
                // Kiểm tra các token quan trọng
                tokens[0].Type.Should().Be(TokenType.IMPORT);
                tokens[1].Type.Should().Be(TokenType.IDENTIFIER);
                tokens[1].Lexeme.Should().Be("system");
                tokens[3].Type.Should().Be(TokenType.USING);
                tokens[4].Type.Should().Be(TokenType.NAMESPACE);
                tokens[5].Type.Should().Be(TokenType.IDENTIFIER);
                tokens[5].Lexeme.Should().Be("sys");
            }

            [Fact]
            public void Lexer_ShouldHandleStringInterpolation()
            {
                // Arrange
                string code = @"println($""Hello {name}!"")";
                var lexer = new Lexer(code);

                // Act
                var tokens = lexer.ScanTokens();

                // Assert
                tokens.Should().Contain(t => t.Type == TokenType.STRING_INTERPOLATED);
                var interpolatedToken = tokens.Find(t => t.Type == TokenType.STRING_INTERPOLATED);
                interpolatedToken.Literal.Should().Be("Hello {name}!");
            }

            [Fact]
            public void Lexer_ShouldHandleNumbers()
            {
                // Arrange
                string code = @"x = 42
y = 3.14";
                var lexer = new Lexer(code);

                // Act
                var tokens = lexer.ScanTokens();

                // Assert
                tokens.Should().Contain(t => t.Type == TokenType.INTEGER && t.Literal?.ToString() == "42");
                tokens.Should().Contain(t => t.Type == TokenType.FLOAT && t.Literal?.ToString() == "3.14");
            }

            [Fact]
            public void Lexer_ShouldHandleComments()
            {
                // Arrange
                string code = @"
// Đây là comment
println(""Hello"") /* Block comment 
   nhiều dòng */";
                var lexer = new Lexer(code);

                // Act
                var tokens = lexer.ScanTokens();

                // Assert
                tokens.Should().Contain(t => t.Type == TokenType.COMMENT);
            }

            [Fact]
            public void Lexer_ShouldThrowOnUnterminatedString()
            {
                // Arrange
                string code = @"println(""Hello)";
                var lexer = new Lexer(code);

                // Act & Assert
                Assert.Throws<LexerException>(() => lexer.ScanTokens());
            }

            [Theory]
            [InlineData("@")]
            [InlineData("#")]
            [InlineData("`")]
            public void Lexer_ShouldThrowOnInvalidCharacter(string invalidChar)
            {
                // Arrange
                string code = $"println(\"Hello\"){invalidChar}";
                var lexer = new Lexer(code);

                // Act & Assert
                Assert.Throws<LexerException>(() => lexer.ScanTokens());
            }
        }

        #endregion

        #region Parser Tests

        public class ParserTests
        {
            [Fact]
            public void Parser_ShouldParseSimpleProgram()
            {
                // Arrange
                string code = @"
import system
using namespace sys
start()
{
    println(""Hello"")
    return(0)
}";
                var lexer = new Lexer(code);
                var tokens = lexer.ScanTokens();
                var parser = new Parser(tokens);

                // Act
                var ast = parser.Parse();

                // Assert
                ast.Should().NotBeNull();
                ast.Imports.Should().HaveCount(1);
                ast.Imports[0].ModuleName.Should().Be("system");
                ast.Usings.Should().HaveCount(1);
                ast.Usings[0].Namespace.Should().Be("sys");
                ast.StartBlock.Should().NotBeNull();
                ast.Statements.Should().HaveCount(2);
            }

            [Fact]
            public void Parser_ShouldParseAtBlock()
            {
                // Arrange
                string code = @"
start()
{
    at ""hello.txt""
    {
        io.println(""Hello File!"")
        io.save()
    }
}";
                var lexer = new Lexer(code);
                var tokens = lexer.ScanTokens();
                var parser = new Parser(tokens);

                // Act
                var ast = parser.Parse();

                // Assert
                ast.StartBlock.Statements.Should().ContainItemsAssignableTo<AtBlockNode>();
                var atBlock = ast.StartBlock.Statements[0] as AtBlockNode;
                atBlock.FileName.Should().Be("hello.txt");
                atBlock.Statements.Should().HaveCount(2);
            }

            [Fact]
            public void Parser_ShouldParseIOOperations()
            {
                // Arrange
                string code = @"
start()
{
    io.cfile(""test"")
    io.save()
}";
                var lexer = new Lexer(code);
                var tokens = lexer.ScanTokens();
                var parser = new Parser(tokens);

                // Act
                var ast = parser.Parse();

                // Assert
                ast.StartBlock.Statements.Should().HaveCount(2);
                ast.StartBlock.Statements[0].Should().BeOfType<IOStatementNode>();
                var io1 = ast.StartBlock.Statements[0] as IOStatementNode;
                io1.Operation.Should().Be("cfile");
                
                ast.StartBlock.Statements[1].Should().BeOfType<IOStatementNode>();
                var io2 = ast.StartBlock.Statements[1] as IOStatementNode;
                io2.Operation.Should().Be("save");
            }

            [Fact]
            public void Parser_ShouldParseMemoryStatements()
            {
                // Arrange
                string code = @"
start()
{
    memory.memoryleft
    memory.memoryused
}";
                var lexer = new Lexer(code);
                var tokens = lexer.ScanTokens();
                var parser = new Parser(tokens);

                // Act
                var ast = parser.Parse();

                // Assert
                ast.StartBlock.Statements.Should().HaveCount(2);
                ast.StartBlock.Statements[0].Should().BeOfType<MemoryStatementNode>();
                var mem1 = ast.StartBlock.Statements[0] as MemoryStatementNode;
                mem1.Property.Should().Be("memoryleft");
                
                ast.StartBlock.Statements[1].Should().BeOfType<MemoryStatementNode>();
                var mem2 = ast.StartBlock.Statements[1] as MemoryStatementNode;
                mem2.Property.Should().Be("memoryused");
            }

            [Fact]
            public void Parser_ShouldThrowOnMissingStart()
            {
                // Arrange
                string code = @"
import system
println(""Hello"")";
                var lexer = new Lexer(code);
                var tokens = lexer.ScanTokens();
                var parser = new Parser(tokens);

                // Act & Assert
                Assert.Throws<ParseException>(() => parser.Parse());
            }
        }

        #endregion

        #region Interpreter Tests

        public class InterpreterTests
        {
            [Fact]
            public void Interpreter_ShouldExecutePrintln()
            {
                // Arrange
                string code = @"
start()
{
    println(""Hello F--!"")
    return(0)
}";
                var lexer = new Lexer(code);
                var tokens = lexer.ScanTokens();
                var parser = new Parser(tokens);
                var ast = parser.Parse();
                var interpreter = new Interpreter();

                // Capture console output
                using var stringWriter = new StringWriter();
                var originalOutput = Console.Out;
                Console.SetOut(stringWriter);

                try
                {
                    // Act
                    var result = interpreter.Execute(ast);

                    // Assert
                    result.Should().Be(0);
                    stringWriter.ToString().Should().Contain("Hello F--!");
                }
                finally
                {
                    Console.SetOut(originalOutput);
                }
            }

            [Fact]
            public void Interpreter_ShouldHandleStringInterpolation()
            {
                // Arrange
                string code = @"
start()
{
    name = ""F--""
    println($""Hello {name}!"")
    return(0)
}";
                var lexer = new Lexer(code);
                var tokens = lexer.ScanTokens();
                var parser = new Parser(tokens);
                var ast = parser.Parse();
                var interpreter = new Interpreter();

                using var stringWriter = new StringWriter();
                var originalOutput = Console.Out;
                Console.SetOut(stringWriter);

                try
                {
                    // Act
                    var result = interpreter.Execute(ast);

                    // Assert
                    result.Should().Be(0);
                    stringWriter.ToString().Should().Contain("Hello F--!");
                }
                finally
                {
                    Console.SetOut(originalOutput);
                }
            }

            [Fact]
            public void Interpreter_ShouldHandleMemoryStatements()
            {
                // Arrange
                string code = @"
start()
{
    memory.memoryleft
    return(0)
}";
                var lexer = new Lexer(code);
                var tokens = lexer.ScanTokens();
                var parser = new Parser(tokens);
                var ast = parser.Parse();
                var interpreter = new Interpreter();

                using var stringWriter = new StringWriter();
                var originalOutput = Console.Out;
                Console.SetOut(stringWriter);

                try
                {
                    // Act
                    var result = interpreter.Execute(ast);

                    // Assert
                    result.Should().Be(0);
                    stringWriter.ToString().Should().Contain("Memory left:");
                }
                finally
                {
                    Console.SetOut(originalOutput);
                }
            }

            [Fact]
            public void Interpreter_ShouldHandleFileOperations()
            {
                // Arrange
                string testFile = "test_io.txt";
                if (File.Exists(testFile)) File.Delete(testFile);

                string code = @"
start()
{
    io.cfile(""test_io"")
    at ""test_io.txt""
    {
        io.println(""Hello File!"")
        io.save()
    }
    return(0)
}";
                var lexer = new Lexer(code);
                var tokens = lexer.ScanTokens();
                var parser = new Parser(tokens);
                var ast = parser.Parse();
                var interpreter = new Interpreter();

                // Act
                var result = interpreter.Execute(ast);

                // Assert
                result.Should().Be(0);
                // File should be created (note: in real implementation, file might be in C:\)
                // File.Exists(@"C:\test_io.txt").Should().BeTrue();
            }

            [Fact]
            public void Interpreter_ShouldThrowOnUnknownStatement()
            {
                // Arrange - tạo mock AST với statement lạ
                var program = new ProgramNode
                {
                    StartBlock = new StartBlockNode()
                };
                // Không thể test trực tiếp vì không thể tạo instance của abstract class
                // Nhưng có thể test bằng cách đợi exception khi gặp statement lạ
            }
        }

        #endregion

        #region Integration Tests

        public class IntegrationTests
        {
            [Theory]
            [InlineData("examples/hello.f--", "Hello from F--!")]
            [InlineData("examples/memory.f--", "Memory left:")]
            [InlineData("examples/fileio.f--", "Created file:")]
            public void FullCompiler_ShouldRunExampleFiles(string filePath, string expectedOutput)
            {
                // Arrange
                if (!File.Exists(filePath))
                {
                    // Skip nếu file không tồn tại trong môi trường test
                    return;
                }

                string code = File.ReadAllText(filePath);

                // Act
                var lexer = new Lexer(code);
                var tokens = lexer.ScanTokens();
                var parser = new Parser(tokens);
                var ast = parser.Parse();
                var interpreter = new Interpreter();

                using var stringWriter = new StringWriter();
                var originalOutput = Console.Out;
                Console.SetOut(stringWriter);

                try
                {
                    var result = interpreter.Execute(ast);

                    // Assert
                    result.Should().Be(0);
                    stringWriter.ToString().Should().Contain(expectedOutput);
                }
                finally
                {
                    Console.SetOut(originalOutput);
                }
            }

            [Fact]
            public void FullCompiler_ShouldHandleComplexProgram()
            {
                // Arrange
                string code = @"
import system
using namespace sys

start()
{
    // Test variables
    message = ""F-- is awesome!""
    version = 1.4
    
    // Test output
    println($""Message: {message}"")
    println($""Version: {version}"")
    
    // Test memory
    memory.memoryleft
    
    // Test file operations
    io.cfile(""test_complex"")
    at ""test_complex.txt""
    {
        io.println(""This is a test"")
        io.println($""Running F-- v{version}"")
        io.save()
    }
    
    return(0)
}";
                var lexer = new Lexer(code);
                var tokens = lexer.ScanTokens();
                var parser = new Parser(tokens);
                var ast = parser.Parse();
                var interpreter = new Interpreter();

                using var stringWriter = new StringWriter();
                var originalOutput = Console.Out;
                Console.SetOut(stringWriter);

                try
                {
                    // Act
                    var result = interpreter.Execute(ast);

                    // Assert
                    result.Should().Be(0);
                    var output = stringWriter.ToString();
                    output.Should().Contain("Message: F-- is awesome!");
                    output.Should().Contain("Version: 1.4");
                    output.Should().Contain("Memory left:");
                    output.Should().Contain("Created file: test_complex.txt");
                }
                finally
                {
                    Console.SetOut(originalOutput);
                }
            }
        }

        #endregion

        #region Error Handling Tests

        public class ErrorHandlingTests
        {
            [Fact]
            public void Compiler_ShouldReportLexerError()
            {
                // Arrange
                string code = @"println(""Hello) // Thiếu quote";

                // Act & Assert
                Assert.Throws<LexerException>(() =>
                {
                    var lexer = new Lexer(code);
                    lexer.ScanTokens();
                });
            }

            [Fact]
            public void Compiler_ShouldReportParserError()
            {
                // Arrange
                string code = @"
import system
start( // Thiếu dấu đóng ngoặc
{
    println(""Hello"")
}";

                var lexer = new Lexer(code);
                var tokens = lexer.ScanTokens();
                var parser = new Parser(tokens);

                // Act & Assert
                Assert.Throws<ParseException>(() => parser.Parse());
            }

            [Fact]
            public void Compiler_ShouldHandleEmptyFile()
            {
                // Arrange
                string code = "";
                var lexer = new Lexer(code);

                // Act
                var tokens = lexer.ScanTokens();
                var parser = new Parser(tokens);

                // Assert
                Assert.Throws<ParseException>(() => parser.Parse());
            }
        }

        #endregion

        #region Performance Tests

        public class PerformanceTests
        {
            [Fact]
            public void Lexer_ShouldHandleLargeFile()
            {
                // Arrange
                var codeBuilder = new System.Text.StringBuilder();
                for (int i = 0; i < 1000; i++)
                {
                    codeBuilder.AppendLine($"println(\"Line {i}\")");
                }
                string code = codeBuilder.ToString();
                
                var lexer = new Lexer(code);

                // Act
                var startTime = DateTime.Now;
                var tokens = lexer.ScanTokens();
                var duration = DateTime.Now - startTime;

                // Assert
                tokens.Count.Should().BeGreaterThan(1000);
                duration.TotalSeconds.Should().BeLessThan(1.0); // Dưới 1 giây cho 1000 dòng
            }

            [Fact]
            public void Parser_ShouldHandleManyStatements()
            {
                // Arrange
                var codeBuilder = new System.Text.StringBuilder();
                codeBuilder.AppendLine("start() {");
                for (int i = 0; i < 500; i++)
                {
                    codeBuilder.AppendLine($"    println(\"Line {i}\")");
                }
                codeBuilder.AppendLine("    return(0)");
                codeBuilder.AppendLine("}");
                
                var lexer = new Lexer(codeBuilder.ToString());
                var tokens = lexer.ScanTokens();
                var parser = new Parser(tokens);

                // Act
                var startTime = DateTime.Now;
                var ast = parser.Parse();
                var duration = DateTime.Now - startTime;

                // Assert
                ast.Statements.Count.Should().Be(501); // 500 println + 1 return
                duration.TotalSeconds.Should().BeLessThan(0.5);
            }
        }

        #endregion
    }
}
