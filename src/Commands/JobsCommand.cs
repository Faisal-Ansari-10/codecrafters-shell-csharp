using System;

namespace CodeCrafters.Shell.src.Commands;

public class JobsCommand(IJobManager jobManager) : ICommand
{
  public string Name => "jobs";

  private readonly IJobManager _jobManager = jobManager;

  public async Task Execute(string[] args, TextWriter output, TextWriter error)
  {
    
  }
}
