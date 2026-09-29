using System;

namespace CodeCrafters.Shell.src.Commands;

public class HistoryCommand(ICommandHistory commandHistory) : ICommand
{
  public string Name => "history";

  public async Task Execute(string[] args, TextWriter output, TextWriter error)
  {

    if (args.Length > 0 && args[0].Equals("-r"))
    {
      await commandHistory.LoadAsync(args[1]);

      return;
    }
    else if (args.Length > 0 && args[0].Equals("-w"))
    {
      await commandHistory.WriteAsync(args[1]);
      return;
    }
    else if (args.Length > 0 && args[0].Equals("-a"))
    {
      await commandHistory.AppendAsync(args[1]);
      return;
    }

    var entries = commandHistory.Entries;
    int limit = entries.Count;

    if (args.Length > 0 && int.TryParse(args[0], out int n) && n >= 0)
      limit = Math.Min(n, entries.Count);

    int start = entries.Count - limit;

    foreach (var line in entries
      .Skip(start)
      .Select((command, i) => $"{start + i + 1,5}  {command}"))
    {
      await output.WriteLineAsync(line);
    }
  }
}
