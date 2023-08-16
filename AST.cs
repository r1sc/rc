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
    public class AST
    {
        public static TokenListParser<Tokens, TopLevelStatement[]> FileParser =
            from statements in TopLevelStatement.TopLevelStatementParser.Many().AtEnd()
            select statements;

    }
}
