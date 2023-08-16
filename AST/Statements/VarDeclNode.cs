using cccc.AST.Expressions;
using Superpower;
using Superpower.Parsers;

namespace cccc.AST.Statements;

public class VarDecl : Statement
{
    public required TypedIdentifier Identifier { get; set; }
    public required ExprNode? Expression { get; set; }

    public static TokenListParser<Tokens, Statement> VarDeclParser =
        from typed_ident in TypedIdentifier.TypedIdentifierParser
        from expression in (
            from eq in Token.EqualTo(Tokens.Eq)
            from expr in Parse.Ref(() => StringExpr.StringParser.Or(ExprNode.ExprParser))
            select expr
        ).OptionalOrDefault()
        select new VarDecl
        {
            Identifier = typed_ident,
            Expression = expression
        } as Statement;
}
