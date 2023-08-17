using LLVMSharp.Interop;

namespace cccc.AST;

public class FuncRef
{
    public required LLVMTypeRef LLVMFuncType { get; set; }
    public required LLVMValueRef LLVMFuncValue { get; set; }
    public required IEnumerable<TypedIdentifier> Parameters { get; set; }
    public required TypeRef ReturnType { get; set; }
    public required bool IsVarArg { get; set; }
}

public class VarRef
{
    public required LLVMValueRef Storage { get; set; }
    public required TypeRef TypeRef { get; set; }
}

public class CodegenScope
{
    public LLVMModuleRef Module { get; }
    public LLVMBuilderRef Builder { get; }
    public CodegenScope? Parent { get; }

    private readonly Dictionary<string, VarRef> _variables = new();
    private readonly Dictionary<string, FuncRef> _functions = new();

    public CodegenScope(LLVMModuleRef module, LLVMBuilderRef builder)
    {
        Module = module;
        Builder = builder;
    }

    public CodegenScope(CodegenScope parent, LLVMBuilderRef builder)
    {
        Parent = parent;
        Builder = builder;
        Module = parent.Module;
    }

    public void DefineVariable(string name, TypeRef typeRef)
    {
        if (_variables.ContainsKey(name)) throw new Exception("Variable already defined");

        var storage = Builder.BuildAlloca(typeRef.GetLLVMType());
        _variables.Add(name, new VarRef { Storage = storage, TypeRef = typeRef });
    }

    public VarRef GetVariable(string name)
    {
        if (_variables.TryGetValue(name, out var value)) return value;
        var parentVar = Parent?.GetVariable(name);
        if (parentVar != null)
        {
            return parentVar;
        }
        throw new Exception("Variable not defined");
    }

    public FuncRef DefineFunction(string name, TypeRef returnType, IEnumerable<TypedIdentifier> parameters, bool isVarArg)
    {
        if (_functions.ContainsKey(name)) throw new Exception("Function already defined");

        var funcType = LLVMTypeRef.CreateFunction(
            returnType.GetLLVMType(),
            parameters.Select(p => p.Type.GetTypeRef().GetLLVMType()).ToArray(),
            isVarArg
        );

        var func = Module.AddFunction(name, funcType);

        _functions.Add(name, new FuncRef { LLVMFuncType = funcType, LLVMFuncValue = func, ReturnType = returnType, Parameters = parameters, IsVarArg = isVarArg });

        return _functions[name];
    }

    public FuncRef GetFunction(string name)
    {
        if (_functions.TryGetValue(name, out var value)) return value;
        var parentFunction = Parent?.GetFunction(name);
        if (parentFunction != null) return parentFunction;
        throw new Exception("Function not defined");
    }

}
