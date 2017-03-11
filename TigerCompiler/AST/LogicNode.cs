using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    abstract class LogicNode : BinaryNode
    {
        public LogicNode() : base() { }

        public LogicNode(IToken token) : base(token) { }

        public LogicNode(LogicNode node) : base(node) { }

        public override void CheckSemantics(Scope scope, List<SemanticError> errors)
        {
            //--------------------------------------------------
            // Hacer 'CheckSemantics' A Los Dos Hijos.
            //--------------------------------------------------
            this.LeftOperandNode.CheckSemantics(scope, errors);
            this.RightOperandNode.CheckSemantics(scope, errors);

            //--------------------------------------------------
            // Poner El Valor De Retorno De La Expresión A 'Error'
            // Por Default.
            //--------------------------------------------------
            this.ExpressionType = PredefinedTypes.ErrorType;

            //--------------------------------------------------
            // Si Ha Ocurrido Un Error En Alguno De Los Hijos,
            // Parar De Reportar Errores.
            //--------------------------------------------------
            if (this.LeftOperandNode.ExpressionType == PredefinedTypes.ErrorType ||
                this.RightOperandNode.ExpressionType == PredefinedTypes.ErrorType)
            {
                return;
            }

            //--------------------------------------------------
            // Comprobar Que Ambas Expresiones Devuelvan 'int'.
            //--------------------------------------------------
            bool IsOk = true;
            if (this.LeftOperandNode.ExpressionType != PredefinedTypes.IntType)
            {
                errors.Add(SemanticError.InvalidUseOfBinaryLogicalOperator("left", this));
                IsOk = false;
            }

            if (this.RightOperandNode.ExpressionType != PredefinedTypes.IntType)
            {
                errors.Add(SemanticError.InvalidUseOfBinaryLogicalOperator("right", this));
                IsOk = false;
            }

            if (IsOk)
                this.ExpressionType = PredefinedTypes.IntType;
        }
    }
}
