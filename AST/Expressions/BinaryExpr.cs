using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cccc.AST.Expressions
{
    public enum Operator
    {
        Plus,
        Minus,
        Mul,
        Div
    }

    public class BinaryExpr : ExprNode
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
}
