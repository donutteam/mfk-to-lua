namespace MFKToLua.MFKLexer;

public class MFKWhitespace(string value) : IMFKNode
{
    public string Value { get; } = value;

    public override string ToString() => Value;
}