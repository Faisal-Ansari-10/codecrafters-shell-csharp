using System.Runtime.Versioning;

static class Utils
{
  private static readonly string[] WindowsExtensions =
        [".exe", ".bat", ".cmd", ".com", ".ps1", ".msi"];

  public static string? GetDirectory(string path)
  {
    try
    {
      return Path.GetDirectoryName(path);
    }
    catch { return null; }
  }

  public static bool CreateDirectory(string directory)
  {
    try
    {
      if (!Directory.Exists(directory))
        Directory.CreateDirectory(directory);
      return true;
    }
    catch
    {
      return false;
    }
  }

  public static string[] FindExecutable(string command)
  {
    var pathVariable = Environment.GetEnvironmentVariable("PATH");
    if (string.IsNullOrEmpty(pathVariable))
      return [];

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
          return [filePath];
      }
    }

    return [];
  }

  public static string[] FindExecutables(string prefix)
  {
    var pathVariable = Environment.GetEnvironmentVariable("PATH");
    if (string.IsNullOrEmpty(pathVariable))
      return [];

    var extensions = OperatingSystem.IsWindows() ? WindowsExtensions : [""];
    var directories = pathVariable.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);

    var results = new List<string>();

    foreach (var directory in directories)
    {
      if (!Directory.Exists(directory))
        continue;

      var matches = Directory.GetFiles(directory)
          .Select(Path.GetFileName)
          .Where(fileName => fileName is not null &&
              extensions.Any(ext =>
                  fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase) &&
                  fileName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
          .Where(fileName => OperatingSystem.IsWindows() || IsExecutable(Path.Combine(directory, fileName!)))
          .Select(fileName => Path.GetFileNameWithoutExtension(fileName)!);

      results.AddRange(matches!);
    }

    return [.. results.Distinct(StringComparer.OrdinalIgnoreCase)];
  }

  [UnsupportedOSPlatform("windows")]
  public static bool IsExecutable(string filePath)
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

  public static void WriteToFile(string path, string content, bool append = false)
  {
    try
    {
      var directory = GetDirectory(path);
      if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

      if (!string.IsNullOrEmpty(content) && !content.EndsWith('\n')) content += "\n";
      if (append) File.AppendAllText(path, content);
      else File.WriteAllText(path, content);
    }
    catch { }
  }
}