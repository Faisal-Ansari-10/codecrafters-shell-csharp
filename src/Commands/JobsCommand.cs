using System;

namespace CodeCrafters.Shell.src.Commands;

public class JobsCommand(IJobManager jobManager) : ICommand
{
  public string Name => "jobs";

  private readonly IJobManager _jobManager = jobManager;

  public async Task Execute(string[] args, TextWriter output, TextWriter error)
  {
    var jobs = _jobManager.List()
    .Where(j => j.IsBackground)
    .Where(j => j.IsRunning)
    .OrderBy(j => j.Id)
    .ToArray();


    for (int i = 0; i < jobs.Length; i++)
    {
      char marker = i == jobs.Length - 1 ? '+' : i == jobs.Length - 2 ? '-' : ' ';
      await output.WriteLineAsync($"[{jobs[i].Id}]{marker}  {"Running",-24}{jobs[i].Command}");
    }
  }
}
