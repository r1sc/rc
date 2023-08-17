using Superpower;
using Superpower.Parsers;

namespace cccc.AST;

public class TypeNode
{
    public required string TypeName { get; set; }
    public int NumIndirections { get; set; }

    public override string ToString()
    {
        return $"{TypeName}{new string('*', NumIndirections)}";
    }

    public static TokenListParser<Tokens, TypeNode> TypeParser =
        from typename in Token.EqualTo(Tokens.Identifier)
        from indirections in Token.EqualTo(Tokens.Mul).Many()
        select new TypeNode
        {
            TypeName = typename.ToStringValue(),
            NumIndirections = indirections.Length
        };


    public TypeRef GetTypeRef()
    {
        if (NumIndirections > 0)
        {
            var typeRef = new TypeNode { TypeName = TypeName, NumIndirections = NumIndirections - 1 }.GetTypeRef();
            return new PointerTypeRef { Inner = typeRef };
        }

        if (TypeName.Length > 1 && (TypeName[0] == 'i' || TypeName[0] == 'u') && int.TryParse(TypeName[1..], out var bits))
        {
            var signed = TypeName[0] == 'i';
            return new NumberTypeRef { NumBits = (uint)bits, Signed = signed };
        }

        var baseType = TypeName switch
        {
            "void" => (TypeRef)new VoidTypeRef(),
            "..." => new VarArgsTypeRef(),
            _ => throw new Exception("Blah")
        }; ;

        return baseType;
    }
}
