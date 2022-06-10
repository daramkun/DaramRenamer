using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace DaramRenamer;

[Serializable]
public class BatchNode
{
    private static readonly Regex BatchNodeTitle = new("\\[((BatchNode)|(RootBatchNode))\\]");
    private static readonly Regex CommandInfo = new("-Command:(.*)");
    private static readonly Regex ConditionInfo = new("-Condition:(.*)");

    public ICommand Command { get; set; }
    public ICondition Condition { get; set; }

    public ObservableCollection<BatchNode> Children { get; } = new();

    public override string ToString()
    {
        var kind = Condition == null ? "BatchWindow_Command" : "BatchWindow_Condition";
        var target = Command as object ?? Condition;
        var type = target?.GetType();
        var localizationKey = type?.GetCustomAttribute<LocalizationKeyAttribute>()?.LocalizationKey;

        if (target == null || type == null)
            return "Null";

        var args = new StringBuilder("(");
        foreach (var propInfo in type.GetProperties())
        {
            var propLocalizationKey = propInfo.GetCustomAttribute<LocalizationKeyAttribute>();
            if (propLocalizationKey == null)
                continue;
            args.AppendFormat("{0} = {1}, ", Strings.Instance[propLocalizationKey.LocalizationKey],
                propInfo.GetValue(target));
        }

        if (args.Length > 1)
        {
            args.Remove(args.Length - 2, 2);
            args.Append(")");
        }
        else
        {
            args.Clear();
        }

        return $"{Strings.Instance[kind]}: {Strings.Instance[localizationKey]}{args}";
    }

    public void Execute(FileInfo fileInfo)
    {
        Command?.DoCommand(fileInfo);
        if (Condition != null && !Condition.IsSatisfyThisCondition(fileInfo))
            return;

        foreach (var node in Children)
            node.Execute(fileInfo);
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

    public virtual void Deserializer(TextReader reader, string line = null)
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

    private string SerializeObject(object obj)
    {
        if (obj == null) return "null";

        var builder = new StringBuilder();
        builder.AppendFormat("\"{0}\"", obj.GetType().FullName);
        builder.Append('{');

        var members = obj.GetType().GetProperties();
        foreach (var member in members)
        {
            if (!member.CanRead || !member.CanWrite)
                continue;

            builder.AppendFormat("\"{0}\":", member.Name);
            var value = member.GetValue(obj)?.ToString()?
                .Replace("\"", "\\\"")
                .Replace("\n", "\\\n")
                .Replace("\r", "");
            if (value != null)
                builder.AppendFormat("\"{0}\"", value);
            else
                builder.Append("\"\"");

            builder.Append(',');
        }

        if (members.Length > 0)
            builder.Remove(builder.Length - 1, 1);

        builder.Append('}');

        return builder.ToString();
    }

    private object DeserializeObject(string str)
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

            builder.Append((char) ch);
        } while (true);

        var name = builder.ToString();

        var type = FindType(name);
        var obj = Activator.CreateInstance(type);

        var members = type.GetProperties();

        if (reader.Read() != '{')
            throw new Exception("Invalid syntax");

        builder.Clear();
        var isStart = true;
        var isKey = true;
        string key = null;
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
                    builder.Append((char) ch);
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

                    var member = (from m in members where m.Name == key select m).FirstOrDefault();
                    if (member == null)
                        throw new Exception("Invalid field");

                    object trueValue;
                    if (string.IsNullOrEmpty(value))
                        trueValue = null;
                    else if (member.PropertyType == typeof(string))
                        trueValue = value;
                    else if (member.PropertyType == typeof(int))
                        trueValue = int.Parse(value);
                    else if (member.PropertyType == typeof(uint))
                        trueValue = uint.Parse(value);
                    else if (member.PropertyType == typeof(short))
                        trueValue = short.Parse(value);
                    else if (member.PropertyType == typeof(ushort))
                        trueValue = ushort.Parse(value);
                    else if (member.PropertyType == typeof(long))
                        trueValue = long.Parse(value);
                    else if (member.PropertyType == typeof(ulong))
                        trueValue = ulong.Parse(value);
                    else if (member.PropertyType == typeof(byte))
                        trueValue = byte.Parse(value);
                    else if (member.PropertyType == typeof(sbyte))
                        trueValue = sbyte.Parse(value);
                    else if (member.PropertyType == typeof(float))
                        trueValue = float.Parse(value);
                    else if (member.PropertyType == typeof(double))
                        trueValue = double.Parse(value);
                    else if (member.PropertyType == typeof(decimal))
                        trueValue = decimal.Parse(value);
                    else if (member.PropertyType == typeof(bool))
                        trueValue = bool.Parse(value);
                    else if (member.PropertyType.IsEnum)
                        trueValue = Enum.Parse(member.PropertyType, value, false);
                    else
                        trueValue = Activator.CreateInstance(member.PropertyType, value);

                    member.SetValue(obj, trueValue);

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

                builder.Append((char) ch);
            }
        }

        return obj;
    }

    private Type FindType(string name)
    {
        foreach (var command in PluginManager.Instance.Commands)
            if (command.GetType().FullName == name)
                return command.GetType();
        return (from condition in PluginManager.Instance.Conditions
            where condition.GetType().FullName == name
            select condition.GetType()).FirstOrDefault();
    }
}

[Serializable]
public class RootBatchNode : BatchNode
{
    public override string ToString()
    {
        return Strings.Instance["BatchWindow_Title"];
    }
}