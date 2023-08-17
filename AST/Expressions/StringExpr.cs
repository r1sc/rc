using LLVMSharp.Interop;
using Superpower;
using Superpower.Parsers;

namespace cccc.AST.Expressions;


public class StringExpr : ExprNode
{
    public required string Value { get; set; }

    public static TokenListParser<Tokens, ExprNode> StringParser =
        from str in Token.EqualTo(Tokens.String)
        select new StringExpr { Value = str.ToStringValue()[1..(str.Span.Length - 1)].Replace("\\n", "\n") } as ExprNode;

    public override LLVMValueRef Codegen(CodegenScope codegenScope, TypeRef wanted)
    {
        if(wanted is PointerTypeRef)
        {
            return codegenScope.Builder.BuildGlobalStringPtr(Value);
        }
        throw new Exception("Expected pointer target for string");
    }

    public override TypeRef InferType(CodegenScope codegenScope)
    {
        return new PointerTypeRef { Inner = new NumberTypeRef { NumBits = 8, Signed = false } };
    }
}
