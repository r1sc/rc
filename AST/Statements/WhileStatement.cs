using cccc.AST.Expressions;
using Superpower;
using Superpower.Parsers;

namespace cccc.AST.Statements;

public class WhileStatement : Statement
{
    public required ExprNode Expression { get; set; }
    public required Scope Scope { get; set; }

    public static TokenListParser<Tokens, Statement> Parser =
        from _while in Token.EqualToValue(Tokens.Identifier, "while")
        from lparen in Token.EqualTo(Tokens.LParen)
        from expression in Parse.Ref(() => ExprNode.ExprParser)
        from rparen in Token.EqualTo(Tokens.RParen)
        from scope in Scope.ScopeParser        
        select new WhileStatement
        {
            Expression = expression,
            Scope = scope
        } as Statement;

    public override void Codegen(CodegenScope codegenScope)
    {
        throw new NotImplementedException();
    }
}
