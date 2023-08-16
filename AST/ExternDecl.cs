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
    public class ExternDecl : TopLevelStatement
    {
        public required TypedIdentifier Ident { get; set; }
        public required IEnumerable<TypedIdentifier> Parameters { get; set; }


        public static TokenListParser<Tokens, TopLevelStatement> ExternParser =
                from _extern in Token.EqualToValue(Tokens.Identifier, "extern")
                from typed_ident in TypedIdentifier.TypedIdentifierParser
                from lparen in Token.EqualTo(Tokens.LParen)
                from parameters in TypedIdentifier.TypedIdentifierParser.ManyDelimitedBy(Token.EqualTo(Tokens.Comma))
                from rparen in Token.EqualTo(Tokens.RParen)
                from semi in Token.EqualTo(Tokens.Semi)
                select new ExternDecl
                {
                    Ident = typed_ident,
                    Parameters = parameters
                } as TopLevelStatement;
    }
}
