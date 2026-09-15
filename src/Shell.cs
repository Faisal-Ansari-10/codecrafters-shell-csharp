class Shell
{
  private readonly CommandHandler _commandHandler;

  public Shell()
  {
    var lexer = new Lexer();
    _commandHandler = new CommandHandler(lexer);
  }

  public void Run()
  {
    while (true)
    {
      _commandHandler.Read();

      if (_commandHandler.Input is null) break;

      _commandHandler.Execute();
    }
  }
}