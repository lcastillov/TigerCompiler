using Antlr.Runtime;
using System.Reflection.Emit;

namespace TigerCompiler.AST
{
    class DivNode : ArithmeticNode
    {
        public DivNode() { }

        public DivNode(IToken token) : base(token) { }

        public DivNode(DivNode node) : base(node) { }

        public override string OperatorName
        {
            get { return "divide"; }
        }

        public override OpCode OperatorOpCode
        {
            get { return OpCodes.Div; }
        }
    }
}
