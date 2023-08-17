using LLVMSharp.Interop;
using Superpower;
using Superpower.Parsers;

namespace cccc.AST.Expressions;


public abstract class ExprNode {
    static readonly TokenListParser<Tokens, Operator> Add =
            Token.EqualTo(Tokens.Plus).Value(Operator.Plus);

    static readonly TokenListParser<Tokens, Operator> Subtract =
        Token.EqualTo(Tokens.Minus).Value(Operator.Minus);

    static readonly TokenListParser<Tokens, Operator> Multiply =
        Token.EqualTo(Tokens.Mul).Value(Operator.Mul);

    static readonly TokenListParser<Tokens, Operator> Divide =
        Token.EqualTo(Tokens.Div).Value(Operator.Div);

    static readonly TokenListParser<Tokens, Operator> LessThen =
        Token.EqualTo(Tokens.LessThen).Value(Operator.LessThen);

    static readonly TokenListParser<Tokens, Operator> GreaterThen =
       Token.EqualTo(Tokens.GreaterThen).Value(Operator.GreaterThen);

    protected static readonly TokenListParser<Tokens, ExprNode> Factor =
        (from lparen in Token.EqualTo(Tokens.LParen)
         from expr in Parse.Ref(() => ExprParser!)
         from rparen in Token.EqualTo(Tokens.RParen)
         select expr)
        .Or(NumberExpr.NumberParser)
        .Or(FuncCallExpr.FuncCallExprParser.Try().Or(VariableExpr.VariableParser));

    static readonly TokenListParser<Tokens, ExprNode> Operand =
        NegatedExpr.Parser
        .Or(Factor).Named("expression");

    static readonly TokenListParser<Tokens, ExprNode> Term2 =
        Parse.Chain(LessThen.Or(GreaterThen), Operand, (op, left, right) => new BinaryExpr { Left = left, Op = op, Right = right });

    static readonly TokenListParser<Tokens, ExprNode> Term =
        Parse.Chain(Multiply.Or(Divide), Term2, (op, left, right) => new BinaryExpr { Left = left, Op = op, Right = right });

    public static readonly TokenListParser<Tokens, ExprNode> ExprParser =
        Parse.Chain(Add.Or(Subtract), Term, (op, left, right) => new BinaryExpr { Left = left, Op = op, Right = right });

    public abstract LLVMValueRef Codegen(CodegenScope codegenScope, TypeRef wanted);

    public abstract TypeRef InferType(CodegenScope codegenScope);
}
