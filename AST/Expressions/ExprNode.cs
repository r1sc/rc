using Superpower;
using Superpower.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static cccc.Lexer;

namespace cccc.AST.Expressions
{

    public abstract class ExprNode {
        static readonly TokenListParser<Tokens, Operator> Add =
                Token.EqualTo(Tokens.Plus).Value(Operator.Plus);

        static readonly TokenListParser<Tokens, Operator> Subtract =
            Token.EqualTo(Tokens.Minus).Value(Operator.Minus);

        static readonly TokenListParser<Tokens, Operator> Multiply =
            Token.EqualTo(Tokens.Mul).Value(Operator.Mul);

        static readonly TokenListParser<Tokens, Operator> Divide =
            Token.EqualTo(Tokens.Div).Value(Operator.Div);

        static readonly TokenListParser<Tokens, ExprNode> Factor =
            (from lparen in Token.EqualTo(Tokens.LParen)
             from expr in Parse.Ref(() => ExprParser!)
             from rparen in Token.EqualTo(Tokens.RParen)
             select expr)
            .Or(NumberExpr.NumberParser)
            .Or(FuncCallExpr.FuncCallExprParser.Try().Or(VariableExpr.VariableParser));

        static readonly TokenListParser<Tokens, ExprNode> Operand =
            (from sign in Token.EqualTo(Tokens.Minus)
             from factor in Factor
             select new NegatedExpr { Expression = factor } as ExprNode
            )
            .Or(Factor).Named("expression");

        static readonly TokenListParser<Tokens, ExprNode> Term =
            Parse.Chain(Multiply.Or(Divide), Operand, (op, left, right) => new BinaryExpr { Left = left, Op = op, Right = right });

        public static readonly TokenListParser<Tokens, ExprNode> ExprParser =
            Parse.Chain(Add.Or(Subtract), Term, (op, left, right) => new BinaryExpr { Left = left, Op = op, Right = right }.FlattenConstants());
    }
}
