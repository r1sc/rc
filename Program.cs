using cccc.AST;
using LLVMSharp.Interop;
using Superpower;
using System.Diagnostics;

namespace cccc;

class Program
{
    static void Main(string[] args)
    {
        var src = @"extern void printf(u8* format)...;

void main() {
	u32 i = 5;
    printf(""Hello %d"", i);
}";

        var tokens = Lexer.Tokenizer.Tokenize(src);
        var ast = AST.Statements.TopLevelStatement.FileParser.Parse(tokens);

        var llvmContext = LLVMContextRef.Create();
        var mod = llvmContext.CreateModuleWithName("file.ec");
        var codegenScope = new CodegenScope(mod, mod.Context.CreateBuilder());

        foreach (var statement in ast)
        {
            statement.Codegen(codegenScope);
        }

        mod.Dump();
        mod.PrintToFile("file.ll");
        Process.Start("clang.exe", "file.ll -o file.exe").WaitForExit();
        Process.Start("file.exe").WaitForExit();
    }
}