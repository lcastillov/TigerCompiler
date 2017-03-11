using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    class FieldTypeNode : UtilNode
    {
        public FieldTypeNode() { }

        public FieldTypeNode(IToken token) : base(token) { }

        public FieldTypeNode(FieldTypeNode node) : base(node) { }

        public IdNode ID
        {
            get
            {
                return this.Children[0] as IdNode;
            }
        }

        public IdNode TypeID
        {
            get
            {
                return this.Children[1] as IdNode;
            }
        }

        public VariableInfo VariableInfo { get; set; }

        public override void CheckSemantics(Scope scope, List<SemanticError> errors)
        {
            //--------------------------------------------------
            // Buscar En El Scope Actual El TypeInfo De TypeID.
            //--------------------------------------------------
            var TI = scope.FindTypeInfo(this.TypeID.Name);

            //--------------------------------------------------
            // Si El Tipo No Existe,  Entonces  Reportar El Error,
            // En Caso  Contrario,  Crear  Un  'VariableInfo' Para
            // El Campo Actual Con Nombre ID Y Tipo TI. Actualizar
            // IsOk En Cada Caso.
            //--------------------------------------------------
            if (TI == null)
            {
                errors.Add(SemanticError.TypeDoesNotExist(this.TypeID.Name, this.TypeID));
                this.IsOk = false;
            }
            else
            {
                this.VariableInfo = new VariableInfo(ID.Name, TI.TypeNode);
                this.IsOk = true;
            }
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            throw new System.NotImplementedException();
        }
    }
}
