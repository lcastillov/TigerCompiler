using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TigerCompiler.AST
{
    abstract class AccessNode : ExpressionNode
    {
        public AccessNode() { }

        public AccessNode(IToken token) : base(token) { }

        public AccessNode(AccessNode node) : base(node) { }

        public bool ReadOnly { get; set; }

        //----------------------------------------------------------------------------------------------------
        // Llamado Solamente Cuando Se Utilice 'AccessNode' Como Hijo Izquierdo.
        //----------------------------------------------------------------------------------------------------
        public abstract void GenerateCode2(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator, ExpressionNode value, bool isRoot);
    }
}
