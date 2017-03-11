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
    class PeriodNode : AccessNode
    {
        public PeriodNode() { }

        public PeriodNode(IToken token) : base(token) { }

        public PeriodNode(PeriodNode node) : base(node) { }

        public AccessNode LeftVariable
        {
            get
            {
                return this.Children[0] as AccessNode;
            }
        }

        public IdNode Field
        {
            get
            {
                return this.Children[1] as IdNode;
            }
        }

        public override void CheckSemantics(Scope scope, List<SemanticError> errors)
        {
            //--------------------------------------------------
            // Hacer 'CheckSemantics' Al Hijo.
            //--------------------------------------------------
            this.LeftVariable.CheckSemantics(scope, errors);

            //--------------------------------------------------
            // ...
            //--------------------------------------------------
            this.ReadOnly = this.LeftVariable.ReadOnly;

            //--------------------------------------------------
            // Poner Por Defecto Que Hay Errores.
            //--------------------------------------------------
            this.ExpressionType = PredefinedTypes.ErrorType;

            //--------------------------------------------------
            // Si Ha Ocurrido Un Error, Parar De Reportar Errores.
            //--------------------------------------------------
            if (LeftVariable.ExpressionType == PredefinedTypes.ErrorType)
            {
                return;
            }

            var RT = this.LeftVariable.ExpressionType as RecordTypeNode;

            //--------------------------------------------------
            // Si La Variable No Es Un Record, Reportar Error.
            //--------------------------------------------------
            if (RT == null)
            {
                errors.Add(SemanticError.InvalidAccessToAField(this.Field.Text,
                    this.LeftVariable.ExpressionType, this));
                return;
            }

            //--------------------------------------------------
            // Buscar El Nombre Del Campo ...
            //--------------------------------------------------
            var FT = RT.Fields.FirstOrDefault(x => x.ID.Name == this.Field.Name);

            if (FT == null)
            {
                errors.Add(SemanticError.InvalidAccessToAField(this.Field.Text,
                    this.LeftVariable.ExpressionType, this
                ));
            }
            else
            {
                this.ExpressionType = FT.VariableInfo.TypeNode;
            }
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            //--------------------------------------------------
            // [Stack Status]
            // 1. left_value
            // (...)
            //--------------------------------------------------
            this.LeftVariable.GenerateCode(moduleBuilder, program, generator);

            //--------------------------------------------------
            // [Stack Status]
            // 1. left_value.field
            // (...)
            //--------------------------------------------------
            generator.Emit(
                OpCodes.Ldfld,
                this.LeftVariable.ExpressionType.UnderlyingSystemType.GetField(this.Field.Name)
            );
        }

        public override void GenerateCode2(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator, ExpressionNode value, bool isRoot)
        {
            //--------------------------------------------------
            // [Stack Status]
            // 1. left_value
            // (...)
            //--------------------------------------------------
            this.LeftVariable.GenerateCode2(moduleBuilder, program, generator, value, false);

            if (isRoot)
            {
                //--------------------------------------------------
                // [Stack Status]
                // 2. value
                // 1. left_value
                // (...)
                //--------------------------------------------------
                value.GenerateCode(moduleBuilder, program, generator);

                //--------------------------------------------------
                // [Stack Status]
                // (...)
                //--------------------------------------------------
                generator.Emit(OpCodes.Stfld,
                    this.LeftVariable.ExpressionType.UnderlyingSystemType.GetField(this.Field.Name));
            }
            else
            {
                //--------------------------------------------------
                // [Stack Status]
                // 1. left_value.field
                // (...)
                //--------------------------------------------------
                generator.Emit(
                    OpCodes.Ldfld,
                    this.LeftVariable.ExpressionType.UnderlyingSystemType.GetField(this.Field.Name)
                );
            }
        }
    }
}
