// Sửa phần xử lý '-'
case '-': 
    if (Match('-'))
    {
        // Line comment starting with --
        while (Peek() != '\n' && !IsAtEnd()) Advance();
        string commentText = _source.Substring(_start + 2, _current - _start - 2);
        AddToken(TokenType.COMMENT, commentText);
    }
    else
    {
        AddToken(TokenType.MINUS);
    }
    break;

// Sửa phần xử lý '$'
case '$':
    if (Peek() == '"')
    {
        Advance();
        StringInterpolation();
    }
    else
    {
        throw SyntaxError.UnexpectedSymbol(_line, _column, '$');
    }
    break;

// Sửa HandleEscapeSequence
private char HandleEscapeSequence()
{
    char c = Peek();
    Advance(); // Consume the escape sequence char
    
    return c switch
    {
        'n' => '\n',
        't' => '\t',
        'r' => '\r',
        '"' => '"',
        '\\' => '\\',
        '{' => '{',
        '}' => '}',
        _ => throw SyntaxError.InvalidEscapeSequence(_line, _column, c)
    };
}

// Thêm class helper cho interpolated string
public class InterpolatedExpression
{
    public string Expression { get; }
    public InterpolatedExpression(string expr) => Expression = expr;
}

// Cập nhật StringInterpolation
private void StringInterpolation()
{
    var parts = new List<object>();
    var currentString = new StringBuilder();
    
    while (Peek() != '"' && !IsAtEnd())
    {
        if (Peek() == '\n') 
        {
            _line++;
            _column = 1;
        }
        
        if (Peek() == '{')
        {
            if (currentString.Length > 0)
            {
                parts.Add(currentString.ToString());
                currentString.Clear();
            }
            
            Advance(); // consume '{'
            
            int startPos = _current;
            int braceDepth = 1;
            
            while (braceDepth > 0 && !IsAtEnd())
            {
                if (Peek() == '{') braceDepth++;
                if (Peek() == '}') braceDepth--;
                if (Peek() == '\n') _line++;
                Advance();
            }
            
            string expr = _source.Substring(startPos, _current - startPos - 1);
            parts.Add(new InterpolatedExpression(expr));
        }
        else if (Peek() == '\\')
        {
            Advance();
            currentString.Append(HandleEscapeSequence());
            // Don't Advance() again because HandleEscapeSequence already did
        }
        else
        {
            currentString.Append(Peek());
            Advance();
        }
    }
    
    if (IsAtEnd())
        throw SyntaxError.UnterminatedString(_line, _column);
    
    Advance(); // consume closing "
    
    if (currentString.Length > 0)
        parts.Add(currentString.ToString());
    
    AddToken(TokenType.STRING_INTERPOLATED, parts);
}

// Sửa Number() cho trường hợp đặc biệt
private void Number()
{
    while (char.IsDigit(Peek())) Advance();
    
    if (Peek() == '.')
    {
        if (char.IsDigit(PeekNext()))
        {
            Advance(); // consume '.'
            while (char.IsDigit(Peek())) Advance();
            
            string numStr = _source.Substring(_start, _current - _start);
            if (!double.TryParse(numStr, out double value))
                throw SyntaxError.InvalidNumber(_line, _column, numStr);
            
            AddToken(TokenType.NUMBER, value);
        }
        else
        {
            // Number followed by dot (e.g., "123.")
            string numStr = _source.Substring(_start, _current - _start);
            if (!int.TryParse(numStr, out int value))
                throw SyntaxError.InvalidNumber(_line, _column, numStr);
            
            AddToken(TokenType.NUMBER, value);
            // Dot will be handled in next ScanToken()
        }
    }
    else
    {
        string numStr = _source.Substring(_start, _current - _start);
        if (!int.TryParse(numStr, out int value))
            throw SyntaxError.InvalidNumber(_line, _column, numStr);
        
        AddToken(TokenType.NUMBER, value);
    }
}
