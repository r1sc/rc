using Superpower;
using Superpower.Parsers;
using Superpower.Tokenizers;

namespace cccc;

public enum Tokens
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

public class Lexer
{
    static TextParser<string> quoteparser =
       from quote in Character.EqualTo('"')
       from rest in Span.Except("\"")
       from endquote in Character.EqualTo('"')
       select "banan";

    public static Tokenizer<Tokens> Tokenizer = new TokenizerBuilder<Tokens>()
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

}
