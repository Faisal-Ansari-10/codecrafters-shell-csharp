using CodeCrafters.Shell.src;

class AutoCompletionHandler(ICommandRegistry builtins, ICompleteRegistry completeRegistry)
{
    private readonly ICommandRegistry _builtins = builtins;
    private readonly ICompleteRegistry _completeRegistry = completeRegistry;

    private static readonly char[] PathSeparators = ['/', '\\'];
    public string[] GetSuggestions(string text, int index)
    {
        string commandName = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";

        if (index > 0 && _completeRegistry.TryGet(commandName, out var path))
        {
            TextWriter output = new StringWriter();
            TextWriter error = new StringWriter();

            ExternalCommand.Run(path, [], output, error);

            var result = output.ToString()?.Trim() ?? "";
            return string.IsNullOrEmpty(result) ? [] : [result + " "];
        }

        return index == 0 ? GetCommandSuggestions(text) : GetFileNameSuggestions(text, index);
    }
    private string[] GetCommandSuggestions(string text)
    {
        var builtInMatches = _builtins
            .Names
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