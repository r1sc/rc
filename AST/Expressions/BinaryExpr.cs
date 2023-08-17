using LLVMSharp.Interop;

namespace cccc.AST.Expressions;

public enum Operator
{
    Plus,
    Minus,
    Mul,
    Div,
    LessThen,
    GreaterThen
}

public class BinaryExpr : ExprNode
{
    public required ExprNode Left { get; set; }
    public required Operator Op { get; set; }
    public required ExprNode Right { get; set; }

    public override LLVMValueRef Codegen(CodegenScope codegenScope, TypeRef wanted)
    {
        if (wanted is NumberTypeRef numType)
        {
            var left = Left.Codegen(codegenScope, numType);
            var right = Right.Codegen(codegenScope, numType);
            return Op switch
            {
                Operator.Plus => codegenScope.Builder.BuildAdd(left, right),
                Operator.Minus => codegenScope.Builder.BuildSub(left, right),
                Operator.Mul => codegenScope.Builder.BuildMul(left, right),
                Operator.Div => numType.Signed ? codegenScope.Builder.BuildSDiv(left, right) : codegenScope.Builder.BuildUDiv(left, right),
                Operator.LessThen => codegenScope.Builder.BuildICmp(numType.Signed ? LLVMIntPredicate.LLVMIntSLT : LLVMIntPredicate.LLVMIntULT, left, right),
                Operator.GreaterThen => codegenScope.Builder.BuildICmp(numType.Signed ? LLVMIntPredicate.LLVMIntSGT : LLVMIntPredicate.LLVMIntUGT, left, right),
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
        throw new Exception("Binary operations only available on numeric types");
    }

    public override TypeRef InferType(CodegenScope codegenScope)
    {
        return Left.InferType(codegenScope);
    }
}
