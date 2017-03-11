using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    class FieldValueNode : ExpressionNode
    {
        public FieldValueNode() { }

        public FieldValueNode(IToken token) : base(token) { }

        public FieldValueNode(FieldValueNode node) : base(node) { }

        public IdNode ID
        {
            get
            {
                return this.Children[0] as IdNode;
            }
        }

        public ExpressionNode Value
        {
            get
            {
                return this.Children[1] as ExpressionNode;
            }
        }

        public override void CheckSemantics(Scope scope, List<SemanticError> errors)
        {
            this.Value.CheckSemantics(scope, errors);
            this.ExpressionType = this.Value.ExpressionType;
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            throw new NotImplementedException();
        }
    }
}
