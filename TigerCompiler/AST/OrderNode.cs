using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    abstract class OrderNode : RelationalNode
    {
        public OrderNode() { }

        public OrderNode(IToken token) : base(token) { }

        public OrderNode(OrderNode node) : base(node) { }

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
            // Solamente Se Pueden Comprar Enteros Y Cadenas.
            //--------------------------------------------------
            if (this.LeftOperandNode.ExpressionType != this.RightOperandNode.ExpressionType)
            {
                errors.Add(SemanticError.InvalidCompareOperation(this.LeftOperandNode.ExpressionType,
                    this.RightOperandNode.ExpressionType, this));
            }
            else
            {
                if (this.LeftOperandNode.ExpressionType != PredefinedTypes.IntType &&
                    this.LeftOperandNode.ExpressionType != PredefinedTypes.StrType)
                {
                    errors.Add(SemanticError.InvalidCompareOperation(this.LeftOperandNode.ExpressionType,
                    this.RightOperandNode.ExpressionType, this));
                }
                else
                {
                    this.ExpressionType = PredefinedTypes.IntType;
                }
            }
        }
    }
}
