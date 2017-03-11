using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TigerCompiler.AST
{
    class StarNode : ArithmeticNode
    {
        public StarNode() : base() { }

        public StarNode(IToken token) : base(token) { }

        public StarNode(StarNode node) : base(node) { }

        public override string OperatorName
        {
            get { return "multiply"; }
        }

        public override OpCode OperatorOpCode
        {
            get { return OpCodes.Mul; }
        }
    }
}
