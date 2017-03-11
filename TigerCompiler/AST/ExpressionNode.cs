using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    abstract class ExpressionNode : TigerNode
    {
        public ExpressionNode() { }

        public ExpressionNode(IToken token) : base(token) { }

        public ExpressionNode(ExpressionNode node) : base(node) { }

        public TypeNode ExpressionType { get; set; }
    }
}
