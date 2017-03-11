using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    class AliasTypeNode : TypeNode
    {
        public AliasTypeNode() { }

        public AliasTypeNode(IToken token) : base(token) { }

        public AliasTypeNode(AliasTypeNode node) : base(node) { }

        public IdNode TypeID
        {
            get
            {
                return this.Children[0] as IdNode;
            }
        }

        public TypeNode AliasOf { get; set; }

        public override void CheckSemantics(Scope scope, List<SemanticError> errors)
        {
            this.IsOk = true;
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            //--------------------------------------------------
            // Si El TypeNode Está Registrado, No Lo Generamos.
            //--------------------------------------------------
            if (this.UnderlyingSystemType != null)
                return;

            //--------------------------------------------------
            // Generamos Código Para El Alias.
            //--------------------------------------------------
            this.AliasOf.GenerateCode(moduleBuilder, program, generator);

            //--------------------------------------------------
            // Los Tipos Son Los Mismos.
            //--------------------------------------------------
            this.UnderlyingSystemType = this.AliasOf.UnderlyingSystemType;

            //--------------------------------------------------
            // Tomamos El Mismo Stack Para Este Tipo.
            //--------------------------------------------------
            this.VirtualStack1 = this.AliasOf.VirtualStack1;
            this.VirtualStack2 = this.AliasOf.VirtualStack2;
        }
    }
}
