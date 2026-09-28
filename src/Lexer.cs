using System.Text;
using CodeCrafters.Shell.src;

class Lexer(string input)
{
  private readonly string _input = input;
  private int _pos = 0;

  public List<Token> Tokenize()
  {
    var tokens = new List<Token>();

    while (_pos < _input.Length)
    {
      SkipWhiteSpace();
      if (_pos >= _input.Length) break;

      char c = _input[_pos];
      if (c == '>') { tokens.Add(ReadRedirect(tokens)); }
      else if (c == '|') { tokens.Add(ReadOperator()); }
      else if (c == '&') { tokens.Add(ReadAmpersand()); }
      else { tokens.Add(ReadWord()); }
    }
    tokens.Add(new Token(TokenType.EOF, ""));
    return tokens;
  }

  private Token ReadWord()
  {
    var sb = new StringBuilder();
    while (_pos < _input.Length && !IsUnquotedSeparator(_input[_pos]))
    {
      char c = _input[_pos];
      if (c == '\'') { ReadSingleQuoted(sb); }
      else if (c == '"') { ReadDoubleQuoted(sb); }
      else if (c == '\\') { ReadEscape(sb); }
      else { sb.Append(c); _pos++; }
    }

    return new Token(Type: TokenType.Word, sb.ToString());
  }

  private void ReadSingleQuoted(StringBuilder sb)
  {
    _pos++;
    while (_pos < _input.Length && _input[_pos] != '\'')
    {
      sb.Append(_input[_pos]);
      _pos++;
    }
    _pos++;
  }

  private void ReadDoubleQuoted(StringBuilder sb)
  {
    _pos++;
    while (_pos < _input.Length && _input[_pos] != '"')
    {
      char c = _input[_pos];
      if (c == '\\') { ReadEscape(sb); }
      else { sb.Append(c); _pos++; }
    }
    _pos++;
  }

  private void ReadEscape(StringBuilder sb)
  {
    _pos++;

    if (_pos < _input.Length) sb.Append(_input[_pos]);
    _pos++;
  }

  private Token ReadOperator()
  {
    var token = new Token(TokenType.Pipe, "|");
    _pos++;
    return token;
  }
  private Token ReadAmpersand()
  {
    var token = new Token(TokenType.Background, "&");
    _pos++;
    return token;
  }
  private static bool IsUnquotedSeparator(char c) => char.IsWhiteSpace(c) || "|<>&;".Contains(c);

  private void SkipWhiteSpace()
  {
    while (_pos < _input.Length && char.IsWhiteSpace(_input[_pos])) { _pos++; }
  }

  private Token ReadRedirect(List<Token> tokens)
  {
    var sb = new StringBuilder();

    if (_pos > 0 && (_input[_pos - 1] == '1' || _input[_pos - 1] == '2')
      && tokens.Count > 0
      && tokens[^1].Type == TokenType.Word
      && tokens[^1].Value == _input[_pos - 1].ToString())
    {
      sb.Append(tokens[^1].Value);
      tokens.RemoveAt(tokens.Count - 1);
    }

    sb.Append(_input[_pos]);
    _pos++;

    if (_pos < _input.Length && _input[_pos] == '>')
    {
      sb.Append(_input[_pos]);
      _pos++;
    }
    string value = sb.ToString();

    if (value == ">" || value == "1>") return new Token(TokenType.RedirectOut, value);
    if (value == ">>" || value == "1>>") return new Token(TokenType.RedirectAppend, value);
    if (value == "2>") return new Token(TokenType.RedirectErr, value);
    return new Token(TokenType.RedirectErrAppend, value);
  }

}