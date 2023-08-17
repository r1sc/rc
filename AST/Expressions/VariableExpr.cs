using LLVMSharp.Interop;
using Superpower;
using Superpower.Parsers;

namespace cccc.AST.Expressions;

public class VariableExpr : ExprNode
{
    public enum IndirectionKind
    {
        None,
        AddressOf,
        Dereference
    }

    public required IndirectionKind Indirection { get; set; }
    public required string Name { get; set; }

    public static TokenListParser<Tokens, ExprNode> VariableParser =
        from indirection in Token.EqualTo(Tokens.Ampersand).Or(Token.EqualTo(Tokens.Mul)).Optional()
        from name in Identifier.IdentifierParser
        select new VariableExpr { Indirection = indirection.HasValue ? indirection.Value.Kind == Tokens.Ampersand ? IndirectionKind.AddressOf : IndirectionKind.Dereference : IndirectionKind.None, Name = name } as ExprNode;

    public override LLVMValueRef Codegen(CodegenScope codegenScope, TypeRef wanted)
    {
        var variable = codegenScope.GetVariable(Name);
        return Indirection switch
        {
            IndirectionKind.None => codegenScope.Builder.BuildLoad2(variable.TypeRef.GetLLVMType(), variable.Storage),
            IndirectionKind.AddressOf => variable.Storage,
            IndirectionKind.Dereference => codegenScope.Builder.BuildLoad2(variable.TypeRef.GetLLVMType(), codegenScope.Builder.BuildLoad2(variable.TypeRef.GetLLVMType(), variable.Storage)),
            _ => throw new ArgumentOutOfRangeException(),
        };
    }

    public override TypeRef InferType(CodegenScope codegenScope)
    {
        var variable = codegenScope.GetVariable(Name);
        return variable.TypeRef;
    }
}
