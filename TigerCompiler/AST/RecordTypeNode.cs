using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    class RecordTypeNode : TypeNode
    {
        public RecordTypeNode() { }

        public RecordTypeNode(IToken token) : base(token) { }

        public RecordTypeNode(RecordTypeNode node) : base(node) { }

        public FieldTypeNode[] Fields
        {
            get
            {
                if (this.Children == null)
                    return new FieldTypeNode[0];
                return this.Children.Cast<FieldTypeNode>().ToArray();
            }
        }

        public override void CheckSemantics(Scope scope, List<SemanticError> errors)
        {
            //--------------------------------------------------
            // Por Default, El Nodo No Tiene Errores.
            //--------------------------------------------------
            this.IsOk = true;

            //--------------------------------------------------
            // Buscar Campos Repetidos En La Declaración Del Record.
            //--------------------------------------------------
            var FieldName = this.Fields.Select(x => x.ID.Name).ToArray<string>();
            for (int i = 0; i < FieldName.Length; i++)
            {
                int j = 0;
                while (j < i && FieldName[i] != FieldName[j])
                    j = j + 1;
                if (j < i)
                {
                    errors.Add(SemanticError.PreviousFieldDeclaration(FieldName[i], this.Fields[i]));
                    this.IsOk = false;
                }
            }

            //--------------------------------------------------
            // Hacer 'CheckSemantics' Para Cada Campo.
            //--------------------------------------------------
            foreach (var field in this.Fields)
            {
                field.CheckSemantics(scope, errors);
                this.IsOk &= field.IsOk;
            }
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            //--------------------------------------------------
            // Si El TypeNode Está Registrado, No Lo Generamos.
            //--------------------------------------------------
            if (this.UnderlyingSystemType != null)
                return;

            //--------------------------------------------------
            // Definimos Una Clase. Que Tendrá La Siguiente Forma:
            //
            // class TYPE_i {
            //     public TYPE_t1 FIELD_1;
            //     public TYPE_t2 FIELD_1;
            //     ...
            //     public TYPE_tk FIELD_k;
            //
            //     public TYPE_i(TYPE_t1 A_1, TYPE_t2 A_2, ..., TYPE_tk A_k) {
            //         this.FIELD_1 = A_1;
            //         this.FIELD_2 = A_2;
            //         ...
            //         this.FIELD_k = A_k;
            //     }
            // }
            //--------------------------------------------------
            var typeBuilder = moduleBuilder.DefineType(string.Format("TYPE_{0}", ++TigerNode.TypesDefined));

            //--------------------------------------------------
            // Actualizamos El Tipo Para El TypeNode Actual.
            //--------------------------------------------------
            this.UnderlyingSystemType = typeBuilder.UnderlyingSystemType;

            //--------------------------------------------------
            // Generamos Los Tipos Para Los Campos Y Despues
            // Los Creamos.
            //--------------------------------------------------
            List<FieldBuilder> fieldInfo = new List<FieldBuilder>();
            foreach (var field in this.Fields)
            {
                field.VariableInfo.TypeNode.GenerateCode(moduleBuilder, program, generator);

                fieldInfo.Add(
                    typeBuilder.DefineField(
                        field.ID.Name,
                        field.VariableInfo.TypeNode.UnderlyingSystemType,
                        FieldAttributes.Public
                    )
               );
            }

            //--------------------------------------------------
            // Definir El Constructor.
            //--------------------------------------------------
            var constructorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.HasThis,
                this.Fields.Select(x => x.VariableInfo.TypeNode.UnderlyingSystemType).ToArray()
            );

            //--------------------------------------------------
            // Cuerpo Del Constructor.
            //--------------------------------------------------
            var constructorGenerator = constructorBuilder.GetILGenerator();

            constructorGenerator.Emit(OpCodes.Ldarg_0);
            constructorGenerator.Emit(OpCodes.Call, typeof(object).GetConstructors()[0]);

            constructorGenerator.Emit(OpCodes.Nop);

            for (int i = 0; i < this.Fields.Length; i++)
            {
                constructorGenerator.Emit(OpCodes.Ldarg_0);
                constructorGenerator.Emit(OpCodes.Ldarg_S, (byte)(i + 1));
                constructorGenerator.Emit(OpCodes.Stfld, fieldInfo[i]);
            }

            constructorGenerator.Emit(OpCodes.Ret);

            //--------------------------------------------------
            // Crea El Tipo.
            //--------------------------------------------------
            typeBuilder.CreateType();

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
