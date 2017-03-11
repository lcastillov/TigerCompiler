using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    class TypeDeclarationNode : DeclarationNode
    {
        public TypeDeclarationNode() { }

        public TypeDeclarationNode(IToken token) : base(token) { }

        public TypeDeclarationNode(TypeDeclarationNode node) : base(node) { }

        public IdNode ID
        {
            get { return this.Children[0] as IdNode; }
        }

        public TypeNode TypeNode
        {
            get { return this.Children[1] as TypeNode; }
        }

        public override void CheckSemantics(Scope scope, List<SemanticError> errors)
        {
            //--------------------------------------------------
            // Hacer  'CheckSemantics'   Solamente  A   La  Parte
            // Derecha  ( TypeNode )  De  La Declaración De Tipo,
            // Porque El Bloque Padre De La Declaración Actual Se
            // Encargó De Comprobar La  Parte  Izquierda  ( ID ).
            //--------------------------------------------------
            this.TypeNode.CheckSemantics(scope, errors);
            //--------------------------------------------------
            // Establecer IsOk Al Mismo Valor Del IsOk De 'TypeNode'.
            //--------------------------------------------------
            this.IsOk = this.TypeNode.IsOk;
            //--------------------------------------------------
            // Si 'TypeNode' Es Un Record O Un Array, Actualizar
            // El Nombre Del Tipo.
            //--------------------------------------------------
            if (this.TypeNode is RecordTypeNode || this.TypeNode is ArrayTypeNode)
                this.TypeNode.Name = this.ID.Name;
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            //--------------------------------------------------
            // Generar Código Solamente Para El TypeNode, Porque
            // La Generación De Este No Depende Del Nombre Del
            // Que Lo Pusieron El El Programa, Es Decir, ID No
            // Hace Falta Para Definir Al Tipo.
            //--------------------------------------------------
            this.TypeNode.GenerateCode(moduleBuilder, program, generator);
        }
    }
}
