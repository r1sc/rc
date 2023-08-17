using LLVMSharp.Interop;
using Superpower;
using Superpower.Parsers;

namespace cccc.AST.Expressions;


public class FuncCallExpr : ExprNode
{
    public required string Name { get; set; }
    public required IEnumerable<ExprNode> Arguments { get; set; }

    public static readonly TokenListParser<Tokens, ExprNode> FuncCallExprParser =
        from name in Identifier.IdentifierParser
        from lparen in Token.EqualTo(Tokens.LParen)
        from args in Parse.Ref(() => ExprNode.ExprParser.Or(StringExpr.StringParser)).ManyDelimitedBy(Token.EqualTo(Tokens.Comma))
        from rparen in Token.EqualTo(Tokens.RParen)
        select new FuncCallExpr
        {
            Name = name,
            Arguments = args
        } as ExprNode;

    public override LLVMValueRef Codegen(CodegenScope codegenScope, TypeRef wanted)
    {
        var func = codegenScope.GetFunction(Name);
        if (!func.IsVarArg && Arguments.Count() != func.Parameters.Count())
        {
            throw new Exception("Number of parameters does not match");
        }

        var args = func.IsVarArg switch
        {
            true => Arguments
                .Take(func.Parameters.Count())
                .Zip(func.Parameters)
                .Select(arg =>
                    arg.First.Codegen(codegenScope, arg.Second.Type.GetTypeRef())
                )
                .Concat(
                    Arguments
                    .Skip(func.Parameters.Count())
                    .Select(arg =>
                        arg.Codegen(codegenScope, arg.InferType(codegenScope))
                    )
                )
                .ToArray(),
            false => Arguments
                .Zip(func.Parameters)
                .Select(arg =>
                    arg.First.Codegen(codegenScope, arg.Second.Type.GetTypeRef())
                )
                .ToArray()
        };

        return codegenScope.Builder.BuildCall2(func.LLVMFuncType, func.LLVMFuncValue, args);
    }

    public override TypeRef InferType(CodegenScope codegenScope)
    {
        var func = codegenScope.GetFunction(Name);
        return func.ReturnType;
    }
}
