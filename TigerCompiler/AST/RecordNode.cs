using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    class RecordNode : ExpressionNode
    {
        public RecordNode() { }

        public RecordNode(IToken token) : base(token) { }

        public RecordNode(RecordNode node) : base(node) { }

        public IdNode TypeID
        {
            get
            {
                return this.Children[0] as IdNode;
            }
        }

        public FieldValueNode[] Fields
        {
            get
            {
                if (this.ChildCount == 1)
                    return new FieldValueNode[0];
                return this.Children.Skip(1).Cast<FieldValueNode>().ToArray();
            }
        }

        public override void CheckSemantics(Scope scope, List<SemanticError> errors)
        {
            //--------------------------------------------------
            // Poner El Valor De Retorno De La Expresión A 'Error'
            // Por Default.
            //--------------------------------------------------
            this.ExpressionType = PredefinedTypes.ErrorType;

            //--------------------------------------------------
            // Buscar Por El Tipo Del Record.
            //--------------------------------------------------
            var TI = scope.FindTypeInfo(this.TypeID.Name);

            //--------------------------------------------------
            // Si El Tipo No Está Definido, Reportar Error.
            //--------------------------------------------------
            if (TI == null)
            {
                errors.Add(SemanticError.TypeDoesNotExist(this.TypeID.Name, this.TypeID));
                return;
            }

            var RecordType = TI.TypeNode as RecordTypeNode;

            //--------------------------------------------------
            // Si El Tipo No Es Un Record.
            //--------------------------------------------------
            if (RecordType == null)
            {
                errors.Add(SemanticError.RecordTypeExpected(TI.TypeNode, this.TypeID));
                return;
            }

            //--------------------------------------------------
            // La Cantidad De Campos Debe Ser La Misma.
            //--------------------------------------------------
            if (this.Fields.Length != RecordType.Fields.Length)
            {
                errors.Add(SemanticError.WrongFieldNumberInRecord(RecordType.Name,
                    RecordType.Fields.Length, this.Fields.Length, this));
                return;
            }

            //--------------------------------------------------
            // Hacer 'CheckSemantics' A Los Campos Del Record.
            //--------------------------------------------------
            bool IsOk = true;
            foreach (var field in this.Fields)
            {
                field.CheckSemantics(scope, errors);
                if (field.ExpressionType == PredefinedTypes.ErrorType)
                {
                    IsOk = false;
                }
            }

            if (!IsOk)
                return;

            //--------------------------------------------------
            // Comprobar El Orden De Los Campos Y Los Tipos.
            //--------------------------------------------------
            for (int i = 0; i < this.Fields.Length; i++)
            {
                if (this.Fields[i].ID.Name != RecordType.Fields[i].ID.Name)
                {
                    IsOk = false;
                    errors.Add(SemanticError.WrongFieldNameInRecord(
                        this.Fields[i].ID.Name, RecordType.Fields[i].ID.Name, this.Fields[i]
                    ));
                }
                if (!SemanticError.CompatibleTypes(RecordType.Fields[i].VariableInfo.TypeNode, this.Fields[i].ExpressionType))
                {
                    IsOk = false;
                    errors.Add(SemanticError.ExpectedType(
                        RecordType.Fields[i].VariableInfo.TypeNode, this.Fields[i].ExpressionType, this.Fields[i]
                    ));
                }
            }

            if (IsOk)
                this.ExpressionType = RecordType;
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            //--------------------------------------------------
            // [Stack Status]
            // n. field_n
            // ...
            // 2. field_2
            // 1. field_1
            // (...)
            //--------------------------------------------------
            for (int i = 0; i < this.Fields.Length; i++)
                this.Fields[i].Value.GenerateCode(moduleBuilder, program, generator);

            //--------------------------------------------------
            // [Stack Status]
            // 1. new TYPE_i( field_1, field2, ..., field_n )
            // (...)
            //--------------------------------------------------
            generator.Emit(
                OpCodes.Newobj,
                this.ExpressionType.UnderlyingSystemType.GetConstructors()[0]
           );
        }
    }
}
