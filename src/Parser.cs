using System;
using System.Data;

namespace CodeCrafters.Shell.src;

public class Parser(List<Token> tokens)
{
  private readonly List<Token> _tokens = tokens;
  private int _pos = 0;

  public CommandLine Parse()
  {
    var line = new CommandLine();
    line.Pipeline.Add(ParseCommand());

    while (Match(TokenType.Pipe))
    {
      Advance();
      line.Pipeline.Add(ParseCommand());
    }

    if (Match(TokenType.Background)) { line.RunInBackground = true; Advance(); }
    Expect(TokenType.EOF);
    return line;
  }

  private Command ParseCommand()
  {
    var cmd = new Command
    {
      Name = ExpectWord()
    };

    while (!IsCommandEnd())
    {
      if (Match(TokenType.RedirectOut)) { Advance(); cmd.StdoutFile = ExpectWord(); }
      else if (Match(TokenType.RedirectAppend)) { Advance(); cmd.StdoutFile = ExpectWord(); cmd.AppendStdout = true; }
      else if (Match(TokenType.RedirectErr)) { Advance(); cmd.StderrFile = ExpectWord(); }
      else if (Match(TokenType.RedirectErrAppend)) { Advance(); cmd.StderrFile = ExpectWord(); cmd.AppendStderr = true; }
      else { cmd.Args.Add(ExpectWord()); }
    }

    return cmd;
  }

  private bool IsCommandEnd() =>
          Match(TokenType.Pipe) || Match(TokenType.EOF) || Match(TokenType.Semicolon) || Match(TokenType.Background);

  private bool Match(TokenType t) => _tokens[_pos].Type == t;
  private Token Advance() => _tokens[_pos++];

  private string ExpectWord()
  {
    if (_tokens[_pos].Type != TokenType.Word)
      throw new SyntaxErrorException($"expected word, got {_tokens[_pos].Type}");

    return _tokens[_pos++].Value;
  }

  private void Expect(TokenType t) { if (!Match(t)) throw new SyntaxErrorException($"unexpected token {_tokens[_pos].Type}"); }
}

