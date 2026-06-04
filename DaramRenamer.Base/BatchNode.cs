using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using DaramRenamer.Registry;

namespace DaramRenamer;

[Serializable]
public class BatchNode
{
    private static readonly Regex BatchNodeTitle = new("\\[((BatchNode)|(RootBatchNode))\\]");
    private static readonly Regex CommandInfo = new("-Command:(.*)");
    private static readonly Regex ConditionInfo = new("-Condition:(.*)");

    public ICommand? Command { get; set; }
    public ICondition? Condition { get; set; }

    public ObservableCollection<BatchNode> Children { get; } = [];

    public override string ToString()
    {
        var kind = Condition == null ? "BatchWindow_Command" : "BatchWindow_Condition";
        var target = Command as object ?? Condition;
        var descriptor = target == null ? null : DaramRenamerRegistry.GetDescriptor(target);

        if (target == null || descriptor == null)
            return "Null";

        var args = new StringBuilder("(");
        foreach (var option in descriptor.Options)
            args.AppendFormat("{0} = {1}, ", Strings.Instance[option.LocalizationKey], option.GetValue(target));

        if (args.Length > 1)
        {
            args.Remove(args.Length - 2, 2);
            args.Append(')');
        }
        else
        {
            args.Clear();
        }

        return $"{Strings.Instance[kind]}: {Strings.Instance[descriptor.LocalizationKey]}{args}";
    }

    public void Execute(FileItem fileInfo, int index = 0)
    {
        Command?.Apply(fileInfo, index);
        if (Condition != null && !Condition.IsSatisfyThisCondition(fileInfo))
            return;

        foreach (var node in Children)
            node.Execute(fileInfo, index);
    }

    public virtual void Serialize(TextWriter writer)
    {
        writer.WriteLine($"[{GetType().Name}]");
        writer.WriteLine($"-Command:{SerializeObject(Command)}");
        writer.WriteLine($"-Condition:{SerializeObject(Condition)}");
        writer.WriteLine("-StartChildren");
        foreach (var child in Children)
            child.Serialize(writer);
        writer.WriteLine("-EndChildren");
    }

    public virtual void Deserializer(TextReader reader, string? line = null)
    {
        var name = line ?? reader.ReadLine();
        var nameMatch = BatchNodeTitle.Match(name ?? string.Empty);
        if (!nameMatch.Success)
            throw new Exception("File is not valid.");

        if (nameMatch.Groups[1].Value != GetType().Name)
            throw new Exception("BatchNode error.");

        var command = reader.ReadLine();
        var commandMatch = CommandInfo.Match(command ?? string.Empty);
        if (!commandMatch.Success)
            throw new Exception("File is not valid.");

        Command = DeserializeObject(commandMatch.Groups[1].Value) as ICommand;

        var condition = reader.ReadLine();
        var conditionMatch = ConditionInfo.Match(condition ?? string.Empty);
        if (!conditionMatch.Success)
            throw new Exception("File is not valid.");

        Condition = DeserializeObject(conditionMatch.Groups[1].Value) as ICondition;

        if (reader.ReadLine() != "-StartChildren")
            throw new Exception("File is not valid.");

        Children.Clear();

        while (true)
        {
            var nextLine = reader.ReadLine();
            if (nextLine == "-EndChildren")
                break;

            var child = new BatchNode();
            child.Deserializer(reader, nextLine);

            Children.Add(child);
        }
    }

    private static string SerializeObject(object? obj)
    {
        if (obj == null)
            return "null";

        var descriptor = DaramRenamerRegistry.GetDescriptor(obj) ??
                         throw new InvalidOperationException($"Unregistered batch item: {obj.GetType().FullName}");
        var builder = new StringBuilder();
        builder.AppendFormat("\"{0}\"", descriptor.Id);
        builder.Append('{');

        foreach (var option in descriptor.Options)
        {
            builder.AppendFormat("\"{0}\":", option.Name);
            var value = option.SerializeValue(obj)
                .Replace("\"", "\\\"")
                .Replace("\n", "\\\n")
                .Replace("\r", "");
            builder.AppendFormat("\"{0}\"", value);
            builder.Append(',');
        }

        if (descriptor.Options.Count > 0)
            builder.Remove(builder.Length - 1, 1);

        builder.Append('}');

        return builder.ToString();
    }

    private static object? DeserializeObject(string str)
    {
        var builder = new StringBuilder();

        if (str == "null")
            return null;

        TextReader reader = new StringReader(str);
        if (reader.Read() != '"')
            throw new Exception("Invalid syntax.");

        do
        {
            var ch = reader.Read();
            if (ch == -1)
                throw new Exception("Invalid syntax");
            if (ch == '"')
                break;

            builder.Append((char)ch);
        } while (true);

        var name = builder.ToString();

        ItemDescriptor descriptor = DaramRenamerRegistry.FindCommandDescriptor(name)
                                    ?? (ItemDescriptor)(DaramRenamerRegistry.FindConditionDescriptor(name)
                                                        ?? throw new Exception("Invalid batch item"));
        var obj = descriptor.Create();

        if (reader.Read() != '{')
            throw new Exception("Invalid syntax");

        builder.Clear();
        var isStart = true;
        var isKey = true;
        string? key = null;
        while (true)
        {
            var ch = reader.Read();
            if (ch == -1)
                throw new Exception("Invalid syntax");
            if (ch == '}')
                break;

            if (isKey && isStart)
            {
                if (ch != '"')
                    throw new Exception("Invalid syntax");
                isStart = false;
            }
            else if (isKey)
            {
                if (ch == '"')
                {
                    isKey = false;
                    isStart = true;
                    key = builder.ToString();
                    builder.Clear();

                    ch = reader.Read();
                    if (ch is -1 or not ':')
                        throw new Exception("Invalid syntax");
                }
                else
                {
                    builder.Append((char)ch);
                }
            }
            else if (isStart)
            {
                if (ch != '"')
                    throw new Exception("Invalid syntax");
                isStart = false;
            }
            else
            {
                if (ch == '"')
                {
                    isKey = true;
                    isStart = true;

                    var value = builder.ToString();

                    var option = descriptor.Options.FirstOrDefault(m => m.Name == key);
                    if (option == null)
                        throw new Exception("Invalid field");

                    option.DeserializeValue(obj, value);

                    builder.Clear();

                    ch = reader.Read();
                    if (ch == -1)
                        throw new Exception("Invalid syntax");
                    if (ch == ',')
                        continue;
                    if (ch == '}')
                        break;
                    throw new Exception("Invalid syntax");
                }

                builder.Append((char)ch);
            }
        }

        return obj;
    }
}

[Serializable]
public class RootBatchNode : BatchNode
{
    public override string ToString() => Strings.Instance["BatchWindow_Title"];
}
