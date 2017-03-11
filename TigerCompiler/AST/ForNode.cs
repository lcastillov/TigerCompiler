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
    class ForNode : ExpressionNode, IBreakableNode
    {
        public ForNode() { }

        public ForNode(IToken token) : base(token) { }

        public ForNode(ForNode node) : base(node) { }

        public List<Label> Labels
        {
            get;
            set;
        }

        public IdNode ID
        {
            get
            {
                return this.Children[0] as IdNode;
            }
        }

        public ExpressionNode LoNode
        {
            get
            {
                return this.Children[1] as ExpressionNode;
            }
        }

        public ExpressionNode HiNode
        {
            get
            {
                return this.Children[2] as ExpressionNode;
            }
        }

        public ExpressionNode BodyNode
        {
            get
            {
                return this.Children[3] as ExpressionNode;
            }
        }

        public bool ContainsBreakNodes
        {
            get;
            set;
        }

        public override void CheckSemantics(Scope scope, List<SemanticError> errors)
        {
            //--------------------------------------------------
            // Hacer 'CheckSemantics' A Los Hijos.
            //--------------------------------------------------
            this.LoNode.CheckSemantics(scope, errors);
            this.HiNode.CheckSemantics(scope, errors);

            //--------------------------------------------------
            // Poner El Valor De Retorno De La Expresión A 'Error'
            // Por Default.
            //--------------------------------------------------
            this.ExpressionType = PredefinedTypes.ErrorType;

            //--------------------------------------------------
            // Si Ha Ocurrido Un Error En Alguno De Los Hijos,
            // Parar De Reportar Errores.
            //--------------------------------------------------
            if (this.LoNode.ExpressionType == PredefinedTypes.ErrorType ||
                this.HiNode.ExpressionType == PredefinedTypes.ErrorType)
            {
                return;
            }

            //--------------------------------------------------
            // Ambos Nodos, LoNode & HiNode, Deben Ser De Tipo <int>.
            //--------------------------------------------------
            if (this.LoNode.ExpressionType != PredefinedTypes.IntType ||
                this.HiNode.ExpressionType != PredefinedTypes.IntType)
            {
                if (this.LoNode.ExpressionType != PredefinedTypes.IntType)
                    errors.Add(SemanticError.ExpectedType(PredefinedTypes.IntType, this.LoNode.ExpressionType, this.LoNode));
                if (this.HiNode.ExpressionType != PredefinedTypes.IntType)
                    errors.Add(SemanticError.ExpectedType(PredefinedTypes.IntType, this.HiNode.ExpressionType, this.HiNode));
                return;
            }

            //--------------------------------------------------
            // Definir Un Nuevo Scope Con La Variable De Iteración
            // Incluída.
            //--------------------------------------------------
            this.ID.VariableInfo = new VariableInfo(this.ID.Name, PredefinedTypes.IntType, true);

            Scope InnerScope = scope.CreateChildScope();

            InnerScope.Add(this.ID.VariableInfo);

            //--------------------------------------------------
            // Hacer 'CheckSemantics' En El Cuerpo Del For.
            //--------------------------------------------------
            this.BodyNode.CheckSemantics(InnerScope, errors);

            //--------------------------------------------------
            // Si Hay Errores En El Cuerpo Del For, Parar De
            // Reportar Errores.
            //--------------------------------------------------
            if (this.BodyNode.ExpressionType == PredefinedTypes.ErrorType)
            {
                return;
            }

            //--------------------------------------------------
            // El Cuerpo Del For No Puede Devolver Valor.
            //--------------------------------------------------
            if (this.BodyNode.ExpressionType != PredefinedTypes.VoidType)
            {
                errors.Add(SemanticError.ExpectedType(PredefinedTypes.VoidType,
                    this.BodyNode.ExpressionType, this));
            }
            else
                this.ExpressionType = PredefinedTypes.VoidType;
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            Labels = new List<Label>();

            this.ID.VariableInfo.DeclareVariable(program);

            var hi = generator.DeclareLocal(typeof(int));

            var STA = generator.DefineLabel();
            var END = generator.DefineLabel();

            //--------------------------------------------------
            // [Stack Status]
            // 2. hi
            // 1. lo
            // (...)
            //--------------------------------------------------
            this.LoNode.GenerateCode(moduleBuilder, program, generator);
            this.HiNode.GenerateCode(moduleBuilder, program, generator);

            //--------------------------------------------------
            // [Stack Status]
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Stloc, hi.LocalIndex);
            generator.Emit(OpCodes.Stsfld, this.ID.VariableInfo.FieldBuilder);

            generator.MarkLabel(STA);

            //--------------------------------------------------
            // [Stack Status]
            // 2. hi
            // 1. lo
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Ldsfld, this.ID.VariableInfo.FieldBuilder);
            generator.Emit(OpCodes.Ldloc, hi.LocalIndex);

            //--------------------------------------------------
            // [Stack Status]
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Bgt, END);

            this.BodyNode.GenerateCode(moduleBuilder, program, generator);

            //--------------------------------------------------
            // [Stack Status]
            // 2. 1
            // 1. lo
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Ldsfld, this.ID.VariableInfo.FieldBuilder);
            generator.Emit(OpCodes.Ldc_I4, 1);

            //--------------------------------------------------
            // [Stack Status]
            // 1. lo + 1
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Add);

            //--------------------------------------------------
            // [Stack Status]
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Stsfld, this.ID.VariableInfo.FieldBuilder);

            generator.Emit(OpCodes.Br, STA);

            generator.MarkLabel(END);

            foreach (var label in this.Labels)
                generator.MarkLabel(label);
        }
    }
}
