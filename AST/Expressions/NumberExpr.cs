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
    public class NumberExpr : ExprNode
    {
        public required int Value { get; set; }

        public static TokenListParser<Tokens, ExprNode> NumberParser =
            from num in Token.EqualTo(Tokens.Number)
            select new NumberExpr { Value = int.Parse(num.ToStringValue()) } as ExprNode;
    }
}
