class AutoCompletionHandler
{
    private static readonly char[] PathSeparators = ['/', '\\'];
    public string[] GetSuggestions(string text, int index) =>
        index == 0 ? GetCommandSuggestions(text) : GetFileNameSuggestions(text, index);

    private static string[] GetCommandSuggestions(string text)
    {
        if (text.Length == 0) return [];

        var builtInMatches = CommandHandler.BuiltIns
            .Where(b => b.StartsWith(text, StringComparison.OrdinalIgnoreCase));

        return [.. builtInMatches
            .Concat(Utils.FindExecutables(text))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(m => m, StringComparer.OrdinalIgnoreCase)
            .Select(m => m + " ")];
    }

    private static string[] GetFileNameSuggestions(string text, int index)
    {
        string pathPrefix = text[index..];

        int slash = pathPrefix.LastIndexOfAny(PathSeparators);
        string directory = slash >= 0 ? pathPrefix[..(slash + 1)] : "";
        string namePrefix = pathPrefix[(slash + 1)..];

        string searchDir = directory.Length == 0
            ? Directory.GetCurrentDirectory()
            : Path.GetFullPath(directory);

        if (!Directory.Exists(searchDir)) return [];

        try
        {
            return [.. Directory.EnumerateFileSystemEntries(searchDir)
                .Select(path => (Name: Path.GetFileName(path), IsDir: Directory.Exists(path)))
                .Where(e => e.Name.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
                .Select(e => directory + e.Name + (e.IsDir ? Path.DirectorySeparatorChar : ' '))];
        }
        catch (UnauthorizedAccessException)
        {
            return [];
        }
    }
}