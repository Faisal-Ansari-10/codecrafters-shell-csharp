using System;

namespace CodeCrafters.Shell.src.Commands;

public class EchoCommand : ICommand
{
  public string Name => "echo";

  public void Execute(string[] args, TextWriter output, TextWriter error) =>
    output.WriteLine(string.Join(' ', args));
}
