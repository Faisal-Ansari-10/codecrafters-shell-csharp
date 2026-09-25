using System;
using System.Diagnostics;

namespace CodeCrafters.Shell.src;

public sealed record Job(int Id, int ProcessId, string[] Command, Task Completion, bool IsBackground, Func<bool> IsRunning);

public interface IJobManager
{
  Job Start(string name, string[] args, TextWriter output, TextWriter error, bool isBackground);
  IReadOnlyList<Job> List();
  bool Kill(int id);

}

public class JobManager(IProcessRunner runner) : IJobManager
{
  private int _jobId = 0;
  private readonly List<Job> _jobs = [];

  public Job Start(string name, string[] args, TextWriter output, TextWriter error, bool isBackground)
  {
    var (processId, completion, isRunning) = runner.Run(name, args, output, error);
    _jobId++;
    var command = new[] { name }.Concat(args).ToArray();
    Job job = new(_jobId, processId, command, completion, isBackground, isRunning);
    _jobs.Add(job);
    return job;
  }

  public IReadOnlyList<Job> List() => _jobs.AsReadOnly();

  public bool Kill(int id)
  {
    var index = _jobs.FindIndex(j => j.Id == id);
    if (index == -1) return false;

    _jobs.RemoveAt(index);
    return true;
  }
}


