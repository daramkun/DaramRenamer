using System.Text;

namespace DaramRenamer.Helpers;

internal class CsvReader(Stream stream) : IDisposable
{
    private readonly TextReader _reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);

    public string[]? Header { get; private set; }
    public string[]? CurrentRow { get; private set; }

    ~CsvReader()
    {
        Dispose();
    }
    
    public void Dispose()
    {
        _reader.Dispose();
        GC.SuppressFinalize(this);
    }

    public bool Read()
    {
        var currentLine = _reader.ReadLine();
        if (currentLine == null)
            return false;

        var row = ParseLine(currentLine);
        if (row == null)
            return false;
        
        if (Header == null)
        {
            Header = row;
            return true;
        }

        if (Header.Length != row.Length)
            return false;

        CurrentRow = row;
        return true;
    }

    private static string[]? ParseLine(string line)
    {
        var reader = new StringReader(line);
        var state = ParseState.Start;
        var builder = new StringBuilder();
        var buffer = new List<string>();

        var isQuoteStarted = false;

        while (reader.Peek() != -1)
        {
            var ch = (char)reader.Read();

            switch (state)
            {
                case ParseState.Start:
                    switch (ch)
                    {
                        case '"':
                            isQuoteStarted = true;
                            break;
                        case ',':
                            buffer.Add(string.Empty);
                            isQuoteStarted = false;
                            continue;
                        default:
                            builder.Append(ch);
                            break;
                    }

                    state = ParseState.Parsing;
                    break;
                case ParseState.Parsing:
                    switch (ch)
                    {
                        case '"':
                            state = ParseState.Ending;
                            break;
                        case ',' when isQuoteStarted:
                            builder.Append(',');
                            continue;
                        case ',':
                            buffer.Add(builder.ToString());
                            builder.Clear();
                            state = ParseState.Start;
                            isQuoteStarted = false;
                            break;
                        default:
                            builder.Append(ch);
                            break;
                    }

                    break;
                case ParseState.Ending:
                    switch (ch)
                    {
                        case ',':
                            buffer.Add(builder.ToString());
                            builder.Clear();
                            state = ParseState.Start;
                            isQuoteStarted = false;
                            break;
                        case '"':
                            builder.Append('"');
                            state = ParseState.Parsing;
                            break;
                        default:
                            return null;
                    }

                    break;
            }
        }

        buffer.Add(builder.ToString());

        return buffer.ToArray();
    }
    
    private enum ParseState
    {
        Start,
        Parsing,
        Ending,
    }
}