namespace DaramRenamer.Utilities;

public static class StringExtension
{
    public static string ToUpperFirstLetter(this string filename)
    {
        Span<char> buffer = stackalloc char[filename.Length];
        filename.CopyTo(buffer);
        
        var isFirstLetter = true;
        for (var i = 0; i < buffer.Length; ++i)
        {
            var ch = buffer[i];
            if (char.IsUpper(ch) || char.IsLower(ch))
            {
                if (isFirstLetter)
                    buffer[i] = char.ToUpper(ch);
                isFirstLetter = false;
            }
            else
                isFirstLetter = true;
        }

        return new string(buffer);
    }
}