using System;
using CodeCrafters.Shell.src.Commands;


namespace CodeCrafters.Shell.src;

public class CommandDispatcher(ICommandRegistry builtinCommands, IJobManager jobManager)
{

  public async Task Run(string name, string[] args, bool runInBackground, TextWriter output, TextWriter error)
  {
    Task task;
    if (builtinCommands.TryGet(name, out var cmd))
    {
      task = cmd!.Execute(args, output, error);
    }
    else
    {
      var job = jobManager.Start(name, args, output, error);
      if(runInBackground)
      {
        await output.WriteLineAsync($"[{job.Id}] {job.ProcessId}");
      }
      task = job.Completion;
    }

    if (!runInBackground) await task;
  }
}
