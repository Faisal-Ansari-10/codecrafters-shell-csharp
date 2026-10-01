using System;

namespace CodeCrafters.Shell.src.Commands;

public class DeclareCommand : ICommand
{
  public string Name => "declare";

  public Task Execute(string[] args, TextWriter output, TextWriter error)
  {
    return Task.CompletedTask;
  }
}
