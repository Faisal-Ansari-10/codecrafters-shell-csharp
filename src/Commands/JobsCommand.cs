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
      .OrderBy(j => j.Id)
      .ToArray();

    List<int> finishedJobIds = [];
    for (int i = 0; i < jobs.Length; i++)
    {
      char marker = i == jobs.Length - 1 ? '+' : i == jobs.Length - 2 ? '-' : ' ';
      bool isRunning = jobs[i].IsRunning();
      string status = isRunning ? "Running" : "Done";

      if (!isRunning) finishedJobIds.Add(jobs[i].Id);
      
      var commandParts = isRunning ? jobs[i].Command.Append("&") : jobs[i].Command;
      string command = string.Join(' ', commandParts);

      await output.WriteLineAsync($"[{jobs[i].Id}]{marker}  {status,-24}{command}");
    }

    foreach (var id in finishedJobIds)
    {
      _jobManager.Kill(id);
    }
  }
}
