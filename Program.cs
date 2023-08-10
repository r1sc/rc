using Superpower.Tokenizers;
using Superpower.Parsers;
using Superpower;
using System.Linq;
using System.Linq.Expressions;

class Program
{
    enum Tokens
    {
        None,
        Number,
        String,
        Identifier,
        LParen,
        RParen,
        LBrace,
        RBrace,
        Semi,
        Quote,
        Plus,
        Minus,
        Div,
        Mul,
        Eq,
        Comma
    }

    class TypeNode
    {
        public required string TypeName { get; set; }
        public int NumIndirections { get; set; }

        public override string ToString()
        {
            return $"{TypeName}{new string('*', NumIndirections)}";
        }
    }

    class TypedIdentifier
    {
        public required TypeNode Type { get; set; }
        public required string Name { get; set; }

        public override string ToString()
        {
            return $"{Type} {Name}";
        }
    }
    abstract class TopLevelStatement { }
    abstract class Statement : TopLevelStatement { }

    abstract class ExprNode { }

    class NumberExpr : ExprNode
    {
        public required int Value { get; set; }
    }

    class NegatedExpr : ExprNode
    {
        public required ExprNode Expression { get; set; }
    }

    enum Operator
    {
        Plus,
        Minus,
        Mul,
        Div
    }

    class BinaryExpr : ExprNode
    {
        public required ExprNode Left { get; set; }
        public required Operator Op { get; set; }
        public required ExprNode Right { get; set; }


        public ExprNode FlattenConstants()
        {
            var l = Left is BinaryExpr ll ? ll.FlattenConstants() : Left;
            var r = Right is BinaryExpr rr ? rr.FlattenConstants() : Right;

            if (l is NumberExpr ln && r is NumberExpr rn)
            {
                switch (Op)
                {
                    case Operator.Plus:
                        return new NumberExpr { Value = ln.Value + rn.Value };
                    case Operator.Minus:
                        return new NumberExpr { Value = ln.Value - rn.Value };
                    case Operator.Mul:
                        return new NumberExpr { Value = ln.Value * rn.Value };
                    case Operator.Div:
                        return new NumberExpr { Value = ln.Value / rn.Value };
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                return this;
            }
        }
    }

    class StringExpr : ExprNode
    {
        public required string Value { get; set; }
    }

    class FuncCallExpr : ExprNode
    {
        public required string Name { get; set; }
        public required IEnumerable<ExprNode> Arguments { get; set; }
    }

    class VariableExpr : ExprNode
    {
        public string Name { get; set; }
    }


    class FuncCallNode : Statement
    {
        public required FuncCallExpr Expression { get; set; }
    }

    class VarDecl : Statement
    {
        public required TypedIdentifier Identifier { get; set; }
        public required ExprNode? Expression { get; set; }
    }

    class ExternDecl : TopLevelStatement
    {
        public required TypedIdentifier Ident { get; set; }
        public required IEnumerable<TypedIdentifier> Parameters { get; set; }
    }

    class Scope
    {
        public required IEnumerable<Statement> Statements { get; set; }
    }

    class FuncDefNode : TopLevelStatement
    {
        public required TypedIdentifier Ident { get; set; }
        public required IEnumerable<TypedIdentifier> Parameters { get; set; }
        public required Scope Scope { get; set; }
    }



    static TokenListParser<Tokens, TypeNode> TypeParser =
            from typename in Token.EqualTo(Tokens.Identifier)
            from indirections in Token.EqualTo(Tokens.Mul).Many()
            select new TypeNode
            {
                TypeName = typename.ToStringValue(),
                NumIndirections = indirections.Length
            };

    static TokenListParser<Tokens, string> IdentifierParser =
        from identifier in Token.EqualTo(Tokens.Identifier)
        select identifier.ToStringValue();

    static TokenListParser<Tokens, TypedIdentifier> TypedIdentifierParser =
            from type in TypeParser
            from name in IdentifierParser
            select new TypedIdentifier
            {
                Type = type,
                Name = name
            };

    static TokenListParser<Tokens, TopLevelStatement> ExternParser =
            from _extern in Token.EqualToValue(Tokens.Identifier, "extern")
            from typed_ident in TypedIdentifierParser
            from lparen in Token.EqualTo(Tokens.LParen)
            from parameters in TypedIdentifierParser.ManyDelimitedBy(Token.EqualTo(Tokens.Comma))
            from rparen in Token.EqualTo(Tokens.RParen)
            from semi in Token.EqualTo(Tokens.Semi)
            select new ExternDecl
            {
                Ident = typed_ident,
                Parameters = parameters
            } as TopLevelStatement;

    static TokenListParser<Tokens, Scope> ScopeParser =
        from lbrace in Token.EqualTo(Tokens.LBrace)
        from statements in Parse.Ref(() => StatementParser!).Many()
        from rbrace in Token.EqualTo(Tokens.RBrace)
        select new Scope { Statements = statements };

    static TokenListParser<Tokens, TopLevelStatement> FuncDefParser =
            from typed_ident in TypedIdentifierParser
            from lparen in Token.EqualTo(Tokens.LParen)
            from parameters in TypedIdentifierParser.ManyDelimitedBy(Token.EqualTo(Tokens.Comma))
            from rparen in Token.EqualTo(Tokens.RParen)
            from scope in Parse.Ref(() => ScopeParser)
            select new FuncDefNode
            {
                Ident = typed_ident,
                Parameters = parameters,
                Scope = scope
            } as TopLevelStatement;


