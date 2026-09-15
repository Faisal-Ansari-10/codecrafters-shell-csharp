using ReadLine;

class AutoCompletionHandler : IAutoCompleteHandler
{
  public char[] Separators { get; set; } = [' ',];

  public string[] GetSuggestions(string text, int index)
  {
    if (index != 0 || string.IsNullOrEmpty(text))
      return [];

    var builtInMatches = CommandHandler.BuiltIns
        .Where(b => b.StartsWith(text, StringComparison.OrdinalIgnoreCase))
        .Select(b => b + " ")
        .ToArray();

    var executableMatches = Utils.FindExecutables(text)
    .Select(exe => exe + " ")
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

    if (builtInMatches.Length > 0 || executableMatches.Length > 0) return [.. builtInMatches, .. executableMatches];

    Console.Write('\a');
    return [];
  }
}