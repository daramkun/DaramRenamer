using System.Text.RegularExpressions;
using Aprillz.MewUI;
using Aprillz.MewUI.Text;

namespace DaramRenamer.MewUI;

internal sealed class RegexSyntaxHighlighter : ITextClassifier
{
    private static readonly Color EscapeColor = new(14, 145, 160);
    private static readonly Color GroupColor = new(59, 130, 246);
    private static readonly Color CharacterClassColor = new(217, 119, 6);
    private static readonly Color QuantifierColor = new(192, 38, 211);
    private static readonly Color AnchorColor = new(220, 38, 38);
    private static readonly Color AlternationColor = new(22, 163, 74);
    private static readonly Color ErrorColor = new(220, 38, 38);

    public void Classify(in TextClassificationContext context, IList<TextPaintSpan> spans)
    {
        var text = context.Text.Span;
        if (text.IsEmpty)
            return;

        Tokenize(text, spans);

        if (!IsValid(text))
            spans.Add(new TextPaintSpan(new TextRange(0, text.Length), null, null, TextDecoration.Underline));
    }

    private static bool IsValid(ReadOnlySpan<char> pattern)
    {
        try
        {
            _ = new Regex(pattern.ToString());
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static void Tokenize(ReadOnlySpan<char> text, IList<TextPaintSpan> spans)
    {
        var inClass = false;

        for (var i = 0; i < text.Length; ++i)
        {
            var c = text[i];

            if (c == '\\')
            {
                var length = Math.Min(2, text.Length - i);
                // \p{...}, \k<...>, \x41 and friends keep their argument in the same color.
                if (length == 2 && i + 2 < text.Length)
                {
                    var closing = text[i + 1] switch { 'p' or 'P' => '}', 'k' => '>', _ => '\0' };
                    if (closing != '\0' && text[i + 2] is '{' or '<')
                    {
                        var end = text[(i + 2)..].IndexOf(closing);
                        if (end >= 0)
                            length = 2 + end + 1;
                    }
                }

                Add(spans, i, length, EscapeColor);
                i += length - 1;
                continue;
            }

            if (inClass)
            {
                Add(spans, i, 1, CharacterClassColor);
                if (c == ']')
                    inClass = false;
                continue;
            }

            switch (c)
            {
                case '[':
                    inClass = true;
                    Add(spans, i, 1, CharacterClassColor);
                    // A leading ^ negates the class, and a leading ] is a literal.
                    if (i + 1 < text.Length && text[i + 1] == '^')
                        Add(spans, ++i, 1, CharacterClassColor);
                    if (i + 1 < text.Length && text[i + 1] == ']')
                        Add(spans, ++i, 1, CharacterClassColor);
                    break;
                case '(':
                    var openLength = 1;
                    if (i + 1 < text.Length && text[i + 1] == '?')
                    {
                        // Group modifiers: (?:  (?=  (?!  (?<=  (?<!  (?<name>  (?'name'
                        var j = i + 2;
                        if (j < text.Length && text[j] == '<' && j + 1 < text.Length && text[j + 1] is '=' or '!')
                            j += 2;
                        else if (j < text.Length && text[j] is '<' or '\'')
                        {
                            var close = text[j] == '<' ? '>' : '\'';
                            var end = text[(j + 1)..].IndexOf(close);
                            j = end >= 0 ? j + end + 2 : text.Length;
                        }
                        else if (j < text.Length)
                            ++j;

                        openLength = j - i;
                    }

                    Add(spans, i, openLength, GroupColor);
                    i += openLength - 1;
                    break;
                case ')':
                    Add(spans, i, 1, GroupColor);
                    break;
                case '*' or '+' or '?':
                    var quantifierLength = i + 1 < text.Length && text[i + 1] == '?' ? 2 : 1;
                    Add(spans, i, quantifierLength, QuantifierColor);
                    i += quantifierLength - 1;
                    break;
                case '{':
                    var braceEnd = text[i..].IndexOf('}');
                    if (braceEnd > 0 && IsRepeatCount(text.Slice(i + 1, braceEnd - 1)))
                    {
                        var length = braceEnd + 1;
                        if (i + length < text.Length && text[i + length] == '?')
                            ++length;

                        Add(spans, i, length, QuantifierColor);
                        i += length - 1;
                    }

                    break;
                case '^' or '$':
                    Add(spans, i, 1, AnchorColor);
                    break;
                case '|':
                    Add(spans, i, 1, AlternationColor);
                    break;
                case '.':
                    Add(spans, i, 1, EscapeColor);
                    break;
            }
        }
    }

    private static bool IsRepeatCount(ReadOnlySpan<char> body)
    {
        if (body.IsEmpty)
            return false;

        var comma = body.IndexOf(',');
        var first = comma < 0 ? body : body[..comma];
        var second = comma < 0 ? default : body[(comma + 1)..];

        return IsDigits(first) && (comma < 0 || second.IsEmpty || IsDigits(second));
    }

    private static bool IsDigits(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty)
            return false;

        foreach (var c in value)
            if (!char.IsAsciiDigit(c))
                return false;

        return true;
    }

    private static void Add(IList<TextPaintSpan> spans, int start, int length, Color color) =>
        spans.Add(new TextPaintSpan(new TextRange(start, length), color, null, TextDecoration.None));
}
