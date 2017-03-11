using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    class WhileNode : ExpressionNode, IBreakableNode
    {
        public WhileNode() { }

        public WhileNode(IToken token) : base(token) { }

        public WhileNode(WhileNode node) : base(node) { }

        public List<Label> Labels
        {
            get;
            set;
        }

        public ExpressionNode ConditionNode
        {
            get
            {
                return this.Children[0] as ExpressionNode;
            }
        }

        public ExpressionNode BodyNode
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
            this.ConditionNode.CheckSemantics(scope, errors);
            this.BodyNode.CheckSemantics(scope, errors);

            //--------------------------------------------------
            // Poner El Valor De Retorno De La Expresión A 'Error'
            // Por Default.
            //--------------------------------------------------
            this.ExpressionType = PredefinedTypes.ErrorType;

            //--------------------------------------------------
            // Si Ha Ocurrido Un Error En Alguno De Los Hijos,
            // Parar De Reportar Errores.
            //--------------------------------------------------
            if (this.ConditionNode.ExpressionType == PredefinedTypes.ErrorType ||
                this.BodyNode.ExpressionType == PredefinedTypes.ErrorType)
            {
                return;
            }

            //--------------------------------------------------
            // La Condición Del While Debe Ser De Tipo <int>.
            //--------------------------------------------------
            if (this.ConditionNode.ExpressionType != PredefinedTypes.IntType)
            {
                errors.Add(SemanticError.ExpectedType(PredefinedTypes.IntType,
                    this.ConditionNode.ExpressionType, this.ConditionNode
                ));
                return;
            }

            //--------------------------------------------------
            // El Cuerpo Del While No Puede Devolver.
            //--------------------------------------------------
            if (this.BodyNode.ExpressionType != PredefinedTypes.VoidType)
            {
                errors.Add(SemanticError.ExpectedType(PredefinedTypes.VoidType,
                    this.BodyNode.ExpressionType, this.BodyNode
                ));
            }
            else
                this.ExpressionType = PredefinedTypes.VoidType;
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            Labels = new List<Label>();

            var STA = generator.DefineLabel(); /* start */
            var END = generator.DefineLabel(); /* end */

            generator.MarkLabel(STA);

            //--------------------------------------------------
            // [Stack Status]
            // 1. condition
            // (...)
            //--------------------------------------------------
            this.ConditionNode.GenerateCode(moduleBuilder, program, generator);

            generator.Emit(OpCodes.Brfalse, END);

            this.BodyNode.GenerateCode(moduleBuilder, program, generator);

            generator.Emit(OpCodes.Br, STA);

            generator.MarkLabel(END);

            foreach (var label in this.Labels)
                generator.MarkLabel(label);
        }
    }
}
