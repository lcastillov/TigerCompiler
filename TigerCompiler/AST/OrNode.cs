using Antlr.Runtime;
using System.Reflection.Emit;

namespace TigerCompiler.AST
{
    class OrNode : LogicNode
    {
        public OrNode() : base() { }

        public OrNode(IToken token) : base(token) { }

        public OrNode(OrNode node) : base(node) { }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
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
            generator.Emit(OpCodes.Brtrue, T);

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
            generator.Emit(OpCodes.Brtrue, T);

            generator.Emit(OpCodes.Br, F);

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
