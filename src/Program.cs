using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using CodeCrafters.Shell.src;
using CodeCrafters.Shell.src.Commands;

ShellState shellState = new();

CompleteRegistry completeRegistry = new();
ProcessRunner runner = new();
JobManager jobManager = new(runner);
FileStore fileStore = new();
CommandHistory commandHistory = new(fileStore);
HistoryNavigator historyNavigator = new(commandHistory);

BuiltinCommandRegistry builtins = new();
builtins.Register(new EchoCommand());
builtins.Register(new PwdCommand());
builtins.Register(new CdCommand());
builtins.Register(new ExitCommand(shellState));
builtins.Register(new TypeCommand(builtins));
builtins.Register(new CompleteCommand(completeRegistry));
builtins.Register(new JobsCommand(jobManager));
builtins.Register(new HistoryCommand(commandHistory));

var cmdDispatcher = new CommandDispatcher(builtins, jobManager);
var autoCompletion = new AutoCompletionHandler(builtins, completeRegistry, runner);
var shell = new Shell(shellState, cmdDispatcher, autoCompletion, jobManager, commandHistory, historyNavigator);
await shell.Run();

return shellState.ExitCode;
