using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using CodeCrafters.Shell.src;
using CodeCrafters.Shell.src.Commands;

CompleteRegistry completeRegistry = new();
JobManager jobManager = new();
BuiltinCommandRegistry builtins = new();
builtins.Register(new EchoCommand());
builtins.Register(new PwdCommand());
builtins.Register(new CdCommand());
builtins.Register(new ExitCommand());
builtins.Register(new TypeCommand(builtins));
builtins.Register(new CompleteCommand(completeRegistry));
builtins.Register(new JobsCommand(jobManager));

var cmdDispatcher = new CommandDispatcher(builtins, jobManager);
var autoCompletion = new AutoCompletionHandler(builtins, completeRegistry, jobManager);
var shell = new Shell(cmdDispatcher, autoCompletion);
await shell.Run();
