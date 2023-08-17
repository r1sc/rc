using cccc.AST.Statements;
using Superpower;

namespace cccc.AST;

public class TypedIdentifier
{
    public required TypeNode Type { get; set; }
    public required string Name { get; set; }

    public override string ToString()
    {
        return $"{Type} {Name}";
    }

    public static TokenListParser<Tokens, TypedIdentifier> TypedIdentifierParser =
            from type in TypeNode.TypeParser
            from name in Identifier.IdentifierParser
            select new TypedIdentifier
            {
                Type = type,
                Name = name
            };


}
