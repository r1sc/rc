using Superpower;
using Superpower.Parsers;

namespace cccc.AST;

public class Identifier
{
    public static TokenListParser<Tokens, string> IdentifierParser =
        from identifier in Token.EqualTo(Tokens.Identifier)
        select identifier.ToStringValue();
}
