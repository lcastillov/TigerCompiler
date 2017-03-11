using Antlr.Runtime;

namespace TigerCompiler.AST
{
    abstract class UnaryNode : ExpressionNode
    {
        public UnaryNode() : base() { }

        public UnaryNode(IToken token) : base(token) { }

        public UnaryNode(UnaryNode node) : base(node) { }

        public ExpressionNode OperandNode
        {
            get
            {
                return this.Children[0] as ExpressionNode;
            }
        }
    }
}
