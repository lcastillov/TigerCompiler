using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    class NegativeNode : UnaryNode
    {
        public NegativeNode() : base() { }

        public NegativeNode(IToken token) : base(token) { }

        public NegativeNode(NegativeNode node) : base(node) { }

        public override void CheckSemantics(Scope scope, List<SemanticError> errors)
        {
            //--------------------------------------------------
            // Hacer 'CheckSemantics' Al Hijo.
            //--------------------------------------------------
            this.OperandNode.CheckSemantics(scope, errors);

            //--------------------------------------------------
            // Poner El Valor De Retorno De La Expresión A 'Error'
            // Por Default.
            //--------------------------------------------------
            this.ExpressionType = PredefinedTypes.ErrorType;

            //--------------------------------------------------
            // Si Ha Ocurrido Un Error En El Hijo, Parar De
            // Reportar Errores.
            //--------------------------------------------------
            if (this.OperandNode.ExpressionType == PredefinedTypes.ErrorType)
                return;

            //--------------------------------------------------
            // Comprobar Que El Tipo De La Expresión Del Hijo Sea <int>.
            //--------------------------------------------------
            if (this.OperandNode.ExpressionType == PredefinedTypes.IntType)
                this.ExpressionType = PredefinedTypes.IntType;
            else
            {
                errors.Add(SemanticError.InvalidUseOfUnaryMinusOperator(this));
            }
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            //--------------------------------------------------
            // [Stack Status]
            // 1. operand_node
            // (...)
            //--------------------------------------------------
            this.OperandNode.GenerateCode(moduleBuilder, program, generator);

            //--------------------------------------------------
            // [Stack Status]
            // 1. -operand_node
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Neg);
        }
    }
}
