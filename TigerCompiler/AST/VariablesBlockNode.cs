using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    class VariablesBlockNode : BlockDeclarationsNode
    {
        public VariablesBlockNode() { }

        public VariablesBlockNode(IToken token) : base(token) { }

        public VariablesBlockNode(VariablesBlockNode node) : base(node) { }

        public IEnumerable<VariableDeclarationNode> Declarations
        {
            get
            {
                return this.Children.Cast<VariableDeclarationNode>();
            }
        }

        public override void CheckSemantics(Scope scope, List<SemanticError> errors)
        {
            //--------------------------------------------------
            // Por Default, El Nodo No Tiene Errores.
            //--------------------------------------------------
            this.IsOk = true;

            //--------------------------------------------------
            // Hacer 'CheckSemantics' A Todas Las Declaraciones
            // De Variables Del Bloque Actual. IsOk Es Falso, Si
            // IsOk Es Falso En Alguna Declaración Hija.
            //--------------------------------------------------
            foreach (var declaration in Declarations)
            {
                declaration.CheckSemantics(scope, errors);
                this.IsOk &= declaration.IsOk;
            }
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            foreach (var declaration in this.Declarations)
                declaration.GenerateCode(moduleBuilder, program, generator);
        }
    }
}
