using Antlr.Runtime;
using System;
using System.Reflection.Emit;

namespace TigerCompiler.AST
{
    class NilNode : AtomicNode
    {
        public NilNode() : base() { }

        public NilNode(IToken token) : base(token) { }

        public NilNode(NilNode node) : base(node) { }

        public override void CheckSemantics(Semantics.Scope scope, System.Collections.Generic.List<Semantics.SemanticError> errors)
        {
            this.ExpressionType = PredefinedTypes.NilType;
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            generator.Emit(OpCodes.Ldnull);
        }
    }
}
