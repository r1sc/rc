using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cccc.AST.Expressions
{
    public class NegatedExpr : ExprNode
    {
        public required ExprNode Expression { get; set; }
    }
}
