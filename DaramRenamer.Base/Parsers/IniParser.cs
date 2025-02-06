using System.Text;

namespace DaramRenamer.Parsers;

[Serializable]
public class IniObject(string? section, IReadOnlyDictionary<string, string> data)
{
    public string? Section { get; } = section;
    public IReadOnlyDictionary<string, string> Data { get; } = data;
}

public class IniParser(Stream stream, bool leaveOpen = false) : IDisposable
{
    private readonly TextReader _reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: leaveOpen);

    public void Dispose()
    {
        _reader.Dispose();
        GC.SuppressFinalize(this);
    }

    public IEnumerable<IniObject> Read()
    {
        string? section = null;
        var data = new Dictionary<string, string>();
        
        while (true)
        {
            var i = 0;
            var line = _reader.ReadLine();
            if (line == null)
                break;
            
            if (line.Length == 0)
                continue;

            for (; i < line.Length; ++i)
            {
                var ch = line [i];
                if (ch != ' ' && ch != '\t' && ch != '\a' && ch != '\r')
                    break;
            }

            switch (line [i])
            {
                case ';':
                    continue;

                case '[':
                {
                    if (section != null || data.Count > 0)
                        yield return new IniObject(section, data);

                    section = line[(i + 1)..line.IndexOf(']', i + 1)];
                    data = new Dictionary<string, string>();
                    break;
                }
                
                default:
                {
                    var key = __IniGetKey (line, ref i);
                    var value = __IniGetValue (line, i);
                    data[key] = value;
                    break;
                }
            }
        }
        
        if (section != null || data.Count > 0)
            yield return new IniObject(section, data);
    }
    
    private static string __IniGetKey (string line, ref int startIndex)
    {
        var sb = new StringBuilder ();
        for (; startIndex < line.Length && line [startIndex] != '='; ++startIndex)
            sb.Append (line [startIndex]);
        
        ++startIndex;
        return sb.ToString ().Trim ();
    }

    private static string __IniGetValue (string line, int startIndex)
    {
        if (line.Length == startIndex) return "";

        var sb = new StringBuilder ();
        for (; startIndex < line.Length; ++startIndex)
        {
            var ch = line [startIndex];
            if (ch != ' ' && ch != '	' && ch != '\a' && ch != '\r')
                break;
        }
        
        if (line [startIndex] == '"')
        {
            ++startIndex;
            for (; startIndex < line.Length && line [startIndex] != '"'; ++startIndex)
                sb.Append (line [startIndex]);
        }
        else
        {
            for (; startIndex < line.Length && line [startIndex] != '\n' && line [startIndex] != ';'; ++startIndex)
                sb.Append (line [startIndex]);
        }
        
        return sb.ToString ().Trim ();
    }
}