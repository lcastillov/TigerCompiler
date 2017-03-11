using Antlr.Runtime;

namespace TigerCompiler.AST
{
    abstract class BlockDeclarationsNode : UtilNode
    {
        public BlockDeclarationsNode() { }

        public BlockDeclarationsNode(IToken token) : base(token) { }

        public BlockDeclarationsNode(BlockDeclarationsNode node) : base(node) { }
    }
}
