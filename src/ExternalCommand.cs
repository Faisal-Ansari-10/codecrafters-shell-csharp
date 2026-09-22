using System;
using System.Diagnostics;

namespace CodeCrafters.Shell.src;

public class ExternalCommand
{
  public static void Run(string name, string[] args, TextWriter output, TextWriter error)
  {
    string? resolvedPath = ResolveForValidation(name);
    if (resolvedPath is null)
    {
      error.WriteLine($"{name}: not found");
      return;
    }

    if (!Utils.IsExecutable(resolvedPath))
    {
      error.WriteLine($"{name}: no execute permission");
      return;
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

    using var process = new Process { StartInfo = startInfo };
    process.OutputDataReceived += (_, e) => { if (e.Data is not null) output.WriteLine(e.Data); };
    process.ErrorDataReceived += (_, e) => { if (e.Data is not null) error.WriteLine(e.Data); };

    process.Start();
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();
    process.WaitForExit();
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
