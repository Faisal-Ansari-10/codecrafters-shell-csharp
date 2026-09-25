using System;

namespace CodeCrafters.Shell.src.Commands;

public class ExitCommand : ICommand
{
  public string Name => "exit";

  public Task Execute(string[] args, TextWriter output, TextWriter error)
  {
    Environment.Exit(0);
    return Task.CompletedTask;
  }
}
