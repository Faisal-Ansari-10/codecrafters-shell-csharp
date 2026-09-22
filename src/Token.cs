using System;

namespace CodeCrafters.Shell.src;

public enum TokenType
{
  Word,
  Pipe,
  RedirectOut,
  RedirectAppend,
  RedirectErr,
  RedirectErrAppend,
  RedirectIn,
  Semicolon,
  EOF
}

public record Token(TokenType Type, string Value);
