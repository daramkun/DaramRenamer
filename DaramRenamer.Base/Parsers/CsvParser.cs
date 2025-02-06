using System.Text;

namespace DaramRenamer.Parsers;

public class CsvParser : IDisposable
{
    private readonly TextReader _reader;

    public string[] Header { get; }

    public CsvParser(Stream stream, bool leaveOpen = false)
    {
        _reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: leaveOpen);
        Header = Read() ?? throw new IOException("No header found.");
    }
    
    public void Dispose()
    {
        _reader.Dispose();
        GC.SuppressFinalize(this);
    }

    public string[]? Read()
    {
        var currentLine = _reader.ReadLine();
        if (currentLine == null)
            return null;

        return ParseLine(currentLine);
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
                
                case ParseState.Ending when ch == ',':
                    buffer.Add(builder.ToString());
                    builder.Clear();
                    state = ParseState.Start;
                    isQuoteStarted = false;
                    break;
                
                case ParseState.Ending when ch == '"':
                    builder.Append('"');
                    state = ParseState.Parsing;
                    break;
                
                case ParseState.Ending:
                    return null;
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