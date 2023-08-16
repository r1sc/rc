using cccc.AST.Expressions;
using cccc.AST.Statements;

namespace cccc;

class TypeCheck
{
    public abstract record CType(int NumIndirections = 0);
    public record SomeType(string Name, int NumIndirections) : CType(NumIndirections)
    {
        public override string ToString()
        {
            return $"{Name}{new string('*', NumIndirections)}";
        }
    }
    public record FuncType(CType ReturnType, IEnumerable<CType> ParameterTypes) : CType(0);

    public class TypeCheckingScope
    {
        public Dictionary<string, CType> ResolvedTypes { get; set; } = new Dictionary<string, CType>();
        public TypeCheckingScope? Parent { get; set; } = null;

        public CType? Resolve(string name)
        {
            if(ResolvedTypes.ContainsKey(name)) return ResolvedTypes[name];
            return Parent?.Resolve(name);
        }
    }

    public static SomeType FromTypeNode(AST.TypeNode t) => new(t.TypeName, t.NumIndirections);

    public static void DoTopLevel(TopLevelStatement[] topLevelStatements)
    {
        var rootScope = new TypeCheckingScope();

        foreach (var statement in topLevelStatements)
        {
            switch (statement)
            {
                case ExternDecl e:
                    {
                        var t = new FuncType(FromTypeNode(e.Ident.Type), e.Parameters.Select(p => FromTypeNode(p.Type)));
                        rootScope.ResolvedTypes.Add(e.Ident.Name, t);
                        break;
                    }
                case FuncDefNode e:
                    {
                        var t = new FuncType(FromTypeNode(e.Ident.Type), e.Parameters.Select(p => FromTypeNode(p.Type)));
                        rootScope.ResolvedTypes.Add(e.Ident.Name, t);
                        VisitFunc(e, rootScope);
                        break;
                    }
            }
        }
    }

    static void VisitFunc(FuncDefNode funcDefNode, TypeCheckingScope parent)
    {
        var scope = new TypeCheckingScope { Parent = parent };
        foreach (var param in funcDefNode.Parameters)
        {
            scope.ResolvedTypes.Add(param.Name, FromTypeNode(param.Type));
        }

        foreach (var statement in funcDefNode.Scope.Statements)
        {
            switch (statement)
            {
                case VarDecl v:
                    if (v.Expression != null)
                    {
                        var left = FromTypeNode(v.Identifier.Type);
                        var right = ResolveExprType(v.Expression, scope);
                        if (left != right)
                        {
                            throw new Exception($"Type mismatch. RHS is '{right}' but LHS is '{left}'");
                        }
                    }
                    scope.ResolvedTypes.Add(v.Identifier.Name, FromTypeNode(v.Identifier.Type));
                    break;
                case FuncCallNode f:
                    ResolveExprType(f.Expression, scope);
                    break;
            }
        }
    }

    static CType ResolveExprType(ExprNode exprNode, TypeCheckingScope scope)
    {
        switch (exprNode)
        {
            case StringExpr s:
                return new SomeType("u8", 1);
            case NumberExpr n:
                return new SomeType("u32", 0);
            case BinaryExpr b:
                var left = ResolveExprType(b.Left, scope);
                var right = ResolveExprType(b.Right, scope);
                if (left != right)
                {
                    throw new Exception("Both sides in binary expr must be same type");
                }
                return left;
            case FuncCallExpr f:
                var target = scope.Resolve(f.Name);
                if (target != null)
                {
                    if (target is FuncType targetFunction)
                    {
                        var paramCount = targetFunction.ParameterTypes.Count();
                        var argCount = f.Arguments.Count();
                        if (paramCount != argCount)
                        {
                            throw new Exception($"Expected {paramCount} parameter(s), but {argCount} provided");

                        }
                        foreach (var (expr, resolvedType) in f.Arguments.Zip(targetFunction.ParameterTypes))
                        {
                            var r = ResolveExprType(expr, scope);
                            if (r != resolvedType)
                            {
                                throw new Exception($"Type mismatch in call to '{f.Name}': expected '{resolvedType}' got '{r}'");
                            }
                        }
                        return targetFunction.ReturnType;
                    }
                    else
                    {
                        throw new Exception($"'{f.Name}' is not a function");
                    }
                }
                else
                {
                    throw new Exception($"Undeclared function '{f.Name}'");
                }
            case VariableExpr v:
                return scope.Resolve(v.Name) ?? throw new Exception($"Undefined variable '{v.Name}'");
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
