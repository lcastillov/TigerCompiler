using Antlr.Runtime;
using System.Collections.Generic;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    abstract class BinaryNode : ExpressionNode
    {
        public BinaryNode() { }

        public BinaryNode(IToken token) : base(token) { }

        public BinaryNode(BinaryNode node) : base(node) { }

        public ExpressionNode LeftOperandNode
        {
            get
            {
                return this.Children[0] as ExpressionNode;
            }
        }

        public ExpressionNode RightOperandNode
        {
            get
            {
                return this.Children[1] as ExpressionNode;
            }
        }
    }
}
