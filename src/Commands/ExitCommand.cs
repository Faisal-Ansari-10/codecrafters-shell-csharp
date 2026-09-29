using System;

namespace CodeCrafters.Shell.src.Commands;

public class ExitCommand(ShellState state) : ICommand
{
  public string Name => "exit";

  public Task Execute(string[] args, TextWriter output, TextWriter error)
  {
    int code = args.Length > 0 && int.TryParse(args[0], out var c) ? c : 0;
    state.RequestExit(code);
    return Task.CompletedTask;
  }
}
