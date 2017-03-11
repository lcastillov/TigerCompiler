using Antlr.Runtime;
using System.Reflection.Emit;

namespace TigerCompiler.AST
{
    class MinusNode : ArithmeticNode
    {
        public MinusNode() : base() { }

        public MinusNode(IToken token) : base(token) { }

        public MinusNode(MinusNode node) : base(node) { }

        public override string OperatorName
        {
            get { return "minus"; }
        }

        public override OpCode OperatorOpCode
        {
            get { return OpCodes.Sub; }
        }
    }
}
