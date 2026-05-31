using System.Text.RegularExpressions;
using Cysharp.Text;

namespace DaramRenamer.Helpers;

public static class ConvensionHelper
{
    private static readonly char[] Separators = ['\n', '\t', '_', '.', '-', '#', '?', '!'];

    extension(string? str)
    {
        public string? ToSnakeCase() =>
            !string.IsNullOrEmpty(str)
                ? Rejoin(str, separator: "_", capital: Capital.Down)
                : str;

        public string? ToUpperSnakeCase() =>
            !string.IsNullOrEmpty(str)
                ? Rejoin(str, separator: "_", capital: Capital.Up)
                : str;

        public string? ToCamelCase() =>
            !string.IsNullOrEmpty(str)
                ? Rejoin(str, separator: string.Empty, Capital.Title).FirstLetterToLower().ToUpperOnlyAfterNumber()
                : str;

        public string? ToPascalCase() =>
            !string.IsNullOrEmpty(str)
                ? Rejoin(str, separator: string.Empty, Capital.Title).ToUpperOnlyAfterNumber()
                : str;

        public string? ToKebabCase() =>
            !string.IsNullOrEmpty(str)
                ? Rejoin(str, separator: "-", capital: Capital.Down)
                : str;

        public string? ToUpperKebabCase() =>
            !string.IsNullOrEmpty(str)
                ? Rejoin(str, separator: "-", capital: Capital.Up)
                : str;

        public string? ToUpperFirstLetter()
        {
            if (string.IsNullOrEmpty(str))
                return str;
            
            var fn = str.Split(' ');
            for (var i = 0; i < fn.Length; ++i)
            {
                var chars = fn[i].ToArray();
                chars[0] = char.ToUpper(chars[0]);
                fn[i] = new string(chars);
            }

            return string.Join(' ', fn);
        }

        private string? ToUpperOnlyAfterNumber()
        {
            if (string.IsNullOrEmpty(str))
                return str;

            var arr = str.ToArray();
            for (var i = 0; i < arr.Length; ++i)
                if (char.IsLower(arr[i]) && i > 0 && char.IsDigit(arr[i - 1]))
                    arr[i] = char.ToUpper(arr[i]);

            return new string(arr);
        }
        
        private string? ToUpper() => str?.ToUpper();
        private string? ToLower() => str?.ToLower();
        private string? ToTitle() => str?.Length > 1
            ? char.ToUpper(str[0]) + str[1..]
            : str?.Length > 0
                ? str.ToUpper()
                : str;

        private string? FirstLetterToLower()
        {
            if (string.IsNullOrEmpty(str))
                return str;

            if (str.Length == 1)
                return str.ToLower();

            return char.ToLower(str[0]) + str[1..];
        }
    }

    private static string Rejoin(string input, string separator = "_", Capital capital = Capital.Down)
    {
        Func<string?, string?> mapper = capital switch
        {
            Capital.Up => ToUpper,
            Capital.Down => ToLower,
            Capital.Title => ToTitle,
            _ => throw new ArgumentOutOfRangeException(nameof(capital), capital, null)
        };

        return string.Join(separator, DoSplit(input).Select(s => mapper(s)));
    }

    private static IEnumerable<string> DoSplit(string input)
    {
        using var builder = ZString.CreateStringBuilder();

        foreach (var ch in input)
        {
            var isSeparator = Separators.Contains(ch);

            if (ch is >= 'A' and <= 'Z')
            {
                if (builder.Length == 0 || builder.AsSpan()[^1] is >= 'A' and <= 'Z')
                    builder.Append(ch);
                else
                {
                    yield return builder.ToString();
                    builder.Clear();
                    builder.Append(ch);
                }
            }
            else switch (isSeparator)
            {
                case false when ch is >= '\x20' and <= '@' or >= '[' and <= '\x7F':
                    builder.Append(ch);
                    break;

                case true when builder.Length > 0:
                    yield return builder.ToString();
                    builder.Clear();
                    break;

                default:
                    builder.Append(ch);
                    break;
            }
        }

        if (builder.Length > 0)
            yield return builder.ToString();
    }

    private enum Capital
    {
        Up,
        Down,
        Title,
    }
}