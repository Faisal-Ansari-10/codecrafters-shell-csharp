namespace CodeCrafters.Shell.src.Commands;

public class TypeCommand(ICommandRegistry builtins) : ICommand
{
  public string Name => "type";
  private readonly ICommandRegistry _builtins = builtins;

  public Task Execute(string[] args, TextWriter output, TextWriter error)
  {
    if (_builtins.TryGet(args[0], out var _))
    {
      return output.WriteLineAsync($"{args[0]} is a shell builtin");
    }

    var exePath = Utils.FindExecutable(args[0]);
    if (exePath.Length > 0 && Utils.IsExecutable(exePath[0]))
    {
      return output.WriteLineAsync($"{args[0]} is {exePath[0]}");
    }

    return output.WriteLineAsync($"{args[0]}: not found");
  }
}