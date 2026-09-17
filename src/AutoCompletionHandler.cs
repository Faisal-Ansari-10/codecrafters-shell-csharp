using ReadLine;

class AutoCompletionHandler : IAutoCompleteHandler
{
    public char[] Separators { get; set; } = [' ',];

    private int _tabCount = 0;
    private string _lastText = "";

    public string[] GetSuggestions(string text, int index)
    {
        if (index == 0) return GetCommandSuggestions(text, index);
        return GetFileNameSuggestions(text, index);
    }

    private string[] GetCommandSuggestions(string text, int index)
    {
        if (index != 0 || string.IsNullOrEmpty(text))
        {
            _lastText = text;
            _tabCount = 0;
            return [];
        }

        if (_lastText == text)
            _tabCount++;
        else
            _tabCount = 1;

        _lastText = text;

        var builtInMatches = CommandHandler.BuiltIns
            .Where(b => b.StartsWith(text, StringComparison.OrdinalIgnoreCase));

        var executableMatches = Utils.FindExecutables(text);

        var allMatches = builtInMatches
            .Concat(executableMatches)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(m => m, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (allMatches.Length == 0)
        {
            Console.Write('\a');
            return [];
        }

        if (allMatches.Length == 1)
        {
            _tabCount = 0;
            return [allMatches[0] + " "];
        }

        string lcp = FindLCP(allMatches);
        if (lcp.Length > text.Length)
        {
            _tabCount = 0;
            _lastText = lcp;
            return [lcp];
        }

        if (_tabCount == 1)
        {
            Console.Write('\a');
            return [];
        }

        Console.WriteLine();
        Console.WriteLine(string.Join("  ", allMatches));
        Console.Write($"$ {text}");

        _tabCount = 0;
        return [];
    }

    private static string[] GetFileNameSuggestions(string text, int index)
    {
        if (index == 0 || string.IsNullOrEmpty(text)) return [];

        var pathPrefix = text[index..];
        if (string.IsNullOrEmpty(pathPrefix)) return [];

        var directory = Path.GetDirectoryName(pathPrefix);
        var searchDir = string.IsNullOrEmpty(directory)
            ? Directory.GetCurrentDirectory()
            : Path.GetFullPath(directory);

        var files = Directory.GetFiles(searchDir);
        if (files is null || files.Length == 0) return [];

        var fileNamePrefix = Path.GetFileName(pathPrefix);
        var matches = files
        .Select(Path.GetFileName)
        .Where(name => name!.StartsWith(fileNamePrefix, StringComparison.OrdinalIgnoreCase))
        .Select(name => string.IsNullOrEmpty(directory)
            ? $"{name} "
            : $"{Path.Combine(directory, name!)} ")
        .ToArray();

        return matches;
    }


    private static string FindLCP(string[] strings)
    {
        if (strings.Length == 0) return "";

        string prefix = strings[0];
        foreach (var s in strings.Skip(1))
        {
            while (!s.StartsWith(prefix))
            {
                prefix = prefix[..^1];
                if (prefix.Length == 0) return "";
            }
        }

        return prefix;
    }
}