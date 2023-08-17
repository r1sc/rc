using LLVMSharp.Interop;
using Superpower;
using Superpower.Parsers;
using System.Numerics;

namespace cccc.AST.Expressions;

public class NegatedExpr : ExprNode
{
    public required ExprNode Expression { get; set; }

    public static TokenListParser<Tokens, ExprNode> Parser =
        from sign in Token.EqualTo(Tokens.Minus)
        from factor in Factor
        select new NegatedExpr { Expression = factor } as ExprNode;

    public override LLVMValueRef Codegen(CodegenScope codegenScope, TypeRef wanted)
    {
        return codegenScope.Builder.BuildNeg(Expression.Codegen(codegenScope, wanted));
    }

    public override TypeRef InferType(CodegenScope codegenScope)
    {
        var inferred = Expression.InferType(codegenScope);
        if (inferred is NumberTypeRef num)
        {
            return new NumberTypeRef { NumBits = num.NumBits, Signed = true };
        }
        throw new Exception("Cannot negate non-numeric type");
    }
}
