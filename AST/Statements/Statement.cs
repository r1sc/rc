using LLVMSharp.Interop;
using Superpower;
using Superpower.Parsers;

namespace cccc.AST.Statements;

public abstract class Statement
{
    static TokenListParser<Tokens, Statement> SemiTerminatedStatementParser =
        from statement in AssignStatement.Parser.Try()
                        .Or(FuncCallNode.FuncCallParser.Try())
                        .Or(VarDeclStatement.VarDeclParser)
        from semi in Token.EqualTo(Tokens.Semi)
        select statement;

    public static TokenListParser<Tokens, Statement> StatementParser =
        WhileStatement.Parser
        .Or(IfStatement.Parser)
        .Or(SemiTerminatedStatementParser);


    public abstract void Codegen(CodegenScope codegenScope);
}
