using Superpower;

namespace cccc.AST.Statements;

public abstract class TopLevelStatement : Statement
{
    static TokenListParser<Tokens, TopLevelStatement> TopLevelStatementParser =
        ExternDecl.ExternParser.Or(FuncDefNode.FuncDefParser);

    public static TokenListParser<Tokens, TopLevelStatement[]> FileParser =
        from statements in TopLevelStatementParser.Many().AtEnd()
        select statements;
}
