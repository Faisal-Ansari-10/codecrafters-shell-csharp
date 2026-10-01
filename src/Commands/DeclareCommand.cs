using System;

namespace CodeCrafters.Shell.src.Commands;

public class DeclareCommand : ICommand
{
  public string Name => "declare";

  public  Task Execute(string[] args, TextWriter output, TextWriter error)
  {
    if (args.Length > 1 && args[0].Equals("-p"))
    {
      return output.WriteLineAsync($"{Name}: {args[1]}: not found");
    }

    return Task.CompletedTask;
  }
}
