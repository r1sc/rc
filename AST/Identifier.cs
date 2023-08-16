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
    public class Identifier
    {
        public static TokenListParser<Tokens, string> IdentifierParser =
            from identifier in Token.EqualTo(Tokens.Identifier)
            select identifier.ToStringValue();
    }
}
