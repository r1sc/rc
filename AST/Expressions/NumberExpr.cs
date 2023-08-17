using LLVMSharp.Interop;
using Superpower;
using Superpower.Parsers;

namespace cccc.AST.Expressions;


public class NumberExpr : ExprNode
{
    public required int Value { get; set; }

    public static TokenListParser<Tokens, ExprNode> NumberParser =
        from num in Token.EqualTo(Tokens.Number)
        select new NumberExpr { Value = int.Parse(num.ToStringValue()) } as ExprNode;

    public override LLVMValueRef Codegen(CodegenScope codegenScope, TypeRef wanted)
    {
        if(wanted is NumberTypeRef numType)
        {
            return LLVMValueRef.CreateConstInt(numType.GetLLVMType(), (ulong)Value, numType.Signed);
        }
        throw new Exception("Cannot coalesce number with non-number type");
    }

    public override TypeRef InferType(CodegenScope codegenScope)
    {
        return new NumberTypeRef { NumBits = 32, Signed = true };
    }
}
