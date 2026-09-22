using System;

namespace CodeCrafters.Shell.src;

public interface ICompleteRegistry
{
  bool TryGet(string name, out string path);
  void Register(string name, string path);
}


public class CompleteRegistry : ICompleteRegistry
{
  private readonly Dictionary<string, string> _scripts = [];

  public void Register(string name, string path) => _scripts[name.Trim()] = path.Trim();

  public bool TryGet(string name, out string path) => _scripts.TryGetValue(name.Trim(), out path!);
}