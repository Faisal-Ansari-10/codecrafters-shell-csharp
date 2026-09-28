using System;

namespace CodeCrafters.Shell.src;

public class HistoryNavigator(ICommandHistory history)
{
  private int _position = history.Entries.Count;

  public string? Previous()
  {
    var entries = history.Entries;
    if (entries.Count == 0 || _position == 0) return null;

    _position--;
    return entries[_position];
  }

  public string? Next()
  {
    var entries = history.Entries;
    if (_position >= entries.Count) return null;

    _position++;
    return _position == entries.Count ? null : entries[_position];
  }

  public void Reset()
  {
    _position = history.Entries.Count;
  }

}
