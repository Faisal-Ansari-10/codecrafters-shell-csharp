using System;

namespace CodeCrafters.Shell.src.Commands;

public class HistoryCommand : ICommand
{
  public string Name => "history";

  public Task Execute(string[] args, TextWriter output, TextWriter error)
  {
    return Task.CompletedTask;
  }
}
