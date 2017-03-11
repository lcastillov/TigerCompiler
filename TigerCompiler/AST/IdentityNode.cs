using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    abstract class IdentityNode : RelationalNode
    {
        public IdentityNode() { }
        
        public IdentityNode(IToken token) : base(token) { }

        public IdentityNode(IdentityNode node) : base(node) { }

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
            // Se Pueden Comparar Solamente Expresiones Del
            // Mismo Tipo. Tener Cuidado De:
            //
            // <void> ( = , != ) <void>
            // <null> ( = , != ) <null>
            //--------------------------------------------------
            if (this.LeftOperandNode.ExpressionType == this.RightOperandNode.ExpressionType)
            {
                if (this.LeftOperandNode.ExpressionType == PredefinedTypes.VoidType ||
                    this.LeftOperandNode.ExpressionType == PredefinedTypes.NilType)
                {
                    errors.Add(SemanticError.InvalidCompareOperation(this.LeftOperandNode.ExpressionType,
                        this.RightOperandNode.ExpressionType, this));
                    return;
                }
            }

            if (SemanticError.CompatibleTypes(this.LeftOperandNode.ExpressionType, this.RightOperandNode.ExpressionType) ||
                SemanticError.CompatibleTypes(this.RightOperandNode.ExpressionType, this.LeftOperandNode.ExpressionType))
            {
                this.ExpressionType = PredefinedTypes.IntType;
            }
            else
            {
                errors.Add(SemanticError.InvalidCompareOperation(this.LeftOperandNode.ExpressionType,
                        this.RightOperandNode.ExpressionType, this));
            }

            //--------------------------------------------------
            // 
            //--------------------------------------------------
        }
    }
}
