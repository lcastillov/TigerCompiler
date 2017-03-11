using Antlr.Runtime;
using System.Reflection.Emit;

namespace TigerCompiler.AST
{
    class PlusNode : ArithmeticNode
    {
        public PlusNode() { }
        
        public PlusNode(IToken token) : base(token) { }

        public PlusNode(PlusNode node) : base(node) { }

        public override string OperatorName
        {
            get { return "plus"; }
        }

        public override OpCode OperatorOpCode
        {
            get { return OpCodes.Add; }
        }
    }
}
