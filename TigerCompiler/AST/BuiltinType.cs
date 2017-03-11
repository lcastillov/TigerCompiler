using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    class BuiltinType : TypeNode
    {
        public BuiltinType() { }

        public BuiltinType(IToken token) : base(token) { }

        public BuiltinType(BuiltinType node) : base(node) { }

        public override void CheckSemantics(Scope scope, List<SemanticError> errors)
        {
            //--------------------------------------------------
            // No Hacer Chequeo Sámantico Para  Los  Valores  Por
            // Defecto : 'int', 'string', 'nil', 'none', 'error'.
            //--------------------------------------------------
            this.IsOk = true;
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            //--------------------------------------------------
            // NADA!!!
            //--------------------------------------------------
        }
    }
}
