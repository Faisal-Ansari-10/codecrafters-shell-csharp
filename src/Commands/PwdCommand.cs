using System;

namespace CodeCrafters.Shell.src.Commands;

public class PwdCommand : ICommand
{
  public string Name => "pwd";

  public void Execute(string[] args, TextWriter output, TextWriter error) =>
    output.WriteLine(Directory.GetCurrentDirectory());
}
