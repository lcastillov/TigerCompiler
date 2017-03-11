using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    class ExpressionsBlockNode : AtomicNode
    {
        public ExpressionsBlockNode() : base() { }

        public ExpressionsBlockNode(IToken token) : base(token) { }

        public ExpressionsBlockNode(ExpressionsBlockNode node) : base(node) { }

        public ExpressionNode[] Expressions
        {
            get
            {
                if (this.Children == null)
                    return new ExpressionNode[0];
                return this.Children.Cast<ExpressionNode>().ToArray();
            }
        }

        public bool ContainsBreakNodes
        {
            get;
            set;
        }

        public override void CheckSemantics(Scope scope, List<SemanticError> errors)
        {
            this.ExpressionType = PredefinedTypes.ErrorType;

            //--------------------------------------------------
            // Aplicar 'CheckSematics' A Cada Expresión Hija Del
            // Bloque. Si Alguna Expresión Da Error,  El  Bloque
            // Da Error.
            //--------------------------------------------------
            bool IsOk = true;
            foreach (var expression in this.Expressions)
            {
                expression.CheckSemantics(scope, errors);
                if (expression.ExpressionType == PredefinedTypes.ErrorType)
                {
                    IsOk = false;
                }
            }

            if (IsOk)
            {
                //--------------------------------------------------
                // Si  Toda  Expresión  Está  OK, Entonces  El Tipo  De
                // Retorno Del Bloque Es El Tipo De La Expresión Final,
                // Si No  Hay Expresiones En  El  Bloque O Contiene Un
                // Break,  Entonces  El Tipo Final Es <void>.
                //--------------------------------------------------
                int S = this.Expressions.Length;
                if (S == 0 || this.ContainsBreakNodes)
                    this.ExpressionType = PredefinedTypes.VoidType;
                else
                    this.ExpressionType = this.Expressions[S - 1].ExpressionType;
            }
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            int n = Expressions.Length;
            for (int i = 0; i < n; i++) {
                this.Expressions[i].GenerateCode(moduleBuilder, program, generator);
                if (this.Expressions[i].ExpressionType != PredefinedTypes.VoidType)
                {
                    if (this.ExpressionType != PredefinedTypes.VoidType && i == n - 1)
                        continue;
                    generator.Emit(OpCodes.Pop);
                }
            }
        }
    }
}
