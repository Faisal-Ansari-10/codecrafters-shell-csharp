using System;

namespace CodeCrafters.Shell.src.Commands;

public class HistoryCommand(ICommandHistory commandHistory) : ICommand
{
  public string Name => "history";

  public async Task Execute(string[] args, TextWriter output, TextWriter error)
  {
    var entries = commandHistory.Entries;

    for (int i = 0; i < entries.Count; i++)
    {
      await output.WriteLineAsync($"{i + 1,5}  {entries[i]}");
    }
  }
}
