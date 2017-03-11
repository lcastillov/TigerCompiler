using Antlr.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    class ArrayTypeNode : TypeNode
    {
        public ArrayTypeNode() { }

        public ArrayTypeNode(IToken token) : base(token) { }

        public ArrayTypeNode(ArrayTypeNode node) : base(node) { }

        public IdNode TypeID 
        {
            get
            {
                return this.Children[0] as IdNode;
            }
        }

        public TypeNode ArrayOf { get; set; }

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
            // Generamos El Tipo De Los Elementos Del Array.
            //--------------------------------------------------
            this.ArrayOf.GenerateCode(moduleBuilder, program, generator);

            //--------------------------------------------------
            // Creamos El Tipo Para El Array.
            //--------------------------------------------------
            this.UnderlyingSystemType = this.ArrayOf.UnderlyingSystemType.MakeArrayType();

            //--------------------------------------------------
            // Creamos Un Stack Para Este Tipo
            //--------------------------------------------------
            this.VirtualStack1 = program.DefineField(
                string.Format("STACK_{0}", ++TigerNode.VariablesDeclared),
                typeof(Stack<>).MakeGenericType(new Type[] { this.UnderlyingSystemType }),
                FieldAttributes.Static
            );

            this.VirtualStack2 = program.DefineField(
                string.Format("STACK_{0}", ++TigerNode.VariablesDeclared),
                typeof(Stack<>).MakeGenericType(new Type[] { this.UnderlyingSystemType }),
                FieldAttributes.Static
            );
        }
    }
}
