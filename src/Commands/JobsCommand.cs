using System;

namespace CodeCrafters.Shell.src.Commands;

public class JobsCommand(IJobManager jobManager) : ICommand
{
  public string Name => "jobs";


  public async Task Execute(string[] args, TextWriter output, TextWriter error)
  {
    var snapshot = jobManager.SnapshotJobs();

    for (int i = 0; i < snapshot.Count; i++)
    {
      var job = snapshot[i];
      var jobLine = jobManager.FormatJobLine(i, snapshot);
      await output.WriteLineAsync(jobLine);
      if (job.Completed) jobManager.Kill(job);
    }
  }
}
