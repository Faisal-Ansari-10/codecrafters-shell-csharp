using System;
using System.Diagnostics;

namespace CodeCrafters.Shell.src;

public sealed record Job(int Id, int ProcessId, string[] Command, Task Completion, bool IsBackground, Func<bool> IsRunning)
{
  public bool Completed => !IsRunning();
}

public interface IJobManager
{
  Job Start(string name, string[] args, TextWriter output, TextWriter error, bool isBackground);
  IReadOnlyList<Job> SnapshotJobs();
  bool Kill(Job job);

  string FormatJobLine(int index, IReadOnlyList<Job> jobs);

}

public class JobManager(IProcessRunner runner) : IJobManager
{
  private readonly List<Job> _jobs = [];

  private int JobId()
  {
    if(_jobs.Count == 0) return 1;
    return _jobs.Max(j => j.Id) + 1;
  }
  public Job Start(string name, string[] args, TextWriter output, TextWriter error, bool isBackground)
  {
    var (processId, completion, isRunning) = runner.Run(name, args, output, error);  
    var command = new[] { name }.Concat(args).ToArray();
    Job job = new(JobId(), processId, command, completion, isBackground, isRunning);
    _jobs.Add(job);
    return job;
  }



  public IReadOnlyList<Job> SnapshotJobs() => _jobs
                            .Where(j => j.IsBackground)
                            .OrderBy(j => j.Id)
                            .ToList()
                            .AsReadOnly();

  public bool Kill(Job job)
  {
    var index = _jobs.FindIndex(j => j.Id == job.Id);
    if (index == -1) return false;
    _jobs.RemoveAt(index);
    return true;
  }

  public string FormatJobLine(int index, IReadOnlyList<Job> jobs)
  {
    var job = jobs[index];
    bool isRunning = job.IsRunning();
    string status = isRunning ? "Running" : "Done";

    var commandParts = isRunning ? job.Command.Append("&") : job.Command;
    string command = string.Join(' ', commandParts);

    char marker = Marker(index, jobs.Count);
    return $"[{job.Id}]{marker}  {status,-24}{command}";
  }

  private static char Marker(int index, int count) =>
  index == count - 1 ? '+' : index == count - 2 ? '-' : ' ';

}