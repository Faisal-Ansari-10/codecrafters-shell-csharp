using System;
using CodeCrafters.Shell.src.Commands;


namespace CodeCrafters.Shell.src;

public class CommandDispatcher(ICommandRegistry builtinCommands, IJobManager jobManager)
{

  public async Task<Task> Start(RunContext context, bool runInBackground)
  {
    if (builtinCommands.TryGet(context.Name, out var cmd))
    {
      return Task.Run(() => cmd!.Execute(context.Args, context.Output, context.Error));
    }

    var job = await jobManager.Start(context, runInBackground);
    if (runInBackground)
      await context.Output.WriteLineAsync($"[{job.Id}] {job.ProcessId}");
    return job.Completion;

  }
}
