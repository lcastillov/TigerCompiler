using Antlr.Runtime;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    abstract class UtilNode : TigerNode
    {
        public bool IsOk { get; set; }

        public UtilNode() : base() { }

        public UtilNode(IToken token) : base(token) { }

        public UtilNode(UtilNode node) : base(node) { }
    }
}
