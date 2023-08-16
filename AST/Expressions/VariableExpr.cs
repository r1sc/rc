using Superpower;

namespace cccc.AST.Expressions;


public class VariableExpr : ExprNode
{
    public required string Name { get; set; }

    public static TokenListParser<Tokens, ExprNode> VariableParser =
        from name in Identifier.IdentifierParser
        select new VariableExpr { Name = name } as ExprNode;
}
