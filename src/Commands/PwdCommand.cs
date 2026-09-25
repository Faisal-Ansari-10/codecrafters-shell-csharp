using System;

namespace CodeCrafters.Shell.src.Commands;

public class PwdCommand : ICommand
{
  public string Name => "pwd";

  public Task Execute(string[] args, TextWriter output, TextWriter error) =>
    output.WriteLineAsync(Directory.GetCurrentDirectory());
}
