using cccc.AST.Statements;
using Superpower;

namespace cccc.AST;

public class AST
{
    public static TokenListParser<Tokens, TopLevelStatement[]> FileParser =
        from statements in TopLevelStatement.TopLevelStatementParser.Many().AtEnd()
        select statements;

}
