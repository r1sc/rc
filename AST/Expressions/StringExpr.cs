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

    public class StringExpr : ExprNode
    {
        public required string Value { get; set; }

        public static TokenListParser<Tokens, ExprNode> StringParser =
            from str in Token.EqualTo(Tokens.String)
            select new StringExpr { Value = str.ToStringValue()[1..(str.Span.Length - 1)].Replace("\\n", "\n") } as ExprNode;
    }
}
