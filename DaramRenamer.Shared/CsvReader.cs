using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Accessibility;

namespace DaramRenamer;

public class CsvReader : IDisposable
{
    private readonly TextReader _reader;
    
    private string[] _header;
    private string[] _currentRow;

    public string[] Header => _header;
    public string[] CurrentRow => _currentRow;

    public CsvReader(Stream stream)
    {
        _reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
    }
    
    public void Dispose()
    {
        _reader?.Dispose();
    }

    public bool Read()
    {
        var currentLine = _reader.ReadLine();
        if (currentLine == null)
            return false;

        var row = ParseLine(currentLine);
        if (row == null)
            return false;
        
        if (_header == null)
        {
            _header = row;
            return true;
        }

        if (_header.Length != row.Length)
            return false;

        _currentRow = row;
        return true;
    }

    private static string[] ParseLine(string line)
    {
        var reader = new StringReader(line);
        var state = ParseState.Start;
        var builder = new StringBuilder();
        var buffer = new List<string>();

        var isQuoteStarted = false;

        while (reader.Peek() != -1)
        {
            var ch = (char)reader.Read();

            if (state == ParseState.Start)
            {
                if (ch == '"')
                    isQuoteStarted = true;
                else if (ch == ',')
                {
                    buffer.Add(string.Empty);
                    isQuoteStarted = false;
                    continue;
                }
                else
                    builder.Append(ch);
                state = ParseState.Parsing;
            }
            else if (state == ParseState.Parsing)
            {
                if (ch == '"')
                    state = ParseState.Ending;
                else if (ch == ',')
                {
                    if (isQuoteStarted)
                    {
                        builder.Append(',');
                        continue;
                    }

                    buffer.Add(builder.ToString());
                    builder.Clear();
                    state = ParseState.Start;
                    isQuoteStarted = false;
                }
                else
                    builder.Append(ch);
            }
            else if (state == ParseState.Ending)
            {
                if (ch == ',')
                {
                    buffer.Add(builder.ToString());
                    builder.Clear();
                    state = ParseState.Start;
                    isQuoteStarted = false;
                }
                else if (ch == '"')
                {
                    builder.Append('"');
                    state = ParseState.Parsing;
                }
                else
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