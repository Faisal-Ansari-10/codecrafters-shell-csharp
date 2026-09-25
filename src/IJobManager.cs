using System;
using System.Diagnostics;

namespace CodeCrafters.Shell.src;

public sealed record Job(int Id, int ProcessId, string Command, Task Completion, bool IsBackground)
{
  public bool IsRunning => !Completion.IsCompleted;
}

public interface IJobManager
{
  Job Start(string name, string[] args, TextWriter output, TextWriter error, bool isBackground);
  IReadOnlyList<Job> List();
}

public class JobManager(IProcessRunner runner) : IJobManager
{
  private int _jobId = 0;
  private readonly List<Job> _jobs = [];

  public Job Start(string name, string[] args, TextWriter output, TextWriter error, bool isBackground)
  {
    var (processId, completion) = runner.Run(name, args, output, error);
    _jobId++;
    Job job = new(_jobId, processId, string.Join(' ', new[] { name }.Concat(args).Concat(isBackground ? new[] { "&" } : [])), completion, isBackground);
    _jobs.Add(job);
    return job;
  }

  public IReadOnlyList<Job> List() => _jobs.AsReadOnly();

}


