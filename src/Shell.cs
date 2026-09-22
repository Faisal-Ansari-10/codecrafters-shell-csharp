using System.Text;
using CodeCrafters.Shell.src;

class Shell(CommandDispatcher commandDispatcher, AutoCompletionHandler autoCompletion)
{
  private readonly AutoCompletionHandler _autoCompletion = autoCompletion;
  private int _tabCount;

  private readonly CommandDispatcher _dispatcher = commandDispatcher;

  public void Run()
  {
    while (true)
    {
      Console.Write("$ ");
      string? input = ReadInput();
      if (string.IsNullOrEmpty(input)) break;

      var lexer = new Lexer(input);
      var tokens = lexer.Tokenize();

      var parser = new Parser(tokens);
      var lines = parser.Parse();

      foreach (var line in lines.Pipeline)
      {
        using TextWriter output = line.StdoutFile is not null
        ? new StreamWriter(line.StdoutFile, append: line.AppendStdout)
        : Console.Out;

        using TextWriter error = line.StderrFile is not null
        ? new StreamWriter(line.StderrFile, append: line.AppendStderr)
        : Console.Error;

        try
        {
          _dispatcher.Run(name: line.Name, args: [.. line.Args], output: output, error: error);
        }
        finally
        {
          if (!ReferenceEquals(output, Console.Out)) output.Dispose();
          if (!ReferenceEquals(error, Console.Error)) error.Dispose();
        }
      }
    }
  }

  private string? ReadInput()
  {
    if (Console.IsInputRedirected) return Console.ReadLine();

    StringBuilder input = new();
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
        if (input.Length == 0) continue;
        input.Remove(input.Length - 1, 1);
        Console.Write("\b \b");
      }
      else if (key == ConsoleKey.Tab)
      {
        HandleTab(input);
      }
      else if (!char.IsControl(keyInfo.KeyChar))
      {
        input.Append(keyInfo.KeyChar);
        Console.Write(keyInfo.KeyChar);
      }
    }

    return input.ToString();
  }

  private void HandleTab(StringBuilder input)
  {
    string current = input.ToString();
    int index = current.LastIndexOf(' ') + 1;
    string word = current[index..];

    string[] matches = _autoCompletion.GetSuggestions(current, index);
    _tabCount++;

    if (matches.Length == 0)
    {
      Console.Write('\a');
      return;
    }

    if (matches.Length == 1)
    {
      ReplaceWord(input, index, matches[0]);
      _tabCount = 0;
      return;
    }

    string lcp = FindLCP(matches);
    if (lcp.Length > word.Length)
    {
      ReplaceWord(input, index, lcp);
      _tabCount = 0;
      return;
    }

    if (_tabCount == 1)
    {
      Console.Write('\a');
      return;
    }

    Console.WriteLine();
    Console.WriteLine(string.Join("  ", matches.Select(DisplayName)));
    Console.Write("$ " + input);
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