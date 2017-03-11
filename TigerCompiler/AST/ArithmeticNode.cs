using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    abstract class ArithmeticNode : BinaryNode
    {
        public ArithmeticNode() : base() { }

        public ArithmeticNode(IToken token) : base(token) { }

        public ArithmeticNode(ArithmeticNode node) : base(node) { }

        public abstract string OperatorName { get; }

        public abstract OpCode OperatorOpCode { get; }

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
                IsOk = false;
                errors.Add(SemanticError.InvalidUseOfBinaryArithmeticOperator(
                    this.OperatorName, "left", this.LeftOperandNode));
            }

            if (this.RightOperandNode.ExpressionType != PredefinedTypes.IntType)
            {
                IsOk = false;
                errors.Add(SemanticError.InvalidUseOfBinaryArithmeticOperator(
                    this.OperatorName, "right", this.RightOperandNode));
            }

            if (IsOk)
                this.ExpressionType = PredefinedTypes.IntType;
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            //--------------------------------------------------
            // [Stack Status]
            // 1. l_operand
            // (...)
            //--------------------------------------------------
            LeftOperandNode.GenerateCode(moduleBuilder, program, generator);

            //--------------------------------------------------
            // [Stack Status]
            // 1. r_operand
            // (...)
            //--------------------------------------------------
            RightOperandNode.GenerateCode(moduleBuilder, program, generator);

            //--------------------------------------------------
            // [Stack Status]
            // 1. l_operand [OperatorOpCode] r_operand
            // (...)
            //--------------------------------------------------
            generator.Emit(OperatorOpCode);
        }
    }
}
