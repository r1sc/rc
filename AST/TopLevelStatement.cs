using Superpower;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static cccc.Lexer;

namespace cccc.AST
{

    public abstract class TopLevelStatement { 
    
        public static TokenListParser<Tokens, TopLevelStatement> TopLevelStatementParser =
            ExternDecl.ExternParser.Or(FuncDefNode.FuncDefParser);
    }
}
