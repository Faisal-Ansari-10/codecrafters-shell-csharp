using System;

class Program
{
    private static readonly string[] Builtincommands = ["echo", "exit", "type"];
    private static readonly bool IsWindows = OperatingSystem.IsWindows();
    private static readonly string[] WindowsExtensions = [".exe", ".bat", ".cmd", ".com", ".ps1", ".msi"];

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
            Console.WriteLine($"{command}: not found");
        }
    }

    private static void HandleType(string argCommand)
    {
        if (Builtincommands.Contains(argCommand))
        {
            Console.WriteLine($"{argCommand} is a shell builtin");
            return;
        }

        var executablePath = FindExecutablePath(argCommand);
        if (executablePath is not null)
        {
            Console.WriteLine($"{argCommand} is {executablePath}");
        }
        else
        {
            Console.WriteLine($"{argCommand}: not found");
        }
    }

    private static string? FindExecutablePath(string command)
    {
        var pathVariable = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathVariable)) return null;

        var extensions = IsWindows ? WindowsExtensions : [""];
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

                if (IsExecutable(filePath))
                    return filePath;
            }
        }

        return null;
    }

    private static bool IsExecutable(string filePath)
    {
        const UnixFileMode executeMask = UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute;
        try
        {
            return IsWindows || (File.GetUnixFileMode(filePath) & executeMask) != 0;
        }
        catch
        {
            return false;
        }
    }
}
