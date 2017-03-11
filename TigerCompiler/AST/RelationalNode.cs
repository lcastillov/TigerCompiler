using Antlr.Runtime;
using System.Reflection.Emit;

namespace TigerCompiler.AST
{
    abstract class RelationalNode : BinaryNode
    {
        public RelationalNode() { }
        
        public RelationalNode(IToken token) : base(token) { }

        public RelationalNode(RelationalNode node) : base(node) { }
    }
}
