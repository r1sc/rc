using Superpower;
using Superpower.Parsers;

namespace cccc.AST.Statements;

public abstract class Statement
{
    public static TokenListParser<Tokens, Statement> StatementParser =
        from statement in FuncCallNode.FuncCallParser.Try().Or(VarDecl.VarDeclParser)
        from semi in Token.EqualTo(Tokens.Semi)
        select statement;
}
