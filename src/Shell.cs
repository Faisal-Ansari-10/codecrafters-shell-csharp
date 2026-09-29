using System.IO.Pipelines;
using System.Text;
using CodeCrafters.Shell.src;

class Shell(
  ShellState state,
  CommandDispatcher commandDispatcher,
AutoCompletionHandler autoCompletion,
IJobManager jobManager,
ICommandHistory commandHistory,
HistoryNavigator historyNavigator)
{
  private int _tabCount;

  public async Task Run()
  {
    await LoadCommandHistory();

    while (!state.ExitRequested)
    {
      RemoveCompletedJobs();
      historyNavigator.Reset();

      Console.Write("$ ");
      string? inputLine = await ReadInput();
      if (inputLine is null) break;
      if (inputLine.Length == 0) continue;

      commandHistory.Add(inputLine);

      var lexer = new Lexer(inputLine);
      var tokens = lexer.Tokenize();

      var parser = new Parser(tokens);
      var line = parser.Parse();
      var n = line.Pipeline.Count;
      var pipes = Enumerable.Range(0, n - 1).Select(_ => new Pipe()).ToArray();
      var stages = new List<Task>();

      for (int i = 0; i < n; i++)
      {
        var cmd = line.Pipeline[i];
        bool isFirst = i == 0, isLast = i == n - 1;

        TextReader input = isFirst
          ? TextReader.Null
          : new StreamReader(pipes[i - 1].Reader.AsStream());

        TextWriter output = isLast
          ? (cmd.StdoutFile is not null
              ? new StreamWriter(cmd.StdoutFile, append: cmd.AppendStdout)
              : Console.Out)
          : new StreamWriter(pipes[i].Writer.AsStream()) { AutoFlush = true };

        TextWriter error = cmd.StderrFile is not null
          ? new StreamWriter(cmd.StderrFile, append: cmd.AppendStderr)
          : Console.Error;

        var context = new RunContext(
          Name: cmd.Name,
          Input: input,
          RedirectStandardInput: !isFirst,
          Args: [.. cmd.Args],
          Output: output,
          Error: error);

        Task completion = await commandDispatcher.Start(context, line.RunInBackground);
        stages.Add(Finish(completion, input, output, error));
      }

      if (!line.RunInBackground) await Task.WhenAll(stages);
    }

    await SaveCommandHistory();
  }


  private static async Task Finish(Task completion, params IDisposable[] resources)
  {
    try { await completion; }
    finally
    {
      foreach (var resource in resources)
        if (!ReferenceEquals(resource, Console.Out) && !ReferenceEquals(resource, Console.Error))
          resource.Dispose();
    }
  }
  private async Task<string?> ReadInput()
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
        await HandleTab(input);
      }
      else if (key == ConsoleKey.UpArrow)
      {
        var previous = historyNavigator.Previous();

        if (previous is not null)
        {
          input.Clear();
          input.Append(previous);

          ReplaceWord(input, 0, previous);
        }
      }
      else if (key == ConsoleKey.DownArrow)
      {
        var next = historyNavigator.Next();

        if (next is not null)
        {
          input.Clear();
          input.Append(next);

          ReplaceWord(input, 0, next);
        }
      }
      else if (!char.IsControl(keyInfo.KeyChar))
      {
        input.Append(keyInfo.KeyChar);
        Console.Write(keyInfo.KeyChar);
      }
    }

    return input.ToString();
  }

  private async Task HandleTab(StringBuilder input)
  {
    string current = input.ToString();
    int index = current.LastIndexOf(' ') + 1;
    string word = current[index..];

    string[] matches = await autoCompletion.GetSuggestions(current, index);
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

  private void RemoveCompletedJobs()
  {
    var jobs = jobManager.SnapshotJobs();
    for (int i = 0; i < jobs.Count; i++)
    {
      var job = jobs[i];
      if (!job.Completed) continue;

      var jobline = jobManager.FormatJobLine(i, jobs);
      Console.Out.WriteLine(jobline);
      jobManager.Kill(job);
    }
  }

  private async Task LoadCommandHistory()
  {
    var path = Environment.GetEnvironmentVariable("HISTFILE");
    if (string.IsNullOrEmpty(path)) return;

    await commandHistory.LoadAsync(path);
  }

  private async Task SaveCommandHistory()
  {
    var path = Environment.GetEnvironmentVariable("HISTFILE");
    if (string.IsNullOrEmpty(path)) return;

    await commandHistory.WriteAsync(path);
  }
}