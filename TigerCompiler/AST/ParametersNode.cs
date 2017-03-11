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
    class ParametersNode : UtilNode
    {
        public ParametersNode() { }

        public ParametersNode(IToken token) : base(token) { }

        public ParametersNode(ParametersNode node) : base(node) { }

        public override void CheckSemantics(Scope scope, List<SemanticError> errors)
        {
            //--------------------------------------------------
            // No Hacer Chequeo Sámantico, FunctionDeclarationNode
            // Se Encarga.
            //--------------------------------------------------
            this.IsOk = true;
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            throw new NotImplementedException();
        }
    }
}
