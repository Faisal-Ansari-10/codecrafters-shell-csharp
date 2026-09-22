using System;
using CodeCrafters.Shell.src.Commands;

namespace CodeCrafters.Shell.src;

public interface ICommandRegistry
{
  void Register(ICommand command);
  bool TryGet(string name, out ICommand? command);
  bool IsRegistered(string name);
  IReadOnlyCollection<string> Names { get; }
}

public class BuiltinCommandRegistry : ICommandRegistry
{
  private readonly Dictionary<string, ICommand> _commands = [];
  public IReadOnlyCollection<string> Names => _commands.Keys;

  public bool IsRegistered(string name) => _commands.ContainsKey(name);

  public void Register(ICommand command) => _commands[command.Name] = command;

  public bool TryGet(string name, out ICommand? command) => _commands.TryGetValue(name, out command);
}
