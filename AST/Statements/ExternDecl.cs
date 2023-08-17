using LLVMSharp.Interop;
using Superpower;
using Superpower.Parsers;

namespace cccc.AST.Statements;

public class ExternDecl : TopLevelStatement
{
    public required TypedIdentifier Ident { get; set; }
    public required IEnumerable<TypedIdentifier> Parameters { get; set; }
    public required bool IsVarArg { get; set; }

    public static TokenListParser<Tokens, TopLevelStatement> ExternParser =
            from _extern in Token.EqualToValue(Tokens.Identifier, "extern")
            from typed_ident in TypedIdentifier.TypedIdentifierParser
            from lparen in Token.EqualTo(Tokens.LParen)
            from parameters in TypedIdentifier.TypedIdentifierParser.ManyDelimitedBy(Token.EqualTo(Tokens.Comma))
            from rparen in Token.EqualTo(Tokens.RParen)
            from vararg in Token.EqualTo(Tokens.Vararg).Optional()
            from semi in Token.EqualTo(Tokens.Semi)
            select new ExternDecl
            {
                Ident = typed_ident,
                Parameters = parameters,
                IsVarArg = vararg.HasValue
            } as TopLevelStatement;

    public override void Codegen(CodegenScope codegenScope)
    {
        codegenScope.DefineFunction(Ident.Name, Ident.Type.GetTypeRef(), Parameters, IsVarArg);
    }
}
