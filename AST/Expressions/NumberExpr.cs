using Superpower;
using Superpower.Parsers;

namespace cccc.AST.Expressions;

public class NumberExpr : ExprNode
{
    public required int Value { get; set; }

    public static TokenListParser<Tokens, ExprNode> NumberParser =
        from num in Token.EqualTo(Tokens.Number)
        select new NumberExpr { Value = int.Parse(num.ToStringValue()) } as ExprNode;
}
