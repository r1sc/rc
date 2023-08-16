using cccc.AST;
using cccc.AST.Expressions;
using LLVMSharp.Interop;

namespace cccc
{
    class ScopedStuff
    {
        Dictionary<string, (LLVMTypeRef type, bool signed, LLVMValueRef storage, bool writable)> _localVars = new();
        private readonly ScopedStuff? _parent;

        public (LLVMTypeRef type, bool signed) GetTypeRef(AST.TypeNode typeNode)
        {
            if (typeNode.NumIndirections > 0)
            {
                var typeRef = GetTypeRef(new AST.TypeNode { TypeName = typeNode.TypeName, NumIndirections = typeNode.NumIndirections - 1 });
                return (LLVMTypeRef.CreatePointer(typeRef.type, 0), typeRef.signed);
            }

            if (typeNode.TypeName.Length > 1 && (typeNode.TypeName[0] == 'i' || typeNode.TypeName[0] == 'u') && int.TryParse(typeNode.TypeName[1..], out var bits)) {
                var signed = typeNode.TypeName[0] == 'i';
                return (LLVMTypeRef.CreateInt((uint)bits), signed);
            }
            var baseType = typeNode.TypeName switch
            {
                "void" => (LLVMTypeRef.Void, false),
                _ => throw new Exception("Blah")
            };
            
            return baseType;
        }

        public void CreateStorageForLocalVariable(string name, TypeNode typeNode, LLVMBuilderRef builder)
        {            
            var paramType = GetTypeRef(typeNode);
            var storage = builder.BuildAlloca(paramType.type);
            _localVars.Add(name, (paramType.type, paramType.signed, storage, true));
        }

        public void CreateConstantVarRef(string name, LLVMTypeRef type, LLVMValueRef value, bool signed)
        {
            _localVars.Add(name, (type, signed, value, false));
        }

        public (LLVMTypeRef type, bool signed, LLVMValueRef storage, bool writable) GetVariable(string name)
        {
            if(!_localVars.ContainsKey(name))
            {
                if(_parent != null)
                {
                    return _parent.GetVariable(name);
                }
                throw new Exception("Variable not defined");
            }

            return _localVars[name];
        }

        public ScopedStuff(ScopedStuff? parent)
        {
            _parent = parent;
        }
    }

    class ModBuilder
    {
        private Dictionary<string, LLVMTypeRef> FunctionTypes = new();
        private Dictionary<string, (LLVMTypeRef type, bool signed)[]> FunctionParameterTypes = new();
        private ScopedStuff _modScope = new(null);
        private LLVMModuleRef _mod;

        public ModBuilder(string name, LLVMContextRef context)
        {
            _mod = context.CreateModuleWithName(name);
        }

        private LLVMTypeRef GetOrCreateFunctionType(AST.TypedIdentifier ident, IEnumerable<AST.TypeNode> parameters)
        {
            if(!FunctionTypes.ContainsKey(ident.Name))
            {
                var llvmReturnType = _modScope.GetTypeRef(ident.Type);
                var llvmParamTypes = parameters.Select(_modScope.GetTypeRef).ToArray();
                var llvmFunctionType = LLVMTypeRef.CreateFunction(llvmReturnType.type, llvmParamTypes.Select(t => t.type).ToArray(), false);
                FunctionTypes.Add(ident.Name, llvmFunctionType);
                FunctionParameterTypes.Add(ident.Name, llvmParamTypes);
            }

            return FunctionTypes[ident.Name];
        }

        private void BuildExtern(AST.ExternDecl externDecl)
        {
            var llvmFuncType = GetOrCreateFunctionType(externDecl.Ident, externDecl.Parameters.Select(i => i.Type));
            _mod.AddFunction(externDecl.Ident.Name, llvmFuncType);
        }

        private LLVMValueRef BuildFuncCallExpr(FuncCallExpr funcCallExpr, ScopedStuff scope, LLVMBuilderRef builder)
        {
            if (!FunctionTypes.ContainsKey(funcCallExpr.Name))
            {
                throw new Exception("Function not defined");
            }

            var llvmFunc = _mod.GetNamedFunction(funcCallExpr.Name);
            var llvmFuncType = FunctionTypes[funcCallExpr.Name];
            var llvmParamTypes = FunctionParameterTypes[funcCallExpr.Name];
            if (llvmParamTypes.Length != funcCallExpr.Arguments.Count())
            {
                throw new Exception("Number of parameters does not match");
            }

            var llvmArgs = funcCallExpr.Arguments.Zip(llvmParamTypes).Select(arg => BuildExpr(arg.First, scope, builder, arg.Second)).ToArray();
            return builder.BuildCall2(llvmFuncType, llvmFunc, llvmArgs);
        }

