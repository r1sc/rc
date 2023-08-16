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
    }

}
