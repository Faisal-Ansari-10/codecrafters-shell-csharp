using System;

namespace CodeCrafters.Shell.src.Commands;

public class CdCommand : ICommand
{
  public string Name => "cd";

  public void Execute(string[] args, TextWriter output, TextWriter error)
  {
    var directory = args[0];
    if (directory == "~")
    {
      directory = Environment.GetEnvironmentVariable("HOME");
    }

    if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
    {
      output.WriteLine($"cd: {directory}: No such file or directory");
      return;
    }

    try
    {
      Directory.SetCurrentDirectory(directory);
    }
    catch (Exception e) { error.WriteLine(e.Message); }
  }
}
