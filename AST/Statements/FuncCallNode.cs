using cccc.AST.Expressions;
using Superpower;

namespace cccc.AST.Statements;

public class FuncCallNode : Statement
{
    public required FuncCallExpr Expression { get; set; }

    public static readonly TokenListParser<Tokens, Statement> FuncCallParser =
        from funccall in Parse.Ref(() => FuncCallExpr.FuncCallExprParser)
        select new FuncCallNode { Expression = (funccall as FuncCallExpr)! } as Statement;

    public override void Codegen(CodegenScope codegenScope)
    {
        Expression.Codegen(codegenScope, new VoidTypeRef());
    }
}
