using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;

class Program
{
    private static readonly string[] BuiltinCommands = ["echo", "exit", "type", "pwd", "cd"];
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
            var (cleanedCommand, outputFileName, errorFileName) = ExtractOutputRedirect(command[5..]);
            var output = HandleEcho(cleanedCommand);
            CreateFile(outputFileName);
            CreateFile(errorFileName);
            PrintOutput(output, outputFileName);
        }
        else if (command.StartsWith("type "))
        {
            var (cleanedCommand, outputFileName, errorFileName) = ExtractOutputRedirect(command[5..]);
            var output = HandleType(cleanedCommand);
            CreateFile(outputFileName);
            CreateFile(errorFileName);

            PrintOutput(output, outputFileName);

        }
        else if (command == "pwd")
        {
            var (_, outputFileName, errorFileName) = ExtractOutputRedirect(command[3..]);
            var output = Directory.GetCurrentDirectory();
            CreateFile(outputFileName);
            CreateFile(errorFileName);

            PrintOutput(output, outputFileName);
        }
        else if (command.StartsWith("cd "))
        {
            ChangeDirectory(command[3..]);
        }
        else
        {
            var (cleanedCommand, outputFileName, errorFileName) = ExtractOutputRedirect(command);
            var (output, error) = ExecuteExternalCommand(cleanedCommand);

            CreateFile(outputFileName);
            CreateFile(errorFileName);

            if (output.Length > 0)
                PrintOutput(output, outputFileName);
            if (!string.IsNullOrEmpty(error))
                PrintError(error, errorFileName);
        }
    }

    private static string HandleEcho(string args)
    {
        StringBuilder output = new();

        for (int i = 0; i < args.Length;)
        {
            if (args[i] == '\'')
            {
                while (++i < args.Length && args[i] != '\'')
                {
                    output.Append(args[i]);
                }
                i++;
            }
            else if (args[i] == '"')
            {
                while (++i < args.Length && args[i] != '"')
                {
                    if (args[i] == '\\') i++;
                    output.Append(args[i]);
                }
                i++;
            }
            else if (args[i] == '\\')
            {
                if (++i < args.Length) output.Append(args[i++]);
            }
            else if (args[i] == ' ')
            {
                output.Append(' ');
                while (++i < args.Length && args[i] == ' ') ;
            }
            else
            {

                while (i < args.Length && !(new[] { ' ', '\'', '"', '\\', '1' }.Contains(args[i])))
                {
                    output.Append(args[i++]);
                }

                if (i < args.Length && args[i] == '1')
                {
                    if (i + 1 < args.Length && args[i + 1] == '>') i += 2;
                    else output.Append(args[i++]);
                }
            }
        }

        return output.ToString().Trim();
    }


    private static string HandleType(string argCommand)
    {
        string output = $"{argCommand}: not found";
        if (BuiltinCommands.Contains(argCommand))
        {
            output = $"{argCommand} is a shell builtin";
            return output;
        }

        var executablePath = FindExecutable(argCommand);
        if (executablePath is not null)
        {
            output = $"{argCommand} is {executablePath}";
        }

        return output;
    }

    private static (string output, string? error) ExecuteExternalCommand(string command)
    {
        var tokens = ParseCommandStrings(command);

        if (tokens.Count == 0)
            return (string.Empty, null);

        var exeName = tokens[0];
        var executablePath = FindExecutable(exeName);
        if (executablePath is null)
            return (string.Empty, $"{command}: not found");

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

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (_, e) => { if (e.Data is not null) outputBuilder.AppendLine(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data is not null) errorBuilder.AppendLine(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        var output = outputBuilder.ToString();
        if (output.EndsWith(Environment.NewLine))
            output = output[..^Environment.NewLine.Length];
        var error = errorBuilder.Length > 0 ? errorBuilder.ToString() : null;
        if (!string.IsNullOrEmpty(error) && error.EndsWith(Environment.NewLine))
            error = error[..^Environment.NewLine.Length];
        return (output, error);
    }
    private static void ChangeDirectory(string directory)
    {
        if (directory == "~")
        {
            directory = Environment.GetEnvironmentVariable("HOME") ?? directory;
        }

        if (!Directory.Exists(directory))
        {
            Console.WriteLine($"cd: {directory}: No such file or directory");
            return;
        }
        try
        {
            Directory.SetCurrentDirectory(directory);
        }
        catch { }
    }

    private static List<string> ParseCommandStrings(string command)
    {
        var tokens = new List<string>();
        if (string.IsNullOrWhiteSpace(command))
            return tokens;

        var current = new StringBuilder();
        int i = 0;

        while (i < command.Length)
        {
            char c = command[i];

            if (c == '\'')
            {
                i++;
                while (i < command.Length && command[i] != '\'')
                    current.Append(command[i++]);
                i++;
            }
            else if (c == '"')
            {
                i++;
                while (i < command.Length && command[i] != '"')
                {
                    if (command[i] == '\\' && i + 1 < command.Length && "\\$`\"".IndexOf(command[i + 1]) >= 0)
                    {
                        current.Append(command[i + 1]);
                        i += 2;
                    }
                    else
                    {
                        current.Append(command[i++]);
                    }
                }
                i++;
            }
            else if (c == '\\')
            {
                i++;
                if (i < command.Length) current.Append(command[i++]);
            }
            else if (char.IsWhiteSpace(c))
            {
                if (current.Length > 0)
                {
                    tokens.Add(current.ToString());
                    current.Clear();
                }
                while (++i < command.Length && char.IsWhiteSpace(command[i])) ;
            }
            else
            {
                while (i < command.Length && !char.IsWhiteSpace(command[i]) &&
                       command[i] != '\'' && command[i] != '"' && command[i] != '\\')
                {
                    current.Append(command[i++]);
                }
            }
        }

        if (current.Length > 0)
            tokens.Add(current.ToString());

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

    private static (string command, string? outputFileName, string? errorFileName) ExtractOutputRedirect(string command)
    {
        string cleaned = command;
        string? outputFileName = null;
        string? errorFileName = null;
        var idx = command.IndexOf("1> ", StringComparison.Ordinal);
        if (idx > 0)
        {
            cleaned = command[..idx].TrimEnd();
            outputFileName = command[(idx + 2)..].Trim();

        }
        else if ((idx = command.IndexOf("2> ", StringComparison.OrdinalIgnoreCase)) > 0)
        {
            cleaned = command[..idx].TrimEnd();
            errorFileName = command[(idx + 2)..].Trim();

        }
        else if ((idx = command.IndexOf("> ", StringComparison.OrdinalIgnoreCase)) > 0)
        {
            cleaned = command[..idx].TrimEnd();
            outputFileName = command[(idx + 2)..].Trim();

        }


        return (cleaned, outputFileName, errorFileName);
    }
    private static void PrintOutput(string output, string? fileName = null)
    {
        Print(output, fileName);
    }

    private static void PrintError(string error, string? fileName = null)
    {
        Print(error, fileName);
    }

    private static void Print(string text, string? fileName = null)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            Console.WriteLine(text);
            return;
        }

        File.WriteAllText(fileName, text + Environment.NewLine);
    }

    private static void CreateFile(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return;
        try
        {
            var directory = Path.GetDirectoryName(fileName);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(fileName, "");
        }
        catch { }
    }
}