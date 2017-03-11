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
    class BreakNode : ExpressionNode
    {
        public BreakNode() { }

        public BreakNode(IToken token) : base(token) { }

        public BreakNode(BreakNode node) : base(node) { }

        public IBreakableNode Owner { get; set; }

        public override void CheckSemantics(Semantics.Scope scope, List<Semantics.SemanticError> errors)
        {
            //--------------------------------------------------
            // Poner Por Defecto Que Hay Errores.
            //--------------------------------------------------
            this.ExpressionType = PredefinedTypes.ErrorType;

            //--------------------------------------------------
            // Buscar El Primer Nodo Hacia La Raiz Que Acepte
            // Break.
            //--------------------------------------------------
            this.Owner = null;

            var pathToTheRoot = this.GetAncestors().Reverse();
            foreach (var u in pathToTheRoot) {
                if (u is IBreakableNode) {
                    this.Owner = u as IBreakableNode;
                    break;
                }
                if (u is FunctionDeclarationNode)
                {
                    break;
                }
                if (u is ExpressionsBlockNode)
                {
                    (u as ExpressionsBlockNode).ContainsBreakNodes = true;
                }
            }

            //--------------------------------------------------
            // Comprobar Si El Break Está Siendo Usado Fuera
            // De  Un  For,  While, O Bloque De Expresiones.
            //--------------------------------------------------
            if (this.Owner == null)
                errors.Add(SemanticError.InvalidUseOfBreak(this));
            else
            {
                this.ExpressionType = PredefinedTypes.VoidType;
            }
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            var LBL = generator.DefineLabel();

            this.Owner.Labels.Add(LBL);

            generator.Emit(OpCodes.Br, LBL);
        }
    }
}
