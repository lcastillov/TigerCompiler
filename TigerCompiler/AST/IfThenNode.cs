using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    class IfThenNode : ExpressionNode
    {
        public IfThenNode() : base() { }

        public IfThenNode(IToken token) : base(token) { }

        public IfThenNode(IfThenNode node) : base(node) { }

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

        public override void CheckSemantics(Scope scope, List<SemanticError> errors)
        {
            //--------------------------------------------------
            // Hacer 'CheckSemantics' A Los Hijos.
            //--------------------------------------------------
            this.IfNode.CheckSemantics(scope, errors);
            this.ThenNode.CheckSemantics(scope, errors);

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
                this.ThenNode.ExpressionType == PredefinedTypes.ErrorType)
            {
                return;
            }

            //--------------------------------------------------
            // La Condición Debe Ser De Tipo <int>.
            //--------------------------------------------------
            if (this.IfNode.ExpressionType != PredefinedTypes.IntType)
            {
                errors.Add(SemanticError.InvalidTypeConvertion(PredefinedTypes.IntType,
                    this.IfNode.ExpressionType, this.IfNode
                ));
                return;
            }

            //--------------------------------------------------
            // El Cuerpo Del IfThen No Puede Devolver.
            //--------------------------------------------------
            if (this.ThenNode.ExpressionType != PredefinedTypes.VoidType)
            {
                errors.Add(SemanticError.ExpectedType(PredefinedTypes.VoidType,
                    this.ThenNode.ExpressionType, this.ThenNode
                ));
            }
            else
                this.ExpressionType = PredefinedTypes.VoidType;
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            var T = generator.DefineLabel();
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

            generator.Emit(OpCodes.Br, END);

            generator.MarkLabel(T);

            //--------------------------------------------------
            // [Stack Status]
            // 1. then_expression
            // (...)
            //--------------------------------------------------
            this.ThenNode.GenerateCode(moduleBuilder, program, generator);

            generator.Emit(OpCodes.Br, END);

            generator.MarkLabel(END);
        }
    }
}
