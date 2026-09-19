using System.Text;

class Shell
{
  private readonly CommandHandler _commandHandler;
  private readonly AutoCompletionHandler _autoCompletion;


  public Shell()
  {
    var lexer = new Lexer();
    _commandHandler = new CommandHandler(lexer);
    _autoCompletion = new AutoCompletionHandler();

  }

  public void Run()
  {
    while (true)
    {
      Console.Write("$ ");
      string? command = ReadCommand();
      if (string.IsNullOrEmpty(command)) break;

      _commandHandler.Execute(command);
    }
  }

  private string? ReadCommand()
  {
    if (Console.IsInputRedirected) return Console.ReadLine();

    StringBuilder command = new();
    while (true)
    {
      ConsoleKeyInfo keyInfo = Console.ReadKey(true);
      var key = keyInfo.Key;

      if (key == ConsoleKey.Enter)
      {
        Console.WriteLine();
        break;
      }
      else if (key == ConsoleKey.Backspace)
      {
        if (command.Length == 0) continue;
        command.Remove(command.Length - 1, 1);
        Console.Write("\b \b");
      }
      else if (key == ConsoleKey.Tab)
      {
        var current = command.ToString();
        int index = current.LastIndexOf(' ') + 1;

        var suggestions = _autoCompletion.GetSuggestions(current, index);
        if (suggestions.Length == 0 || string.IsNullOrEmpty(suggestions[0])) continue;

        string suggestion = suggestions[0];
        int oldLen = current.Length - index;
        command.Remove(index, oldLen);
        command.Append(suggestion);
        Console.Write("\r\x1b[K$ " + command);
      }
      else if (!char.IsControl(keyInfo.KeyChar))
      {
        command.Append(keyInfo.KeyChar);
        Console.Write(keyInfo.KeyChar);
      }
    }

    return command.ToString();
  }

}