using System;

namespace CodeCrafters.Shell.src.Commands;

public class JobsCommand(IJobManager jobManager) : ICommand
{
  public string Name => "jobs";

  private readonly IJobManager _jobManager = jobManager;

  public Task Execute(string[] args, TextWriter output, TextWriter error)
  {
    var latestUnfinishedJob = _jobManager.List().LastOrDefault(j => j.IsRunning);

    if (latestUnfinishedJob is null) return Task.CompletedTask;

    return output.WriteLineAsync(string.Format("[{0}]+  {1,-24}{2}", latestUnfinishedJob.Id, latestUnfinishedJob.IsRunning ? "Running" : "", latestUnfinishedJob.Command));
  }
}
