using Superpower;
using Superpower.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static cccc.AST.AST;
using static cccc.Lexer;

namespace cccc.AST
{

    public class FuncDefNode : TopLevelStatement
    {
        public required TypedIdentifier Ident { get; set; }
        public required IEnumerable<TypedIdentifier> Parameters { get; set; }
        public required Scope Scope { get; set; }

        public static TokenListParser<Tokens, TopLevelStatement> FuncDefParser =
                from typed_ident in TypedIdentifier.TypedIdentifierParser
                from lparen in Token.EqualTo(Tokens.LParen)
                from parameters in TypedIdentifier.TypedIdentifierParser.ManyDelimitedBy(Token.EqualTo(Tokens.Comma))
                from rparen in Token.EqualTo(Tokens.RParen)
                from scope in Parse.Ref(() => Scope.ScopeParser)
                select new FuncDefNode
                {
                    Ident = typed_ident,
                    Parameters = parameters,
                    Scope = scope
                } as TopLevelStatement;
    }


}
