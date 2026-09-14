using System.Text;

class Lexer
{
  private List<string> _tokens = [];
  public IReadOnlyList<string> Tokens => _tokens.AsReadOnly();
  public void Parse(string input)
  {
    input = input.Trim();
    _tokens = [];

    bool inSingleQuotes = false;
    bool inDoubleQuotes = false;
    bool escapeChar = false;

    StringBuilder currentToken = new();

    for (int i = 0; i < input.Length; i++)
    {
      char c = input[i];

      if (c == '\'')
      {
        if (inSingleQuotes) { inSingleQuotes = false; }
        else if (inDoubleQuotes) { currentToken.Append(c); }
        else { inSingleQuotes = true; }
      }
      else if (c == '"')
      {
        if (inDoubleQuotes)
        {
          if (escapeChar)
          {
            currentToken.Append(c);
            escapeChar = false;
          }
          else
          {
            inDoubleQuotes = false;
          }
        }
        else if (inSingleQuotes)
        {
          currentToken.Append(c);
        }
      }
      else if (c == '\\')
      {
        if (inSingleQuotes)
        {
          currentToken.Append(c);
        }
        else
        {
          if (escapeChar)
          {
            currentToken.Append(c);
            escapeChar = false;
          }
          else
          {
            escapeChar = true;
          }
        }
      }
      else if (c == ' ')
      {
        if (!(inSingleQuotes || inDoubleQuotes || escapeChar) && currentToken.Length > 0)
        {
          _tokens.Add(currentToken.ToString().Trim());
          currentToken.Clear();
        }
        else if (inSingleQuotes || inDoubleQuotes || escapeChar)
        {
          currentToken.Append(c);
        }

        if (escapeChar) escapeChar = false;

      }
      else
      {
        currentToken.Append(c);
      }
    }

    if (currentToken.Length > 0)
    {
      _tokens.Add(currentToken.ToString().Trim());
    }
  }
}