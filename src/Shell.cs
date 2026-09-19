using System.Text;

class Shell
{
  private readonly CommandHandler _commandHandler;
  private readonly AutoCompletionHandler _autoCompletion;
  private int _tabCount;

  public Shell()
  {
    var lexer = new Lexer();
    _commandHandler = new CommandHandler(lexer);
    _autoCompletion = new AutoCompletionHandler();
  }

  public void Run()
  {
    while (true)
    {
      Console.Write("$ ");
      string? command = ReadCommand();
      if (string.IsNullOrEmpty(command)) break;

      _commandHandler.Execute(command);
    }
  }

  private string? ReadCommand()
  {
    if (Console.IsInputRedirected) return Console.ReadLine();

    StringBuilder command = new();
    _tabCount = 0;

    while (true)
    {
      ConsoleKeyInfo keyInfo = Console.ReadKey(true);
      var key = keyInfo.Key;

      if (key != ConsoleKey.Tab) _tabCount = 0;

      if (key == ConsoleKey.Enter)
      {
        Console.WriteLine();
        break;
      }
      else if (key == ConsoleKey.Backspace)
      {
        if (command.Length == 0) continue;
        command.Remove(command.Length - 1, 1);
        Console.Write("\b \b");
      }
      else if (key == ConsoleKey.Tab)
      {
        HandleTab(command);
      }
      else if (!char.IsControl(keyInfo.KeyChar))
      {
        command.Append(keyInfo.KeyChar);
        Console.Write(keyInfo.KeyChar);
      }
    }

    return command.ToString();
  }

  private void HandleTab(StringBuilder command)
  {
    string current = command.ToString();
    int index = current.LastIndexOf(' ') + 1;
    string word = current[index..];
    bool isCommand = index == 0;

    string[] matches = _autoCompletion.GetSuggestions(current, index);
    _tabCount++;

    if (matches.Length == 0)
    {
      Console.Write('\a');
      return;
    }

    if (matches.Length == 1)
    {
      ReplaceWord(command, index, matches[0]);
      _tabCount = 0;
      return;
    }

    if (isCommand)
    {
      string lcp = FindLCP(matches);
      if (lcp.Length > word.Length)
      {
        ReplaceWord(command, index, lcp);
        _tabCount = 0;
        return;
      }
    }

    if (_tabCount == 1)
    {
      Console.Write('\a');
      return;
    }

    Console.WriteLine();
    Console.WriteLine(string.Join("  ", matches.Select(DisplayName)));
    Console.Write("$ " + command);
  }

  private static void ReplaceWord(StringBuilder command, int index, string replacement)
  {
    command.Remove(index, command.Length - index);
    command.Append(replacement);
    Console.Write("\r\x1b[K$ " + command);
  }

  private static string DisplayName(string match)
  {
    string trimmed = match.TrimEnd(' ');
    bool isDir = trimmed.EndsWith('/') || trimmed.EndsWith(Path.DirectorySeparatorChar);
    string name = Path.GetFileName(trimmed.TrimEnd('/', Path.DirectorySeparatorChar));
    return isDir ? name + Path.DirectorySeparatorChar : name;
  }

  private static string FindLCP(string[] strings)
  {
    string prefix = strings[0];
    foreach (var s in strings.Skip(1))
    {
      while (!s.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
      {
        prefix = prefix[..^1];
        if (prefix.Length == 0) return "";
      }
    }
    return prefix;
  }
}