using System;

namespace CodeCrafters.Shell.src;

public interface ICommandHistory
{
  IReadOnlyList<string> Entries { get; }
  void Add(string command);
}

public class CommandHistory : ICommandHistory
{
  private readonly List<string> _entries = [];
  public IReadOnlyList<string> Entries => _entries.AsReadOnly();

  public void Add(string command)
  {
    _entries.Add(command);
  }
}
