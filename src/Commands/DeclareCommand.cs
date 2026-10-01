using System;
using System.Text.RegularExpressions;

namespace CodeCrafters.Shell.src.Commands;

public class DeclareCommand : ICommand
{
  public string Name => "declare";

  private readonly Dictionary<string, string> _variables = [];

  public Task Execute(string[] args, TextWriter output, TextWriter error)
  {
    if (args.Length > 1 && args[0].Equals("-p"))
    {
      var outputLine = $"{Name}: {args[1]}: not found";
      if (_variables.TryGetValue(args[1], out var value))
      {
        outputLine = $"{Name} -- {args[1]}=\"{value}\"";
      }

      return output.WriteLineAsync(outputLine);
    }
    else if (args.Length > 0 && args[0].Contains('='))
    {
      var delimiterIndex = args[0].IndexOf('=');
      var variable = args[0][..delimiterIndex];
      var value = args[0][(delimiterIndex + 1)..];

      if (!Regex.IsMatch(variable, @"^[A-Za-z_][A-Za-z0-9_]*$"))
      {
        return output.WriteLineAsync($"{Name}: `{args[0]}': not a valid identifier");
      }

      _variables[variable] = value;

    }

    return Task.CompletedTask;
  }
}
