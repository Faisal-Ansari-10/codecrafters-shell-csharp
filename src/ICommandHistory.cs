using System;

namespace CodeCrafters.Shell.src;

public interface ICommandHistoryStore
{
  Task<IReadOnlyList<string>> LoadAsync(string path);

  Task WriteAsync(string path, List<string> history);

  Task AppendAsync(string path, List<string> history);
}

public sealed class FileStore : ICommandHistoryStore
{
  public async Task AppendAsync(string path, List<string> history)
  {
    var directory = Path.GetDirectoryName(path);
    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

    await File.AppendAllLinesAsync(path, history);
  }

  public async Task<IReadOnlyList<string>> LoadAsync(string path)
  {
    try
    {
      return (await File.ReadAllLinesAsync(path))
      .Where(c => !string.IsNullOrEmpty(c))
      .ToList()
      .AsReadOnly();
    }
    catch
    {
      return [];
    }
  }

  public async Task WriteAsync(string path, List<string> history)
  {

    var directory = Path.GetDirectoryName(path);
    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

    await File.WriteAllLinesAsync(path, history);
  }
}

public interface ICommandHistory
{
  IReadOnlyList<string> Entries { get; }
  void Add(string command);

  Task LoadAsync(string path);

  Task WriteAsync(string path);

  Task AppendAsync(string path);
}

public class CommandHistory(ICommandHistoryStore historyStore) : ICommandHistory
{
  private readonly List<string> _entries = [];
  public IReadOnlyList<string> Entries => _entries.AsReadOnly();
  private int _writeOffset = 0;

  public void Add(string command)
  {
    _entries.Add(command);
  }

  public async Task AppendAsync(string path)
  {
    try
    {
      await historyStore.AppendAsync(path, [.. _entries.Skip(_writeOffset)]);
      _writeOffset = _entries.Count;
    }
    catch { }
  }

  public async Task LoadAsync(string path)
  {
    foreach (var c in await historyStore.LoadAsync(path))
    {
      _entries.Add(c);
    }
  }

  public async Task WriteAsync(string path)
  {
    try
    {
      try
      {
        await historyStore.WriteAsync(path, [.. _entries.Skip(_writeOffset)]);
        _writeOffset = _entries.Count;
      }
      catch { }
    }
    catch { }
  }
}
