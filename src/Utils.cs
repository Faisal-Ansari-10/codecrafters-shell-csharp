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

  public static string? FindExecutable(string command)
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

      if (!content.EndsWith('\n')) content += "\n";
      if (append) File.AppendAllText(path, content);
      else File.WriteAllText(path, content);
    }
    catch { }
  }
}