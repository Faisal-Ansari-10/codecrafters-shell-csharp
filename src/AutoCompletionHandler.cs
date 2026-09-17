using ReadLine;

class AutoCompletionHandler : IAutoCompleteHandler
{
    public char[] Separators { get; set; } = [' ',];

    private int _tabCount = 0;
    private string _lastText = "";

    public string[] GetSuggestions(string text, int index)
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

        if (_tabCount == 1)
        {
            Console.Write('\a');
            allMatches.Sort((a, b) => a.Length - b.Length);
            int prefixIndex = 0;
            for(int i = 0; i < allMatches.Length; i++)
            {
                int j = i + 1;
                for(; j < allMatches.Length; j++)
                {
                    if(!allMatches[j].StartsWith(allMatches[i])) break;
                }

                if(j < allMatches.Length) break;
                prefixIndex = i;
            }
            return [allMatches[prefixIndex] + " "];
        }

        Console.WriteLine();
        Console.WriteLine(string.Join("  ", allMatches));
        Console.Write($"$ {text}");

        _tabCount = 0;
        return [];
    }
}