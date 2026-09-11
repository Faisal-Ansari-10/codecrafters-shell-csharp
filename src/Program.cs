using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;

class Program
{
    private static readonly string[] BuiltinCommands = ["echo", "exit", "type"];
    private static readonly string[] WindowsExtensions =
        [".exe", ".bat", ".cmd", ".com", ".ps1", ".msi"];

    static void Main()
    {
        while (true)
        {
            Console.Write("$ ");
            var input = Console.ReadLine();
            if (input is null || input == "exit")
                break;

            Run(input);
        }
    }

    private static void Run(string command)
    {
        if (command.StartsWith("echo "))
        {
            Console.WriteLine(command[5..]);
        }
        else if (command.StartsWith("type "))
        {
            HandleType(command[5..]);
        }
        else
        {
            ExecuteExternalCommand(command);
        }
    }

    private static void HandleType(string argCommand)
    {
        if (BuiltinCommands.Contains(argCommand))
        {
            Console.WriteLine($"{argCommand} is a shell builtin");
            return;
        }

        var executablePath = FindExecutable(argCommand);
        if (executablePath is not null)
        {
            Console.WriteLine($"{argCommand} is {executablePath}");
        }
        else
        {
            Console.WriteLine($"{argCommand}: not found");
        }
    }

    private static void ExecuteExternalCommand(string command)
    {
        var tokens = ParseCommandStrings(command);
        if (tokens.Count == 0)
            return;

        var exeName = tokens[0];
        var executablePath = FindExecutable(exeName);
        if (executablePath is null)
        {
            Console.WriteLine($"{command}: not found");
            return;
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = exeName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        for (int i = 1; i < tokens.Count; i++)
        {
            startInfo.ArgumentList.Add(tokens[i]);
        }

        using var process = new Process { StartInfo = startInfo };

        var output = new StringBuilder();
        var error = new StringBuilder();

        process.OutputDataReceived += (_, e) => { if (e.Data is not null) output.AppendLine(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data is not null) error.AppendLine(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        if (output.Length > 0)
            Console.Write(output.ToString());

        if (error.Length > 0)
            Console.Write(error.ToString());
    }

    private static List<string> ParseCommandStrings(string command)
    {
        var tokens = new List<string>();
        if (string.IsNullOrWhiteSpace(command))
            return tokens;

        var currentToken = new StringBuilder();
        bool inDoubleQuotes = false;
        bool inSingleQuotes = false;
        bool hasToken = false;

        for (int i = 0; i < command.Length; i++)
        {
            char c = command[i];

            if (c == '\\' && !inSingleQuotes)
            {
                if (inDoubleQuotes)
                {
                    if (i + 1 < command.Length && "\\$`\"".IndexOf(command[i + 1]) >= 0)
                    {
                        currentToken.Append(command[i + 1]);
                        i++;
                    }
                    else
                    {
                        currentToken.Append(c);
                    }
                }
                else if (i + 1 < command.Length)
                {
                    currentToken.Append(command[i + 1]);
                    i++;
                }

                hasToken = true;
                continue;
            }

            if (c == '"' && !inSingleQuotes)
            {
                inDoubleQuotes = !inDoubleQuotes;
                hasToken = true;
                continue;
            }

            if (c == '\'' && !inDoubleQuotes)
            {
                inSingleQuotes = !inSingleQuotes;
                hasToken = true;
                continue;
            }

            if (char.IsWhiteSpace(c) && !inDoubleQuotes && !inSingleQuotes)
            {
                if (hasToken)
                {
                    tokens.Add(currentToken.ToString());
                    currentToken.Clear();
                    hasToken = false;
                }
                continue;
            }

            currentToken.Append(c);
            hasToken = true;
        }

        if (hasToken)
        {
            tokens.Add(currentToken.ToString());
        }

        return tokens;
    }

    private static string? FindExecutable(string command)
    {
        var pathVariable = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathVariable))
            return null;

        var extensions = OperatingSystem.IsWindows() ? WindowsExtensions : [""];
        var directories = pathVariable.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

        foreach (var directory in directories)
        {
            foreach (var extension in extensions)
            {
                var fileName = command.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
                    ? command
                    : command + extension;

                var filePath = Path.Combine(directory.Trim(), fileName);
                if (!Path.Exists(filePath))
                    continue;

                if (OperatingSystem.IsWindows() || IsExecutable(filePath))
                    return filePath;
            }
        }

        return null;
    }

    [UnsupportedOSPlatform("windows")]
    private static bool IsExecutable(string filePath)
    {
        const UnixFileMode executeMask =
            UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute;

        try
        {
            return (File.GetUnixFileMode(filePath) & executeMask) != 0;
        }
        catch
        {
            return false;
        }
    }
}