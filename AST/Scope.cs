using cccc.AST.Statements;
using Superpower;
using Superpower.Parsers;

namespace cccc.AST;

public class Scope
{
    public required IEnumerable<Statement> Statements { get; set; }

    public static TokenListParser<Tokens, Scope> ScopeParser =
        from lbrace in Token.EqualTo(Tokens.LBrace)
        from statements in Parse.Ref(() => Statement.StatementParser!).Many()
        from rbrace in Token.EqualTo(Tokens.RBrace)
        select new Scope { Statements = statements };

    public void Codegen(CodegenScope codegenScope)
    {
        foreach (var statement in Statements)
        {
            statement.Codegen(codegenScope);
        }
    }
}
