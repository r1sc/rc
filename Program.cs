using Superpower;
using cccc;
using System.Diagnostics;
using cccc.AST;

class Program
{
    

    static void Main(string[] args)
    {
       

        var src = @"extern void puts(u8* str);
extern u8* itoa(i32 value, u8* str, i32 base);

void main() {
	u8* hello_world = ""Hello world!\n"";
    u32 apa = 23 + 62 * (15 - 2);
    u3 hej = 200;
	puts(hello_world);
}";


        var tokens = Lexer.Tokenizer.Tokenize(src);
        var ast = AST.FileParser.Parse(tokens);
        var modbuilder = new ModBuilder("file.ec", LLVMSharp.Interop.LLVMContextRef.Global);
        var mod = modbuilder.Build(ast);
        mod.Dump();
        mod.PrintToFile("file.ll");
        Process.Start("clang.exe", "file.ll -o file.exe");
        
        //TypeCheck.DoTopLevel(ast);
    }
}