using System;
using System.Collections.Generic;

class MiniInterpreter
{
    static void Main()
    {
        Console.WriteLine("=== Mini Math Interpreter ===");
        Console.WriteLine("Supported: +, -, *, /, parentheses");
        Console.WriteLine("Type 'exit' to quit.\n");

        while (true)
        {
            Console.Write("> ");
            string input = Console.ReadLine().Trim();

            if (input.ToLower() == "exit") break;
            if (string.IsNullOrWhiteSpace(input)) continue;

            try
            {
                var lexer = new Lexer(input);
                var parser = new Parser(lexer);
                double result = parser.Parse();
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}

// TokenType enum
public enum TokenType
{
    Number,
    Plus,
    Minus,
    Multiply,
    Divide,
    LParen,
    RParen,
    EndOfInput
}

// Token class
public class Token
{
    public TokenType Type { get; }
    public string Value { get; }

    public Token(TokenType type, string value)
    {
        Type = type;
        Value = value;
    }

    public override string ToString() => $"{Type}:{Value}";
}

// Lexer class
public class Lexer
{
    private string _input;
    private int _pos;

    public Lexer(string input)
    {
        _input = input;
        _pos = 0;
    }

    public Token GetNextToken()
    {
        SkipWhitespace();

        if (_pos >= _input.Length)
            return new Token(TokenType.EndOfInput, "");

        char currentChar = _input[_pos];

        if (char.IsDigit(currentChar))
        {
            return new Token(TokenType.Number, ReadNumber());
        }

        switch (currentChar)
        {
            case '+': _pos++; return new Token(TokenType.Plus, "+");
            case '-': _pos++; return new Token(TokenType.Minus, "-");
            case '*': _pos++; return new Token(TokenType.Multiply, "*");
            case '/': _pos++; return new Token(TokenType.Divide, "/");
            case '(': _pos++; return new Token(TokenType.LParen, "(");
            case ')': _pos++; return new Token(TokenType.RParen, ")");
            default:
                throw new Exception($"Unexpected character: '{currentChar}'");
        }
    }

    private string ReadNumber()
    {
        int start = _pos;
        while (_pos < _input.Length && char.IsDigit(_input[_pos]))
        {
            _pos++;
        }
        return _input.Substring(start, _pos - start);
    }

    private void SkipWhitespace()
    {
        while (_pos < _input.Length && char.IsWhiteSpace(_input[_pos]))
        {
            _pos++;
        }
    }
}

// Parser class with Recursive Descent
public class Parser
{
    private Lexer _lexer;
    private Token _currentToken;

    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        _currentToken = _lexer.GetNextToken();
    }

    public double Parse()
    {
        double result = ParseExpression();
        if (_currentToken.Type != TokenType.EndOfInput)
        {
            throw new Exception("Unexpected token at end of input.");
        }
        return result;
    }

    private double ParseExpression()
    {
        double result = ParseTerm();

        while (_currentToken.Type == TokenType.Plus || _currentToken.Type == TokenType.Minus)
        {
            Token token = _currentToken;
            Advance();

            double right = ParseTerm();
            if (token.Type == TokenType.Plus)
                result += right;
            else
                result -= right;
        }

        return result;
    }

    private double ParseTerm()
    {
        double result = ParseFactor();

        while (_currentToken.Type == TokenType.Multiply || _currentToken.Type == TokenType.Divide)
        {
            Token token = _currentToken;
            Advance();

            double right = ParseFactor();
            if (token.Type == TokenType.Multiply)
                result *= right;
            else
            {
                if (right == 0)
                    throw new DivideByZeroException();
                result /= right;
            }
        }

        return result;
    }

    private double ParseFactor()
    {
        if (_currentToken.Type == TokenType.Number)
        {
            double value = double.Parse(_currentToken.Value);
            Advance();
            return value;
        }
        else if (_currentToken.Type == TokenType.LParen)
        {
            Advance();
            double result = ParseExpression();
            if (_currentToken.Type != TokenType.RParen)
                throw new Exception("Expected ')'");
            Advance();
            return result;
        }
        else
        {
            throw new Exception("Expected number or '('");
        }
    }

    private void Advance()
    {
        _currentToken = _lexer.GetNextToken();
    }
}
