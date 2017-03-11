using Antlr.Runtime;

namespace TigerCompiler.AST
{
    abstract class DeclarationNode : UtilNode
    {
        public DeclarationNode() { }

        public DeclarationNode(IToken token) : base(token) { }

        public DeclarationNode(DeclarationNode node) : base(node) { }
    }
}
