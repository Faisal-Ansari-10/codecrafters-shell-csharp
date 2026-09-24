using System;

namespace CodeCrafters.Shell.src;

public interface ICompleteRegistry
{
  bool TryGet(string name, out string path);
  void Add(string name, string path);

  bool Remove(string name);
}


public class CompleteRegistry : ICompleteRegistry
{
  private readonly Dictionary<string, string> _scripts = [];

  public void Add(string name, string path) => _scripts[name.Trim()] = path.Trim();

  public bool TryGet(string name, out string path) => _scripts.TryGetValue(name.Trim(), out path!);

  public bool Remove(string name)
  {
    var key = name.Trim();
    return _scripts.Remove(key);
  }
}