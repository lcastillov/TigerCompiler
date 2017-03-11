using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    abstract class AtomicNode : ExpressionNode
    {
        public AtomicNode() { }

        public AtomicNode(IToken token) : base(token) { }

        public AtomicNode(AtomicNode node) : base(node) { }
    }
}
