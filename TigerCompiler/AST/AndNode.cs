using Antlr.Runtime;
using System.Reflection.Emit;

namespace TigerCompiler.AST
{
    class AndNode : LogicNode
    {
        public AndNode() : base() { }

        public AndNode(IToken token) : base(token) { }

        public AndNode(AndNode node) : base(node) { }

        public override void GenerateCode(ModuleBuilder moduleBuilder, System.Reflection.Emit.TypeBuilder program, ILGenerator generator)
        {
            var END = generator.DefineLabel();
            var T = generator.DefineLabel(); /* true */
            var F = generator.DefineLabel(); /* false */

            //--------------------------------------------------
            // [Stack Status]
            // 1. l_operand
            // (...)
            //--------------------------------------------------
            this.LeftOperandNode.GenerateCode(moduleBuilder, program, generator);

            //--------------------------------------------------
            // [Stack Status]
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Brfalse, F);

            //--------------------------------------------------
            // [Stack Status]
            // 1. r_operand
            // (...)
            //--------------------------------------------------
            this.RightOperandNode.GenerateCode(moduleBuilder, program, generator);

            //--------------------------------------------------
            // [Stack Status]
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Brfalse, F);

            generator.MarkLabel(T);

            //--------------------------------------------------
            // [Stack Status]
            // 1. 1 { true }
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Ldc_I4, 1);

            generator.Emit(OpCodes.Br, END);

            generator.MarkLabel(F);

            //--------------------------------------------------
            // [Stack Status]
            // 1. 0 { false }
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Ldc_I4, 0);

            generator.MarkLabel(END);
        }
    }
}
