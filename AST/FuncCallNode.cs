using cccc.AST.Expressions;
using Superpower;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static cccc.Lexer;

namespace cccc.AST
{

    public class FuncCallNode : Statement
    {
        public required FuncCallExpr Expression { get; set; }

        public static readonly TokenListParser<Tokens, Statement> FuncCallParser =
            from funccall in Parse.Ref(() => FuncCallExpr.FuncCallExprParser)
            select new FuncCallNode { Expression = (funccall as FuncCallExpr)! } as Statement;
    }
}
