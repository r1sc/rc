using Superpower;
using Superpower.Parsers;

namespace cccc.AST.Statements;

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
