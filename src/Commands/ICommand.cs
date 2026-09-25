using System;

namespace CodeCrafters.Shell.src.Commands;

public interface ICommand
{
  string Name {get;}
  Task Execute(string[] args, TextWriter output, TextWriter error);
}
