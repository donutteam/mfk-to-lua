namespace MFKToLua.MFKLexer;

public class MFKComment(string text, bool multiline) : IMFKNode
{
    public string Text { get; set; } = text;
    public bool Multiline { get; set; } = multiline;

    public override string ToString() => Multiline ? $"/*{Text}*/" : $"//{Text}";
}
