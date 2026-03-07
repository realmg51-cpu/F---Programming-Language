using System;
using System.Collections.Generic;
using Fminusminus.Errors;

namespace Fminusminus
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _current = 0;
        private readonly List<SyntaxError> _errors = new();

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        public ProgramNode Parse()
        {
            var program = new ProgramNode();
            
            try
            {
                // Skip leading comments and newlines
                SkipCommentsAndNewlines();

                if (!Match(TokenType.IMPORT))
                    throw SyntaxError.MissingToken(Peek().Line, Peek().Column, "import");
                
                if (!Match(TokenType.COMPUTER))
                    throw SyntaxError.MissingToken(Previous().Line, Previous().Column + 7, "computer");
                
                program.HasImportComputer = true;
                
                SkipCommentsAndNewlines();
                
                if (!Match(TokenType.START))
                    throw SyntaxError.MissingToken(Peek().Line, Peek().Column, "start");
                
                if (!Match(TokenType.LPAREN))
                    throw SyntaxError.MissingToken(Previous().Line, Previous().Column + 5, "(");
                
                if (!Match(TokenType.RPAREN))
                    throw SyntaxError.MissingToken(Previous().Line, Previous().Column + 1, ")");
                
                program.StartBlock = ParseStartBlock();
                
                SkipCommentsAndNewlines();
                
                if (Peek().Type != TokenType.EOF)
                    throw SyntaxError.UnexpectedSymbol(Peek().Line, Peek().Column, Peek().Lexeme[0]);
            }
            catch (SyntaxError ex)
            {
                _errors.Add(ex);
                throw new AggregateException("Parser errors occurred", _errors);
            }
            
            return program;
        }

        private StartBlockNode ParseStartBlock()
        {
            var block = new StartBlockNode();
            
            SkipCommentsAndNewlines();
            
            if (!Match(TokenType.LBRACE))
                throw SyntaxError.MissingToken(Peek().Line, Peek().Column, "{");
            
            while (!Check(TokenType.RBRACE) && !IsAtEnd())
            {
                var stmt = ParseStatement();
                if (stmt != null)
                    block.Statements.Add(stmt);
                
                SkipCommentsAndNewlines();
            }
            
            if (!Match(TokenType.RBRACE))
                throw SyntaxError.MissingToken(Peek().Line, Peek().Column, "}");
            
            bool hasReturn = false;
            bool hasEnd = false;
            int returnIndex = -1;
            int endIndex = -1;
            
            for (int i = 0; i < block.Statements.Count; i++)
            {
                if (block.Statements[i] is ReturnStatementNode)
                {
                    hasReturn = true;
                    returnIndex = i;
                }
                if (block.Statements[i] is EndStatementNode)
                {
                    hasEnd = true;
                    endIndex = i;
                }
            }
            
            if (!hasReturn)
                throw SyntaxError.MissingToken(Peek().Line, Peek().Column, "return()");
            
            if (!hasEnd)
                throw SyntaxError.MissingToken(Peek().Line, Peek().Column, "end()");
            
            if (returnIndex > endIndex)
                throw new SyntaxError("return() must be before end()", 
                    block.Statements[endIndex].Line, block.Statements[endIndex].Column, "");
            
            block.HasReturn = hasReturn;
            block.HasEnd = hasEnd;
            
            return block;
        }

        /// <summary>
        /// Helper method to skip comments and newlines
        /// </summary>
        private void SkipCommentsAndNewlines()
        {
            while (true)
            {
                if (Match(TokenType.NEWLINE))
                    continue;
                if (Match(TokenType.COMMENT))
                    continue;
                break;
            }
        }

        private StatementNode? ParseStatement()
        {
            SkipCommentsAndNewlines();
            
            if (IsAtEnd()) return null;
            
            var token = Peek();
            
            try
            {
                switch (token.Type)
                {
                    case TokenType.PRINTLN:
                        return ParsePrintln();
                        
                    case TokenType.PRINT:
                        return ParsePrint();
                        
                    case TokenType.RETURN:
                        return ParseReturn();
                        
                    case TokenType.END:
                        return ParseEnd();
                        
                    case TokenType.IDENTIFIER:
                        return ParseIdentifierStatement();
                        
                    case TokenType.AT:
                        return ParseAtBlock();
                        
                    case TokenType.IO:
                        return ParseIOStatement();
                        
                    case TokenType.COMPUTER:
                        return ParseComputerStatement();
                        
                    case TokenType.MEMORY:
                        return ParseMemoryStatement();
                        
                    case TokenType.COMMENT:
                        Advance(); // Skip comment
                        return null;
                        
                    default:
                        throw SyntaxError.UnexpectedSymbol(token.Line, token.Column, token.Lexeme[0]);
                }
            }
            catch (SyntaxError ex)
            {
                _errors.Add(ex);
                while (!Check(TokenType.NEWLINE) && !IsAtEnd()) Advance();
                return null;
            }
        }

        private PrintlnStatementNode ParsePrintln()
        {
            Advance();
            var node = new PrintlnStatementNode();
            
            if (!Match(TokenType.LPAREN))
                throw SyntaxError.MissingToken(Previous().Line, Previous().Column + 6, "(");
            
            node.Expression = ParseExpression();
            
            if (!Match(TokenType.RPAREN))
                throw SyntaxError.MissingToken(Previous().Line, Previous().Column + 1, ")");
            
            return node;
        }

        private PrintStatementNode ParsePrint()
        {
            Advance();
            var node = new PrintStatementNode();
            
            if (!Match(TokenType.LPAREN))
                throw SyntaxError.MissingToken(Previous().Line, Previous().Column + 5, "(");
            
            node.Expression = ParseExpression();
            
            if (!Match(TokenType.RPAREN))
                throw SyntaxError.MissingToken(Previous().Line, Previous().Column + 1, ")");
            
            return node;
        }

        private ReturnStatementNode ParseReturn()
        {
            Advance();
            var node = new ReturnStatementNode();
            
            if (!Match(TokenType.LPAREN))
                throw SyntaxError.MissingToken(Previous().Line, Previous().Column + 6, "(");
            
            if (Check(TokenType.NUMBER))
            {
                node.ReturnCode = Convert.ToInt32(Peek().Literal);
                Advance();
            }
            else
            {
                throw SyntaxError.MissingToken(Peek().Line, Peek().Column, "number");
            }
            
            if (!Match(TokenType.RPAREN))
                throw SyntaxError.MissingToken(Previous().Line, Previous().Column + 1, ")");
            
            return node;
        }

        private EndStatementNode ParseEnd()
        {
            Advance();
            var node = new EndStatementNode();
            
            if (!Match(TokenType.LPAREN))
                throw SyntaxError.MissingToken(Previous().Line, Previous().Column + 3, "(");
            
            if (!Match(TokenType.RPAREN))
                throw SyntaxError.MissingToken(Previous().Line, Previous().Column + 1, ")");
            
            return node;
        }

        private StatementNode? ParseIdentifierStatement()
        {
            string identifier = Peek().Lexeme;
            int line = Peek().Line;
            int col = Peek().Column;
            Advance();
            
            if (Match(TokenType.ASSIGN))
            {
                var node = new AssignmentNode { VariableName = identifier };
                node.Value = ParseExpression();
                return node;
            }
            
            throw SyntaxError.InvalidToken(line, col, identifier);
        }

        private AtBlockNode ParseAtBlock()
        {
            Advance();
            var node = new AtBlockNode();
            
            node.FileName = ParseExpression();
            
            if (!(node.FileName is StringLiteralNode))
                throw new SyntaxError("Filename must be a string", 
                    node.FileName!.Line, node.FileName.Column, "");
            
            SkipCommentsAndNewlines();
            
            if (!Match(TokenType.LBRACE))
                throw SyntaxError.MissingToken(Previous().Line, Previous().Column + 1, "{");
            
            while (!Check(TokenType.RBRACE) && !IsAtEnd())
            {
                var stmt = ParseStatement();
                if (stmt != null)
                    node.Statements.Add(stmt);
                
                SkipCommentsAndNewlines();
            }
            
            if (!Match(TokenType.RBRACE))
                throw SyntaxError.MissingToken(Peek().Line, Peek().Column, "}");
            
            return node;
        }

        private IOStatementNode ParseIOStatement()
        {
            Advance();
            var node = new IOStatementNode();
            
            if (!Match(TokenType.DOT))
                throw SyntaxError.MissingToken(Previous().Line, Previous().Column + 2, ".");
            
            if (Check(TokenType.IDENTIFIER))
            {
                node.Operation = Peek().Lexeme;
                Advance();
            }
            else
            {
                throw SyntaxError.MissingToken(Peek().Line, Peek().Column, "operation");
            }
            
            if (Match(TokenType.LPAREN))
            {
                while (!Check(TokenType.RPAREN) && !IsAtEnd())
                {
                    node.Parameters.Add(ParseExpression()!);
                    Match(TokenType.COMMA);
                }
                
                if (!Match(TokenType.RPAREN))
                    throw SyntaxError.MissingToken(Previous().Line, Previous().Column + 1, ")");
            }
            
            return node;
        }

        private ComputerStatementNode ParseComputerStatement()
        {
            Advance();
            var node = new ComputerStatementNode();
            
            if (!Match(TokenType.DOT))
                throw SyntaxError.MissingToken(Previous().Line, Previous().Column + 8, ".");
            
            if (Check(TokenType.IDENTIFIER))
            {
                string property = Peek().Lexeme;
                Advance();
                
                switch (property)
                {
                    case "systeminfo":
                        node.Property = "systeminfo";
                        
                        if (!Match(TokenType.LPAREN))
                            throw SyntaxError.MissingToken(Previous().Line, Previous().Column + 10, "(");
                        
                        if (Check(TokenType.IDENTIFIER) && Peek().Lexeme == "get")
                        {
                            Advance();
                            node.Operation = "get";
                        }
                        else
                        {
                            throw SyntaxError.MissingToken(Peek().Line, Peek().Column, "get");
                        }
                        
                        if (!Match(TokenType.RPAREN))
                            throw SyntaxError.MissingToken(Previous().Line, Previous().Column + 3, ")");
                        break;
                        
                    default:
                        throw new SyntaxError($"Unknown computer property: {property}", 
                            Previous().Line, Previous().Column, property);
                }
            }
            else
            {
                throw SyntaxError.MissingToken(Peek().Line, Peek().Column, "property");
            }
            
            return node;
        }

        private MemoryStatementNode ParseMemoryStatement()
        {
            Advance();
            var node = new MemoryStatementNode();
            
            if (!Match(TokenType.DOT))
                throw SyntaxError.MissingToken(Previous().Line, Previous().Column + 6, ".");
            
            if (Check(TokenType.IDENTIFIER))
            {
                node.Property = Peek().Lexeme;
                Advance();
                
                if (node.Property != "memoryleft" && node.Property != "memoryused" && node.Property != "memorytotal")
                    throw new SyntaxError($"Unknown memory property: {node.Property}", 
                        Previous().Line, Previous().Column, node.Property);
            }
            else
            {
                throw SyntaxError.MissingToken(Peek().Line, Peek().Column, "property");
            }
            
            return node;
        }

        private ExpressionNode? ParseExpression()
        {
            if (Check(TokenType.STRING))
            {
                var node = new StringLiteralNode { 
                    Value = Peek().Literal?.ToString() ?? "",
                    IsInterpolated = false
                };
                Advance();
                return node;
            }
            
            if (Check(TokenType.STRING_INTERPOLATED))
            {
                var node = new StringLiteralNode { 
                    Value = Peek().Literal?.ToString() ?? "",
                    IsInterpolated = true
                };
                Advance();
                return node;
            }
            
            if (Check(TokenType.NUMBER))
            {
                var node = new NumberLiteralNode { 
                    Value = Convert.ToDouble(Peek().Literal ?? 0)
                };
                Advance();
                return node;
            }
            
            if (Check(TokenType.IDENTIFIER))
            {
                var node = new VariableNode { Name = Peek().Lexeme };
                Advance();
                return node;
            }
            
            throw SyntaxError.MissingToken(Peek().Line, Peek().Column, "expression");
        }

        private bool Match(TokenType type)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
            return false;
        }

        private bool Check(TokenType type) => !IsAtEnd() && Peek().Type == type;
        private Token Peek() => _tokens[_current];
        private Token Previous() => _tokens[_current - 1];
        private bool IsAtEnd() => Peek().Type == TokenType.EOF;
        
        private Token Advance()
        {
            if (!IsAtEnd()) _current++;
            return Previous();
        }
    }
}
