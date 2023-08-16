using Superpower;
using Superpower.Parsers;

namespace cccc.AST.Expressions;


public class FuncCallExpr : ExprNode
{
    public required string Name { get; set; }
    public required IEnumerable<ExprNode> Arguments { get; set; }

    public static readonly TokenListParser<Tokens, ExprNode> FuncCallExprParser =
        from name in Identifier.IdentifierParser
        from lparen in Token.EqualTo(Tokens.LParen)
        from args in Parse.Ref(() => ExprNode.ExprParser.Or(StringExpr.StringParser)).ManyDelimitedBy(Token.EqualTo(Tokens.Comma))
        from rparen in Token.EqualTo(Tokens.RParen)
        select new FuncCallExpr
        {
            Name = name,
            Arguments = args
        } as ExprNode;
}
