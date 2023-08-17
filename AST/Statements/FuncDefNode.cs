using Superpower;
using Superpower.Parsers;

namespace cccc.AST.Statements;

public class FuncDefNode : TopLevelStatement
{
    public required TypedIdentifier Ident { get; set; }
    public required IEnumerable<TypedIdentifier> Parameters { get; set; }
    public required Scope Scope { get; set; }

    public static TokenListParser<Tokens, TopLevelStatement> FuncDefParser =
            from typed_ident in TypedIdentifier.TypedIdentifierParser
            from lparen in Token.EqualTo(Tokens.LParen)
            from parameters in TypedIdentifier.TypedIdentifierParser.ManyDelimitedBy(Token.EqualTo(Tokens.Comma))
            from rparen in Token.EqualTo(Tokens.RParen)
            from scope in Parse.Ref(() => Scope.ScopeParser)
            select new FuncDefNode
            {
                Ident = typed_ident,
                Parameters = parameters,
                Scope = scope
            } as TopLevelStatement;

    public override void Codegen(CodegenScope codegenScope)
    {
        var funcRef = codegenScope.DefineFunction(Ident.Name, Ident.Type.GetTypeRef(), Parameters, false);

        var funcBlock = funcRef.LLVMFuncValue.AppendBasicBlock("entry");
        var builder = codegenScope.Module.Context.CreateBuilder();
        builder.Position(funcBlock, funcBlock.FirstInstruction);

        Scope.Codegen(new CodegenScope(codegenScope, builder));

        if(funcRef.ReturnType is VoidTypeRef)
        {
            builder.BuildRetVoid();
        } else
        {
            throw new Exception("Return statement not supported yet");
        }
    }
}
