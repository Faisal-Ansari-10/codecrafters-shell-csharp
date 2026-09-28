using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CodeCrafters.Shell.src;

public sealed record RunContext(
  string Name,
  TextReader Input,
  bool RedirectStandardInput,
  string[] Args,
  TextWriter Output,
  TextWriter Error
);
public interface IProcessRunner
{
  Task<(int processId, Task completion, Func<bool> isRunning)> Run(RunContext context);
}

public class ProcessRunner : IProcessRunner
{
  async Task<(int processId, Task completion, Func<bool> isRunning)> IProcessRunner.Run(RunContext context)
  {
    var (name, input, redirectStdin, args, output, error) = context;

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
      RedirectStandardInput = redirectStdin,
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

    if (redirectStdin) _ = PumpInputAsync(input, process);
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

  private static async Task PumpInputAsync(TextReader input, Process process)
  {
    try
    {
      var stdin = process.StandardInput;
      stdin.AutoFlush = true;
      var buffer = new char[4096];
      int n;
      while ((n = await input.ReadAsync(buffer)) > 0)
        await stdin.WriteAsync(buffer.AsMemory(0, n));
    }
    catch (IOException) { }
    catch (ObjectDisposedException) { }
    catch (InvalidOperationException) { }
    finally
    {
      try { process.StandardInput.Close(); } catch { }
    }
  }

}
