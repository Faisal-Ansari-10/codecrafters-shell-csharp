using System;
using CodeCrafters.Shell.src.Commands;


namespace CodeCrafters.Shell.src;

public class CommandDispatcher(ICommandRegistry builtinCommands)
{

  public void Run(string name, string[] args, TextWriter output, TextWriter error)
  {
    if(builtinCommands.TryGet(name, out var cmd))
      cmd!.Execute(args, output, error);
    else
      ExternalCommand.Run(name, args, output, error);
  }
}
