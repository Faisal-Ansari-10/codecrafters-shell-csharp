using System;

namespace CodeCrafters.Shell.src;

public class ShellState
{
  public bool ExitRequested { get; private set; }
  public int ExitCode { get; private set; }

  public void RequestExit(int code = 0)
  {
    ExitRequested = true;
    ExitCode = code;
  }
}
