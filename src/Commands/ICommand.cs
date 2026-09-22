using System;

namespace CodeCrafters.Shell.src.Commands;

public interface ICommand
{
  string Name {get;}
  void Execute(string[] args, TextWriter output, TextWriter error);
}
