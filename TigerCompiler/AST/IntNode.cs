using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    class IntNode : AtomicNode
    {
        public IntNode() : base() { }

        public IntNode(IToken token) : base(token) { }

        public IntNode(IntNode node) : base(node) { }

        public int Value { get { return int.Parse(this.Text); } }

        public override void CheckSemantics(Scope scope, List<SemanticError> errors)
        {
            int value;
            if (!int.TryParse(this.Text, out value))
            {
                errors.Add(SemanticError.InvalidNumber(this.Text, this));
                this.ExpressionType = PredefinedTypes.ErrorType;
            }
            else
            {
                this.ExpressionType = PredefinedTypes.IntType;
            }
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            generator.Emit(OpCodes.Ldc_I4, this.Value);
        }
    }
}
