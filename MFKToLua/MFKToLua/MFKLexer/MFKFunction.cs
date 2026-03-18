using System.Globalization;
using System.Text;

namespace MFKToLua.MFKLexer;

internal class MFKFunction(string name, object? arguments = null, bool conditional = false, bool not = false) : IMFKNode
{
    public string Name { get; set; } = name;
    public List<object> Arguments { get; set; } = arguments switch
    {
        null => [new List<object>()],
        List<object> list => list,
        _ => [arguments]
    };
    public bool Conditional { get; set; } = conditional;
    public bool Not { get; set; } = not;
    public List<IMFKNode> Children { get; set; } = [];

    public void AddChild(IMFKNode node)
    {
        if (!Conditional)
            throw new Exception("Cannot add a child to a non-conditional function.");

        Children.Add(node);
    }

    public bool SetArg(int index, object value, object? condition = null)
    {
        if (index < 0 || index >= Arguments.Count)
            return false;

        if (condition == null || Equals(condition, Arguments[index]))
        {
            Arguments[index] = value;
            return true;
        }

        return false;
    }

    public override string ToString()
    {
        var args = new StringBuilder();
        var first = true;
        foreach (var arg in Arguments)
        {
            if (first)
                first = false;
            else
                args.Append(", ");

            switch (arg)
            {
                case char c:
                    args.Append($"\"c\"");
                    break;
                case string s:
                    args.Append($"\"{s.Replace("\"", "\\\"")}\"");
                    break;
                case sbyte sb:
                    args.Append(sb.ToString(CultureInfo.InvariantCulture));
                    break;
                case byte b:
                    args.Append(b.ToString(CultureInfo.InvariantCulture));
                    break;
                case short s:
                    args.Append(s.ToString(CultureInfo.InvariantCulture));
                    break;
                case ushort us:
                    args.Append(us.ToString(CultureInfo.InvariantCulture));
                    break;
                case int i:
                    args.Append(i.ToString(CultureInfo.InvariantCulture));
                    break;
                case uint ui:
                    args.Append(ui.ToString(CultureInfo.InvariantCulture));
                    break;
                case long l:
                    args.Append(l.ToString(CultureInfo.InvariantCulture));
                    break;
                case ulong ul:
                    args.Append(ul.ToString(CultureInfo.InvariantCulture));
                    break;
                case float f:
                    args.Append(f.ToString(CultureInfo.InvariantCulture));
                    break;
                case double d:
                    args.Append(d.ToString(CultureInfo.InvariantCulture));
                    break;
                case bool b:
                    args.Append(b ? "1" : "0");
                    break;
                default:
                    throw new NotSupportedException($"Unsupported argument type: {arg.GetType()}");
            }
        }

        var prefix = Not ? "!" : "";

        if (Conditional)
        {
            var children = string.Concat(Children.Select(c => c.ToString()));
            return $"{prefix}\"{Name}\"({args})\n{{\n{children}\n}}";
        }

        return $"{prefix}\"{Name}\"({args});";
    }
}
