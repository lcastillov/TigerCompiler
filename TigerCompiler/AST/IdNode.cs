using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    class IdNode : AccessNode
    {
        public IdNode() { }

        public IdNode(IToken token) : base(token) { }

        public IdNode(IdNode node) : base(node) { }

        public string Name { get { return this.Text; } }

        public VariableInfo VariableInfo { get; set; }

        public override void CheckSemantics(Scope scope, List<SemanticError> errors)
        {
            //--------------------------------------------------
            // A 'IdNode' Se Le Hace 'CheckSemantics' Solamente
            // Cuando Es Usado Como 'AccessNode'.
            //--------------------------------------------------
            this.VariableInfo = scope.FindVariableInfo(this.Name);

            //--------------------------------------------------
            // Si No Existe Una Variable En El Scope Visible
            // Actual Que Corresponda Al Nombre Del 'ID' Actual,
            // Informamos Error.
            //--------------------------------------------------
            if (this.VariableInfo == null)
            {
                errors.Add(SemanticError.UndefinedVariableUsed(this.Name, this));
                this.ExpressionType = PredefinedTypes.ErrorType;
            }
            else
            {
                this.ExpressionType = this.VariableInfo.TypeNode;
                this.ReadOnly = this.VariableInfo.ReadOnly;
            }
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            //--------------------------------------------------
            // [Stack Status]
            // 1. static_variable
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Ldsfld, this.VariableInfo.FieldBuilder);
        }

        public override void GenerateCode2(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator, ExpressionNode value, bool isRoot)
        {
            if (isRoot)
            {
                //--------------------------------------------------
                // [Stack Status]
                // 1. value
                // (...)
                //--------------------------------------------------
                value.GenerateCode(moduleBuilder, program, generator);

                //--------------------------------------------------
                // [Stack Status]
                // (...)
                //--------------------------------------------------
                generator.Emit(OpCodes.Stsfld, this.VariableInfo.FieldBuilder);
            }
            else
            {
                //--------------------------------------------------
                // [Stack Status]
                // 1. static_variable
                // (...)
                //--------------------------------------------------
                generator.Emit(OpCodes.Ldsfld, this.VariableInfo.FieldBuilder);
            }
        }
    }
}
