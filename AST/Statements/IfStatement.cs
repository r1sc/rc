using cccc.AST.Expressions;
using LLVMSharp.Interop;
using Superpower;
using Superpower.Parsers;

namespace cccc.AST.Statements;

public class IfStatement : Statement
{
    public required ExprNode Expression { get; set; }
    public required Scope Then { get; set; }
    public required Scope? Else { get; set; }

    public static TokenListParser<Tokens, Statement> Parser =
        from _if in Token.EqualToValue(Tokens.Identifier, "if")
        from lparen in Token.EqualTo(Tokens.LParen)
        from expression in Parse.Ref(() => ExprNode.ExprParser)
        from rparen in Token.EqualTo(Tokens.RParen)
        from then in Scope.ScopeParser
        from _else in (
            from _ in Token.EqualToValue(Tokens.Identifier, "else")
            from scope in Scope.ScopeParser
            select scope
        ).OptionalOrDefault()
        select new IfStatement
        {
            Expression = expression,
            Then = then,
            Else = _else
        } as Statement;

    public override void Codegen(CodegenScope codegenScope)
    {
        throw new NotImplementedException();
    }
}
