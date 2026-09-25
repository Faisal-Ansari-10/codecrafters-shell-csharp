using System;

namespace CodeCrafters.Shell.src.Commands;

public class EchoCommand : ICommand
{
  public string Name => "echo";

  public Task Execute(string[] args, TextWriter output, TextWriter error) =>
   output.WriteLineAsync(string.Join(' ', args));
  
}
