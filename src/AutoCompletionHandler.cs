using ReadLine;

class AutoCompletionHandler : IAutoCompleteHandler
{
  public char[] Separators { get; set; } = [' ',];

  public string[] GetSuggestions(string text, int index)
  {
    if (index == 0)
    {
      if (text.StartsWith("echo") || text.StartsWith("ech") || text.StartsWith("ec"))
        return ["echo "];
      else if (text.StartsWith("exit") || text.StartsWith("exi") || text.StartsWith("ex"))
        return ["exit "];
      else if (text.StartsWith('e'))
        return ["echo ", "exit "];
    }

    return [];
  }
}