using System;
using System.Diagnostics;

namespace CodeCrafters.Shell.src;

public sealed record Job(int Id, int ProcessId, Task Completion)
{
  public bool IsRunning => !Completion.IsCompleted;
}

public interface IJobManager
{
  Job Start(string name, string[] args, TextWriter output, TextWriter error);
  IReadOnlyList<Job> List();
}

public class JobManager : IJobManager
{
  private int _jobId = 0;
  private readonly List<Job> _jobs = [];
  public Job Start(string name, string[] args, TextWriter output, TextWriter error)
  {
    var (processId, processTask) = Run(name, args, output, error);
    _jobId++;
    Job job = new(_jobId, processId, processTask);
    _jobs.Add(job);
    return job;
  }

  public IReadOnlyList<Job> List() => _jobs.AsReadOnly();

  private static (int processId, Task processTask) Run(string name, string[] args, TextWriter output, TextWriter error)
  {
    string? resolvedPath = ResolveForValidation(name);
    if (resolvedPath is null)
    {
      error.WriteLine($"{name}: not found");
      return new(-1, Task.CompletedTask);
    }

    if (!Utils.IsExecutable(resolvedPath))
    {
      error.WriteLine($"{name}: no execute permission");
      return new(-1, Task.CompletedTask);
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

     var process = new Process { StartInfo = startInfo, EnableRaisingEvents=true };
    process.OutputDataReceived += (_, e) => { if (e.Data is not null) output.WriteLine(e.Data); };
    process.ErrorDataReceived += (_, e) => { if (e.Data is not null) error.WriteLine(e.Data); };

    process.Start();
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();

    var pid = process.Id;
    var completion = process.WaitForExitAsync()
    .ContinueWith(_ => process.Dispose(), TaskScheduler.Default);

    return new(pid, completion);
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


