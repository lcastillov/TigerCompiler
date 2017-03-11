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
    class AssignNode : ExpressionNode
    {
        public AssignNode() { }

        public AssignNode(IToken token) : base(token) { }

        public AssignNode(AssignNode node) : base(node) { }

        public AccessNode LeftVariable
        {
            get
            {
                return this.Children[0] as AccessNode;
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
            //--------------------------------------------------
            // Hacer 'CheckSemantics' A Los Hijos.
            //--------------------------------------------------
            this.LeftVariable.CheckSemantics(scope, errors);
            this.Value.CheckSemantics(scope, errors);

            //--------------------------------------------------
            // Poner Por Defecto Que Hay Errores.
            //--------------------------------------------------
            this.ExpressionType = PredefinedTypes.ErrorType;

            //--------------------------------------------------
            // Si Ha Ocurrido Un Error En Alguno De Los Hijos,
            // Parar De Reportar Errores.
            //--------------------------------------------------
            if (this.LeftVariable.ExpressionType == PredefinedTypes.ErrorType ||
                this.Value.ExpressionType == PredefinedTypes.ErrorType)
            {
                return;
            }

            if (this.LeftVariable.ReadOnly)
            {
                errors.Add(SemanticError.InvalidUseOfAssignmentToAReadonlyVariable(this.LeftVariable));
                return;
            }

            if (!SemanticError.CompatibleTypes(this.LeftVariable.ExpressionType, this.Value.ExpressionType))
            {
                errors.Add(SemanticError.ExpectedType(this.LeftVariable.ExpressionType,
                    this.Value.ExpressionType, this));
            }
            else
                this.ExpressionType = PredefinedTypes.VoidType;
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            this.LeftVariable.GenerateCode2(
                moduleBuilder, program, generator, this.Value, true
            );
        }
    }
}
