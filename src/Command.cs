using System;

namespace CodeCrafters.Shell.src;

public class Command
{
  public string Name = "";
  public List<string> Args = [];
  public string? StdoutFile;
  public bool AppendStdout;
  public string? StderrFile;
  public bool AppendStderr;
  public string? StdinFile;
}

public class CommandLine
{
  public List<Command> Pipeline = [];
}
