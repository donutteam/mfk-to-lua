using MFKToLua.MFKLexer;
using System.Globalization;
using System.Text;

if (args.Length == 0 || args.ContainsAny(["--help", "-?"]))
{
    Console.WriteLine("Usage: MFKToLua [options] <MFKPath>");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  -?, --help                        Show this help message and exit");
    Console.WriteLine("  -o, --output <path>               Set the output path");
    Console.WriteLine("  -f, --force                       Force overwrite the output path if it exists");
    return;
}

string mfkPath = args[^1];
string? outputPath = null;
bool force = false;

var optionCount = args.Length - 1;
for (var i = 0; i < optionCount; i++)
{
    switch (args[i].ToLower())
    {
        case "-o":
        case "--output":
            if (i + 1 < optionCount)
                outputPath = args[++i];
            else
                Console.WriteLine("Output argument specified with no path given.");
            break;
        case "-f":
        case "--force":
            force = true;
            break;
    }
}

var (valid, fullPath) = GetFullPath(mfkPath);
if (!valid)
{
    Console.WriteLine($"Invalid input MFK path specified: {mfkPath}");
    return;
}
mfkPath = fullPath!;

if (!File.Exists(mfkPath))
{
    Console.WriteLine($"Could not find input MFK: {mfkPath}");
    return;
}

if (outputPath != null)
{
    (valid, fullPath) = GetFullPath(outputPath);
    if (!valid)
    {
        Console.WriteLine($"Invalid output path specified: {outputPath}");
        return;
    }
    outputPath = fullPath!;

    if (!force && File.Exists(outputPath) && !AskYesNo($"Output \"{outputPath}\" already exists. Overwrite?"))
        return;
}

Console.WriteLine("Options:");
Console.WriteLine($"  Input MFK: {mfkPath}");
Console.WriteLine($"  Output: {outputPath ?? "Console"}");
Console.WriteLine($"  Force: {force}");
Console.WriteLine();

Lexer lexer;
try
{
    using var sr = new StreamReader(mfkPath);
    lexer = new(sr);
    lexer.Parse();
}
catch (Exception ex)
{
    Console.WriteLine($"There was an error parsing the MFK : {ex}");
    return;
}

try
{
    var sb = new StringBuilder();
    foreach (var node in lexer.Nodes)
        WriteNode(node, sb);

    if (outputPath == null)
    {
        Console.WriteLine(sb.ToString());
        return;
    }

    File.WriteAllText(outputPath, sb.ToString());
    Console.WriteLine($"Lua file written to: {outputPath}");
}
catch (Exception ex)
{
    Console.WriteLine($"There was an error converting to Lua: {ex}");
    return;
}

static void WriteNode(IMFKNode node, StringBuilder sb)
{
    switch (node)
    {
        case MFKComment comment:
            sb.Append(comment.Multiline ? $"--[[{comment.Text}]]" : $"--{comment.Text}");
            break;
        case MFKWhitespace whitespace:
            sb.Append(whitespace.Value);
            break;
        case MFKFunction function:
            if (function.Not)
                sb.Append("Not_");

            sb.Append($"Game.{function.Name}(");

            var first = true;
            foreach (var arg in function.Arguments)
            {
                if (first)
                    first = false;
                else
                    sb.Append(", ");

                switch (arg)
                {
                    case char c:
                        sb.Append($"\"c\"");
                        break;
                    case string s:
                        sb.Append($"\"{EscapeLuaString(s)}\"");
                        break;
                    case sbyte b:
                        sb.Append(b.ToString(CultureInfo.InvariantCulture));
                        break;
                    case byte b:
                        sb.Append(b.ToString(CultureInfo.InvariantCulture));
                        break;
                    case short s:
                        sb.Append(s.ToString(CultureInfo.InvariantCulture));
                        break;
                    case ushort us:
                        sb.Append(us.ToString(CultureInfo.InvariantCulture));
                        break;
                    case int i:
                        sb.Append(i.ToString(CultureInfo.InvariantCulture));
                        break;
                    case uint ui:
                        sb.Append(ui.ToString(CultureInfo.InvariantCulture));
                        break;
                    case long l:
                        sb.Append(l.ToString(CultureInfo.InvariantCulture));
                        break;
                    case ulong ul:
                        sb.Append(ul.ToString(CultureInfo.InvariantCulture));
                        break;
                    case float f:
                        sb.Append(f.ToString(CultureInfo.InvariantCulture));
                        break;
                    case double d:
                        sb.Append(d.ToString(CultureInfo.InvariantCulture));
                        break;
                    case bool b:
                        sb.Append(b ? "1" : "0");
                        break;
                    default:
                        throw new NotSupportedException($"Unsupported argument type: {arg.GetType()}");
                }
            }

            sb.Append(')');

            if (!function.Conditional)
                return;

            sb.AppendLine();
            sb.AppendLine("{");

            foreach (var child in function.Children)
                WriteNode(child, sb);

            sb.AppendLine("}");
            break;
        default:
            throw new Exception($"Unsupported node type: {node.GetType()}");
    }
}

static string EscapeLuaString(string s)
{
    var sb = new StringBuilder();
    foreach (var c in s)
    {
        switch (c)
        {
            case '\\': sb.Append("\\\\"); break;
            case '\"': sb.Append("\\\""); break;
            case '\n': sb.Append("\\n"); break;
            case '\r': sb.Append("\\r"); break;
            case '\t': sb.Append("\\t"); break;
            case '\0': sb.Append("\\0"); break;
            default:
                if (char.IsControl(c) || c > 127)
                {
                    var bytes = Encoding.UTF8.GetBytes([c]);
                    foreach (var b in bytes)
                        sb.AppendFormat("\\x{0:X2}", b);
                }
                else
                    sb.Append(c);
                break;
        }
    }
    return sb.ToString();
}

static (bool Valid, string? FullPath) GetFullPath(string path)
{
    if (string.IsNullOrWhiteSpace(path))
        return (false, null);

    if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
        return (false, null);

    try
    {
        var fullPath = Path.GetFullPath(path);
        return (true, fullPath);
    }
    catch
    {
        return (false, null);
    }
}

static bool AskYesNo(string question, bool defaultYes = true)
{
    var defaultOption = defaultYes ? "Y/n" : "y/N";
    while (true)
    {
        Console.Write($"{question} [{defaultOption}]: ");
        var input = Console.ReadLine()?.Trim().ToLower();

        if (string.IsNullOrEmpty(input))
            return defaultYes;

        if (input == "y" || input == "yes")
            return true;
        if (input == "n" || input == "no")
            return false;

        Console.WriteLine("Please enter 'y' or 'n'.");
    }
}