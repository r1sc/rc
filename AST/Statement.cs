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

    public abstract class Statement { 
        public static TokenListParser<Tokens, Statement> StatementParser =
            from statement in FuncCallNode.FuncCallParser.Try().Or(VarDecl.VarDeclParser)
            from semi in Token.EqualTo(Tokens.Semi)
            select statement;
    }
}
