using cccc.AST.Expressions;
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

    public class VarDecl : Statement
    {
        public required TypedIdentifier Identifier { get; set; }
        public required ExprNode? Expression { get; set; }

        public static TokenListParser<Tokens, Statement> VarDeclParser =
            from typed_ident in TypedIdentifier.TypedIdentifierParser
            from expression in (
                from eq in Token.EqualTo(Tokens.Eq)
                from expr in Parse.Ref(() => StringExpr.StringParser.Or(ExprNode.ExprParser))
                select expr
            ).OptionalOrDefault()
            select new VarDecl
            {
                Identifier = typed_ident,
                Expression = expression
            } as Statement;
    }
}
