using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    class IfThenElseNode : ExpressionNode
    {
        public IfThenElseNode() : base() { }

        public IfThenElseNode(IToken token) : base(token) { }

        public IfThenElseNode(IfThenElseNode node) : base(node) { }

        public ExpressionNode IfNode
        {
            get
            {
                return this.Children[0] as ExpressionNode;
            }
        }

        public ExpressionNode ThenNode
        {
            get
            {
                return this.Children[1] as ExpressionNode;
            }
        }

        public ExpressionNode ElseNode
        {
            get
            {
                return this.Children[2] as ExpressionNode;
            }
        }

        public override void CheckSemantics(Scope scope, List<SemanticError> errors)
        {
            //--------------------------------------------------
            // Hacer 'CheckSemantics' A Los Hijos.
            //--------------------------------------------------
            this.IfNode.CheckSemantics(scope, errors);
            this.ThenNode.CheckSemantics(scope, errors);
            this.ElseNode.CheckSemantics(scope, errors);

            //--------------------------------------------------
            // Poner El Valor De Retorno De La Expresión A 'Error'
            // Por Default.
            //--------------------------------------------------
            this.ExpressionType = PredefinedTypes.ErrorType;

            //--------------------------------------------------
            // Si Ha Ocurrido Un Error En Alguno De Los Hijos,
            // Parar De Reportar Errores.
            //--------------------------------------------------
            if (this.IfNode.ExpressionType == PredefinedTypes.ErrorType ||
                this.ThenNode.ExpressionType == PredefinedTypes.ErrorType ||
                this.ElseNode.ExpressionType == PredefinedTypes.ErrorType)
            {
                return;
            }

            //--------------------------------------------------
            // La Condición Debe Ser De Tipo <int>.
            //--------------------------------------------------
            if (this.IfNode.ExpressionType != PredefinedTypes.IntType)
            {
                errors.Add(SemanticError.InvalidTypeConvertion(PredefinedTypes.IntType, this.IfNode.ExpressionType, this.IfNode));
            }
            else
            {
                //--------------------------------------------------
                // Debe Haber Conversión Entre El Tipo Del 'Then' Y
                // Del 'Else'.
                //--------------------------------------------------
                if (SemanticError.CompatibleTypes(this.ThenNode.ExpressionType, this.ElseNode.ExpressionType))
                    this.ExpressionType = this.ThenNode.ExpressionType;
                else if (SemanticError.CompatibleTypes(this.ElseNode.ExpressionType, this.ThenNode.ExpressionType))
                    this.ExpressionType = this.ElseNode.ExpressionType;
                else
                {
                    errors.Add(SemanticError.IncompatibleTypesInIfThenElse(this));
                }
            }
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            var T = generator.DefineLabel();
            var F = generator.DefineLabel();
            var END = generator.DefineLabel();

            //--------------------------------------------------
            // [Stack Status]
            // 1. condition
            // (...)
            //--------------------------------------------------
            this.IfNode.GenerateCode(moduleBuilder, program, generator);

            //--------------------------------------------------
            // [Stack Status]
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Brtrue, T);

            generator.Emit(OpCodes.Br, F);

            generator.MarkLabel(T);

            //--------------------------------------------------
            // [Stack Status]
            // 1. then_expression
            // (...)
            //--------------------------------------------------
            this.ThenNode.GenerateCode(moduleBuilder, program, generator);

            generator.Emit(OpCodes.Br, END);

            generator.MarkLabel(F);

            //--------------------------------------------------
            // [Stack Status]
            // 1. else_expression
            // (...)
            //--------------------------------------------------
            this.ElseNode.GenerateCode(moduleBuilder, program, generator);

            generator.MarkLabel(END);
        }
    }
}
