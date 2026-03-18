using System.Text;

namespace MFKToLua.MFKLexer;

public class Lexer
{
    private readonly StreamReader _reader;
    private int _current;
    private bool _eof;

    public List<IMFKNode> Nodes { get; } = [];

    public Lexer(StreamReader reader)
    {
        _reader = reader;
        Advance();
    }

    private void Advance()
    {
        _current = _reader.Read();
        _eof = _current == -1;
    }

    private static bool IsWhiteSpace(int c) => c == 9 || c == 10 || c == 13 || c == 32;

    private string? ReadWhiteSpace()
    {
        if (_eof || !IsWhiteSpace(_current))
            return null;

        var sb = new StringBuilder();

        while (!_eof && IsWhiteSpace(_current))
        {
            sb.Append((char)_current);
            Advance();
        }

        return sb.ToString();
    }

    private MFKComment? TryReadComment()
    {
        if (_current != '/')
            return null;

        Advance();

        if (_current == '/')
        {
            Advance();

            var sb = new StringBuilder();
            while (!_eof && _current != '\n' && _current != '\r')
            {
                sb.Append((char)_current);
                Advance();
            }

            return new MFKComment(sb.ToString(), false);
        }

        if (_current == '*')
        {
            Advance();

            var sb = new StringBuilder();
            while (!_eof)
            {
                if (_current == '*')
                {
                    Advance();
                    if (_current == '/')
                    {
                        Advance();
                        break;
                    }
                    sb.Append('*');
                }

                sb.Append((char)_current);
                Advance();
            }

            return new MFKComment(sb.ToString(), true);
        }

        return null;
    }

    private string ReadQuoted()
    {
        Advance();
        var result = new StringBuilder();

        while (!_eof)
        {
            if (_current == '\\')
            {
                Advance();
                if (_current == '"')
                {
                    result.Append('"');
                    Advance();
                    continue;
                }
                result.Append('\\');
            }

            if (_current == '"')
            {
                Advance();
                break;
            }

            result.Append((char)_current);
            Advance();
        }

        return result.ToString();
    }

    private string? ReadToken(bool addWhitespace)
    {
        var whitespace = ReadWhiteSpace();
        if (!string.IsNullOrEmpty(whitespace))
        {
            if (addWhitespace)
                Nodes.Add(new MFKWhitespace(whitespace));
            return ReadToken(addWhitespace);
        }

        var comment = TryReadComment();
        if (comment != null)
        {
            Nodes.Add(comment);
            return ReadToken(addWhitespace);
        }

        if (_eof)
            return null;

        if (_current == '"')
            return ReadQuoted();

        var token = new StringBuilder();

        while (!_eof && !IsWhiteSpace(_current) && !"(){};,!".Contains((char)_current))
        {
            token.Append((char)_current);
            Advance();
        }

        if (token.Length == 0)
        {
            token.Append((char)_current);
            Advance();
        }

        return token.ToString();
    }

    private void Expect(string expected, bool addWhitespace)
    {
        var token = ReadToken(addWhitespace);

        if (token != expected)
            throw new Exception($"Expected token: {expected}, got: {token ?? "EOF"}");
    }

    public void Parse()
    {
        var stack = new Stack<MFKFunction>();

        while (true)
        {
            var token = ReadToken(true);
            if (token == null)
                break;

            if (token == "}")
            {
                stack.Pop();
                continue;
            }

            var not = token == "!";
            if (not)
                token = ReadToken(false);

            var name = token!;

            Expect("(", false);

            var args = new List<object>();

            while (true)
            {
                var t = ReadToken(false);
                if (t == ")")
                    break;

                if (args.Count > 0)
                {
                    if (t != ",")
                        throw new Exception($"Expected ',', got: {t ?? "EOF"}");
                    t = ReadToken(false);
                }

                if (t == null)
                    throw new Exception($"Expected argument, got: EOF");

                if (long.TryParse(t, out var l))
                    args.Add(l);
                else if (double.TryParse(t, out var d))
                    args.Add(d);
                else
                    args.Add(t);
            }

            var next = ReadToken(false);
            var conditional = next == "{";

            if (!conditional && next != ";")
                throw new Exception($"Expected ';', got: {next ?? "EOF"}");

            var func = new MFKFunction(name, args, conditional, not);

            if (stack.Count > 0)
                stack.Peek().AddChild(func);
            else
                Nodes.Add(func);

            if (conditional)
                stack.Push(func);
        }
    }
}
