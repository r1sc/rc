using LLVMSharp.Interop;

namespace cccc.AST;

public class NumberTypeRef : TypeRef
{
    public required uint NumBits { get; set; }
    public required bool Signed { get; set; }

    public (NumberTypeRef typeref, bool extend) AutoCast(NumberTypeRef target)
    {
        var widen = target.NumBits >= NumBits;
        return (target, widen);
    }

    public override LLVMTypeRef GetLLVMType()
    {
        return LLVMTypeRef.CreateInt(NumBits);
    }
}

public class PointerTypeRef : TypeRef
{
    public required TypeRef Inner { get; set; }
    public override LLVMTypeRef GetLLVMType()
    {
        return LLVMTypeRef.CreatePointer(Inner.GetLLVMType(), 0);
    }
}

public class VoidTypeRef : TypeRef {
    public override LLVMTypeRef GetLLVMType()
    {
        return LLVMTypeRef.Void;
    }
}

public class VarArgsTypeRef : TypeRef
{
    public override LLVMTypeRef GetLLVMType()
    {
        return LLVMTypeRef.Void;
    }
}

public abstract class TypeRef {
    public abstract LLVMTypeRef GetLLVMType();
}