using cccc.AST.Expressions;
using Superpower;
using Superpower.Parsers;

namespace cccc.AST.Statements;

public class AssignStatement : Statement
{
    public required string Name { get; set; }
    public required ExprNode Expression { get; set; }

    public static TokenListParser<Tokens, Statement> Parser =
        from name in Identifier.IdentifierParser
        from eq in Token.EqualTo(Tokens.Eq)
        from expression in Parse.Ref(() => ExprNode.ExprParser)
        select new AssignStatement
        {
            Name = name,
            Expression = expression
        } as Statement;

    public override void Codegen(CodegenScope codegenScope)
    {
        var variable = codegenScope.GetVariable(Name);
        codegenScope.Builder.BuildStore(Expression.Codegen(codegenScope, variable.TypeRef), variable.Storage);
    }
}