    static TokenListParser<Tokens, TopLevelStatement> TopLevelStatementParser =
        ExternParser.Or(FuncDefParser);

    static TokenListParser<Tokens, Statement> VarDeclParser =
        from typed_ident in TypedIdentifierParser
        from expression in (
            from eq in Token.EqualTo(Tokens.Eq)
            from expr in Parse.Ref(() => StringParser.Or(ExprParser))
            select expr
        ).OptionalOrDefault()
        select new VarDecl
        {
            Identifier = typed_ident,
            Expression = expression
        } as Statement;

    static readonly TokenListParser<Tokens, Statement> FuncCallParser =
        from funccall in Parse.Ref(() => FuncCallExprParser)
        select new FuncCallNode { Expression = funccall as FuncCallExpr } as Statement;

    static TokenListParser<Tokens, Statement> StatementParser =
        from statement in FuncCallParser.Try().Or(VarDeclParser)
        from semi in Token.EqualTo(Tokens.Semi)
        select statement;

    static TokenListParser<Tokens, TopLevelStatement[]> FileParser =
        from statements in TopLevelStatementParser.Many().AtEnd()
        select statements;

    static TokenListParser<Tokens, ExprNode> NumberParser =
        from num in Token.EqualTo(Tokens.Number)
        select new NumberExpr { Value = int.Parse(num.ToStringValue()) } as ExprNode;

    static TokenListParser<Tokens, ExprNode> StringParser =
        from str in Token.EqualTo(Tokens.String)
        select new StringExpr { Value = str.ToStringValue() } as ExprNode;

    static TokenListParser<Tokens, ExprNode> VariableParser =
        from name in IdentifierParser
        select new VariableExpr { Name = name } as ExprNode;


    static readonly TokenListParser<Tokens, ExprNode> FuncCallExprParser =
        from name in IdentifierParser
        from lparen in Token.EqualTo(Tokens.LParen)
        from args in Parse.Ref(() => ExprParser.Or(StringParser)).ManyDelimitedBy(Token.EqualTo(Tokens.Comma))
        from rparen in Token.EqualTo(Tokens.RParen)
        select new FuncCallExpr
        {
            Name = name,
            Arguments = args
        } as ExprNode;

    static readonly TokenListParser<Tokens, Operator> Add =
        Token.EqualTo(Tokens.Plus).Value(Operator.Plus);

    static readonly TokenListParser<Tokens, Operator> Subtract =
        Token.EqualTo(Tokens.Minus).Value(Operator.Minus);

    static readonly TokenListParser<Tokens, Operator> Multiply =
        Token.EqualTo(Tokens.Mul).Value(Operator.Mul);

    static readonly TokenListParser<Tokens, Operator> Divide =
        Token.EqualTo(Tokens.Div).Value(Operator.Div);

    static readonly TokenListParser<Tokens, ExprNode> Factor =
        (from lparen in Token.EqualTo(Tokens.LParen)
         from expr in Parse.Ref(() => ExprParser)
         from rparen in Token.EqualTo(Tokens.RParen)
         select expr)
        .Or(NumberParser).Or(FuncCallExprParser.Try().Or(VariableParser));

    static readonly TokenListParser<Tokens, ExprNode> Operand =
        (from sign in Token.EqualTo(Tokens.Minus)
         from factor in Factor
         select new NegatedExpr { Expression = factor } as ExprNode
        )
        .Or(Factor).Named("expression");

    static readonly TokenListParser<Tokens, ExprNode> Term =
        Parse.Chain(Multiply.Or(Divide), Operand, (op, left, right) => new BinaryExpr { Left = left, Op = op, Right = right });

    static readonly TokenListParser<Tokens, ExprNode> ExprParser =
        Parse.Chain(Add.Or(Subtract), Term, (op, left, right) => new BinaryExpr { Left = left, Op = op, Right = right }.FlattenConstants());


    static void Main(string[] args)
    {
        var quoteparser =
            from quote in Character.In('"')
            from rest in Span.Except("\"")
            from endquote in Character.In('"')
            select rest;

        var tokenizer = new TokenizerBuilder<Tokens>()
            .Ignore(Span.WhiteSpace)
            .Ignore(Comment.CPlusPlusStyle)
            .Ignore(Comment.CStyle)
            .Match(Numerics.Natural, Tokens.Number)
            .Match(quoteparser, Tokens.String)
            .Match(Identifier.CStyle, Tokens.Identifier)
            .Match(Character.In('('), Tokens.LParen)
            .Match(Character.In(')'), Tokens.RParen)
            .Match(Character.In('{'), Tokens.LBrace)
            .Match(Character.In('}'), Tokens.RBrace)
            .Match(Character.In(';'), Tokens.Semi)
            .Match(Character.In('+'), Tokens.Plus)
            .Match(Character.In('-'), Tokens.Minus)
            .Match(Character.In('/'), Tokens.Div)
            .Match(Character.In('*'), Tokens.Mul)
            .Match(Character.In('='), Tokens.Eq)
            .Match(Character.In(','), Tokens.Comma)
            .Build();


        var src = @"extern void puts(u8* str);

void main() {
	u8* hello_world = ""Hello world!\n"";
	puts(hello_world);
}";


        var tokens = tokenizer.Tokenize(src);
        var ast = FileParser.Parse(tokens);
        // See https://aka.ms/new-console-template for more information
        Console.WriteLine("Hello, World!");
    }
}