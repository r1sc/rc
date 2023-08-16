using Superpower;
using Superpower.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static cccc.Lexer;

namespace cccc.AST
{
    public class Scope
    {
        public required IEnumerable<Statement> Statements { get; set; }


        public static TokenListParser<Tokens, Scope> ScopeParser =
            from lbrace in Token.EqualTo(Tokens.LBrace)
            from statements in Parse.Ref(() => Statement.StatementParser!).Many()
            from rbrace in Token.EqualTo(Tokens.RBrace)
            select new Scope { Statements = statements };
    }
}
