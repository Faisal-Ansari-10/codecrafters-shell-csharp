using System;
using System.Diagnostics;

namespace CodeCrafters.Shell.src;

public interface IProcessRunner
{
  (int processId, Task completion, Func<bool> isRunning) Run(string name, string[] args, TextWriter output, TextWriter error);
}

public class ProcessRunner : IProcessRunner
{
  public (int processId, Task completion, Func<bool> isRunning) Run(string name, string[] args, TextWriter output, TextWriter error)
  {
    string? resolvedPath = ResolveForValidation(name);
    if (resolvedPath is null)
    {
      error.WriteLine($"{name}: not found");
      return new(-1, Task.CompletedTask, () => false);
    }

    if (!Utils.IsExecutable(resolvedPath))
    {
      error.WriteLine($"{name}: no execute permission");
      return new(-1, Task.CompletedTask, () => false);
    }

    var startInfo = new ProcessStartInfo
    {
      FileName = name,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      UseShellExecute = false,
      CreateNoWindow = true
    };

    foreach (var a in args) startInfo.ArgumentList.Add(a);

    var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

    process.OutputDataReceived += (_, e) => { if (e.Data is not null) output.WriteLine(e.Data); };
    process.ErrorDataReceived += (_, e) => { if (e.Data is not null) error.WriteLine(e.Data); };

    process.Start();
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();

    var pid = process.Id;
    var completion = process.WaitForExitAsync()
    .ContinueWith(_ => process.Dispose(), TaskScheduler.Default);

    bool IsRunning()
    {
      try
      {
        process.Refresh();
        return !process.HasExited;
      }
      catch (InvalidOperationException)
      {
        return false;
      }
    }
    return new(pid, completion, IsRunning);
  }

  private static string? ResolveForValidation(string name)
  {
    bool looksLikePath = name.Contains(Path.DirectorySeparatorChar);

    if (looksLikePath)
      return File.Exists(name) ? name : null;

    var found = Utils.FindExecutable(name);
    return found.Length == 0 ? null : found[0];
  }

}
