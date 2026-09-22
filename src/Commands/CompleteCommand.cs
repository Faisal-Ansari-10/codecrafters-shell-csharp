using System;

namespace CodeCrafters.Shell.src.Commands;

public class CompleteCommand(ICompleteRegistry completeRegistry) : ICommand
{
  public string Name => "complete";
  private readonly ICompleteRegistry _registry = completeRegistry;

  public void Execute(string[] args, TextWriter output, TextWriter error)
  {
    var flag = args[0];

    if (flag.Equals("-C"))
    {
      _registry.Register(args[2], args[1]);
      return;
    }

    if (flag.Equals("-p"))
    {
      if (_registry.TryGet(args[1], out var path))
      {
        output.WriteLine($"complete -C '{path}' {args[1]}");
      }
      else
      {
        output.WriteLine($"complete: {args[1]}: no completion specification");
      }

      return;
    }
  }
}
