using Antlr.Runtime;
using System;
using System.Reflection.Emit;

namespace TigerCompiler.AST
{
    class EqNode : IdentityNode
    {
        public EqNode() { }
        
        public EqNode(IToken token) : base(token) { }

        public EqNode(EqNode node) : base(node) { }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            //--------------------------------------------------
            // [Stack Status]
            // 1. l_operand
            // (...)
            //--------------------------------------------------
            this.LeftOperandNode.GenerateCode(moduleBuilder, program, generator);

            //--------------------------------------------------
            // [Stack Status]
            // 2. r_operand
            // 1. l_operand
            // (...)
            //--------------------------------------------------
            this.RightOperandNode.GenerateCode(moduleBuilder, program, generator);
            
            if (this.LeftOperandNode.ExpressionType == PredefinedTypes.StrType ||
                this.RightOperandNode.ExpressionType == PredefinedTypes.StrType)
            {
                var LBL = generator.DefineLabel();
                var END = generator.DefineLabel();
                generator.Emit(OpCodes.Call,
                    typeof(String).GetMethod("Compare", new System.Type[] { typeof(string), typeof(string) }));
                generator.Emit(OpCodes.Brfalse, LBL); // 1 Si c = 0
                generator.Emit(OpCodes.Ldc_I4, 0);
                generator.Emit(OpCodes.Br, END);
                generator.MarkLabel(LBL);
                generator.Emit(OpCodes.Ldc_I4, 1);
                generator.MarkLabel(END);
            }
            else
            {
                generator.Emit(OpCodes.Ceq);
            }
        }
    }
}
