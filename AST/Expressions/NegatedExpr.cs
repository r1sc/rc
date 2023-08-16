namespace cccc.AST.Expressions;

public class NegatedExpr : ExprNode
{
    public required ExprNode Expression { get; set; }
}