        private LLVMValueRef BuildExpr(ExprNode expr, ScopedStuff scope, LLVMBuilderRef builder, (LLVMTypeRef type, bool signed) saturatingType)
        {
            switch (expr)
            {
                case NumberExpr num:
                    return LLVMValueRef.CreateConstInt(saturatingType.type, (ulong)num.Value, saturatingType.signed);
                case StringExpr str:
                    return builder.BuildGlobalStringPtr(str.Value);
                case NegatedExpr neg:
                    return builder.BuildNeg(BuildExpr(neg.Expression, scope, builder, saturatingType));
                case BinaryExpr binaryExpr:
                    var lhs = BuildExpr(binaryExpr.Left, scope, builder, saturatingType);
                    var rhs = BuildExpr(binaryExpr.Right, scope, builder, saturatingType);
                    switch (binaryExpr.Op)
                    {
                        case Operator.Plus:
                            return builder.BuildAdd(lhs, rhs);
                        case Operator.Minus:
                            return builder.BuildSub(lhs, rhs);
                        case Operator.Mul:
                            return builder.BuildMul(lhs, rhs);
                        case Operator.Div:
                            return saturatingType.signed ? builder.BuildSDiv(lhs, rhs) : builder.BuildUDiv(lhs, rhs);
                    }
                    throw new ArgumentOutOfRangeException();
                case FuncCallExpr funcCallExpr:
                    return BuildFuncCallExpr(funcCallExpr,scope, builder);
                case VariableExpr varExpr:
                    var variable = scope.GetVariable(varExpr.Name);
                    return builder.BuildLoad2(variable.type, variable.storage);
                    
            }
            throw new ArgumentOutOfRangeException();
        }

        private void BuildScope(AST.Scope scope, ScopedStuff parentScope, LLVMBuilderRef builder, IEnumerable<TypedIdentifier> funcParams)
        {
            var localScope = new ScopedStuff(parentScope);

            foreach (var funcParam in funcParams)
            {
                localScope.CreateStorageForLocalVariable(funcParam.Name, funcParam.Type, builder);
            }

            foreach (var statement in scope.Statements)
            {
                switch (statement)
                {
                    case AST.FuncCallNode funcCall:
                        BuildFuncCallExpr(funcCall.Expression, localScope, builder);
                        break;
                    case AST.VarDecl varDecl:
                        localScope.CreateStorageForLocalVariable(varDecl.Identifier.Name, varDecl.Identifier.Type, builder);
                        if(varDecl.Expression != null)
                        {
                            var typedVar = localScope.GetVariable(varDecl.Identifier.Name);
                            builder.BuildStore(BuildExpr(varDecl.Expression, localScope, builder, (typedVar.type, typedVar.signed)), typedVar.storage);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void BuildFunc(AST.FuncDefNode funcDef)
        {
            var llvmFuncType = GetOrCreateFunctionType(funcDef.Ident, funcDef.Parameters.Select(i => i.Type));
            var llvmFunc = _mod.AddFunction(funcDef.Ident.Name, llvmFuncType);
            var llvmBlockRef = llvmFunc.AppendBasicBlock("entry");

            var builder = _mod.Context.CreateBuilder();
            builder.Position(llvmBlockRef, llvmBlockRef.FirstInstruction);

            BuildScope(funcDef.Scope, _modScope, builder, funcDef.Parameters);

            if(llvmFuncType.ReturnType == LLVMTypeRef.Void)
            {
                builder.BuildRetVoid();
            }
            else
            {
                throw new Exception("Return statement not supported yet");
            }
        }

        public LLVMModuleRef Build(AST.TopLevelStatement[] statements)
        {
            foreach(var statement in statements)
            {
                switch (statement)
                {
                    case AST.ExternDecl externDecl:
                        BuildExtern(externDecl); 
                        break;
                    case AST.FuncDefNode funcDef:
                        BuildFunc(funcDef); 
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return _mod;
        }

    }
}
