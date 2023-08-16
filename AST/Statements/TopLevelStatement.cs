using Superpower;

namespace cccc.AST.Statements;

public abstract class TopLevelStatement
{
    public static TokenListParser<Tokens, TopLevelStatement> TopLevelStatementParser =
        ExternDecl.ExternParser.Or(FuncDefNode.FuncDefParser);
}
