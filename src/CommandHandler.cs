using System.Diagnostics;
using System.Text;
using Microsoft.VisualBasic;
using RL = ReadLine.ReadLine;


class CommandHandler
{
  private readonly struct CommandResult(string output = "", string error = "")
  {
    public readonly string Output = output;
    public readonly string Error = error;
  }

  private string? _input = null;
  private readonly Lexer _lexer;
  public string? Input => _input;
  private readonly string[] _builtIns = ["echo", "exit", "type", "pwd", "cd"];
  private readonly string[] _redirectOperators = [">", "1>", "2>", ">>", "1>>", "2>>"];

  public CommandHandler(Lexer lexer)
  {
    _lexer = lexer;
    RL.Context.AutoCompletionHandler = new AutoCompletionHandler();

  }
  public void Read()
  {
    _input = RL.Read("$ ");
  }

  public void Execute()
  {
    if (_input is null) Environment.Exit(0);
    _lexer.Parse(_input);

    if (_lexer.Tokens.Count < 1) return;

    CommandResult result = new(output: $"{_lexer.Tokens[0]}: not found");
    string cmd = _lexer.Tokens[0];
    int redirectOperatorIndex = _lexer.Tokens.Select((token, index) => new { token, index })
    .FirstOrDefault(x => _redirectOperators.Contains(x.token))?.index ?? int.MaxValue;

    List<string> args = [];
    for (int i = 1; i < Math.Min(_lexer.Tokens.Count, redirectOperatorIndex); i++)
    {
      args.Add(_lexer.Tokens[i]);
    }

    switch (_lexer.Tokens[0])
    {
      case "exit":
        Environment.Exit(0);
        break;

      case "echo":
        result = HandleEchoCommand(args);
        break;

      case "type":
        if (args.Count > 0)
          result = HandleTypeCommand(args[0]);
        break;

      case "pwd":
        result = HandlePwdCommand();
        break;

      case "cd":
        if (args.Count > 0)
          result = HandleChangeDirectoryCommand(args[0]);
        break;

      default:
        result = HandleExecuteCommand(cmd, args);
        break;
    }

    if (redirectOperatorIndex < _lexer.Tokens.Count)
    {
      string redirectOperator = _lexer.Tokens[redirectOperatorIndex];
      bool isAppend = redirectOperator.EndsWith(">>");
      bool isStdError = redirectOperator.StartsWith('2');
      string? redirectTarget = redirectOperatorIndex + 1 < _lexer.Tokens.Count
      ? _lexer.Tokens[redirectOperatorIndex + 1]
      : null;

      if (redirectTarget is null) return;
      var content = isStdError ? result.Error : result.Output;
      Utils.WriteToFile(redirectTarget, content, isAppend);

      if (isStdError)
      {
        if (!string.IsNullOrEmpty(result.Output)) Console.WriteLine(result.Output.TrimEnd('\n'));
      }
      else
      {
        if (!string.IsNullOrEmpty(result.Error)) Console.Error.WriteLine(result.Error.TrimEnd('\n'));
      }
    }
    else
    {
      if (!string.IsNullOrEmpty(result.Output)) Console.WriteLine(result.Output.TrimEnd('\n'));
      if (!string.IsNullOrEmpty(result.Error)) Console.Error.WriteLine(result.Error.TrimEnd('\n'));
    }

  }

  private static CommandResult HandleEchoCommand(List<string> args)
  {
    return new(output: string.Join(' ', args).Trim(), error: "");
  }

  private CommandResult HandleTypeCommand(string command)
  {
    if (_builtIns.Contains(command))
    {
      return new(output: $"{command} is a shell builtin");
    }
    else
    {
      var exePath = Utils.FindExecutable(command);
      if (exePath is not null && (OperatingSystem.IsWindows() || Utils.IsExecutable(exePath)))
        return new(output: $"{command} is {exePath}");
    }

    return new(output: $"{command}: not found");
  }

  private static CommandResult HandlePwdCommand()
  {
    var pwd = Directory.GetCurrentDirectory();
    return new(output: pwd);
  }
  private static CommandResult HandleChangeDirectoryCommand(string directory)
  {
    if (directory == "~")
    {
      directory = Environment.GetEnvironmentVariable("HOME") ?? directory;
    }

    if (!Directory.Exists(directory))
    {
      return new(output: $"cd: {directory}: No such file or directory");
    }
    try
    {
      Directory.SetCurrentDirectory(directory);
      return new();
    }
    catch (Exception e) { return new(error: e.Message); }
  }


  private static CommandResult HandleExecuteCommand(string command, List<string> args)
  {
    var exePath = Utils.FindExecutable(command);
    if (exePath is null)
      return new(output: $"{command}: not found");

    if (!OperatingSystem.IsWindows() && !Utils.IsExecutable(exePath))
      return new(output: $"{command}: no execute permission");

    var exeName = OperatingSystem.IsWindows()
                    ? exePath
                    : Path.GetFileName(exePath);
    var startInfo = new ProcessStartInfo
    {
      FileName = exeName,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      UseShellExecute = false,
      CreateNoWindow = true
    };

    foreach (var a in args) startInfo.ArgumentList.Add(a);

    using var process = new Process { StartInfo = startInfo };

    var outputBuilder = new StringBuilder();
    var errorBuilder = new StringBuilder();

    process.OutputDataReceived += (_, e) => { if (e.Data is not null) outputBuilder.AppendLine(e.Data); };
    process.ErrorDataReceived += (_, e) => { if (e.Data is not null) errorBuilder.AppendLine(e.Data); };

    process.Start();
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();
    process.WaitForExit();

    var output = outputBuilder.ToString();
    var error = errorBuilder.ToString();

    return new(output, error);
  }
}