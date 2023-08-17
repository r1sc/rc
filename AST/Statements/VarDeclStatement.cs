using cccc.AST.Expressions;
using Superpower;
using Superpower.Parsers;

namespace cccc.AST.Statements;

public class VarDeclStatement : Statement
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
        select new VarDeclStatement
        {
            Identifier = typed_ident,
            Expression = expression
        } as Statement;

    public override void Codegen(CodegenScope codegenScope)
    {
        codegenScope.DefineVariable(Identifier.Name, Identifier.Type.GetTypeRef());
        if(Expression != null)
        {
            var variable = codegenScope.GetVariable(Identifier.Name);
            codegenScope.Builder.BuildStore(Expression.Codegen(codegenScope, variable.TypeRef), variable.Storage);
        }
    }
}
