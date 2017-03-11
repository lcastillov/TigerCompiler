using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    class StringNode : AtomicNode
    {
        public StringNode() { }

        public StringNode(IToken token) : base(token) { }

        public StringNode(StringNode node) : base(node) { }

        public string Value { get { return this.Text; } }

        public override void CheckSemantics(Scope scope, List<SemanticError> errors)
        {
            this.ExpressionType = PredefinedTypes.StrType;
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            generator.Emit(OpCodes.Ldstr, this.Value);
        }
    }
}
