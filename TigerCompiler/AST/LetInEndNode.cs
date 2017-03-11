using Antlr.Runtime;
using Antlr.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    class LetInEndNode : ExpressionNode
    {
        public LetInEndNode() { }

        public LetInEndNode(IToken token) : base(token) { }

        public LetInEndNode(LetInEndNode node) : base(node) { }

        public IEnumerable<BlockDeclarationsNode> Declarations
        {
            get
            {
                return ((CommonTree)this.Children[0]).Children.Cast<BlockDeclarationsNode>();
            }
        }

        public ExpressionsBlockNode Expressions
        {
            get
            {
                return this.GetChild(1) as ExpressionsBlockNode;
            }
        }

        public override void CheckSemantics(Scope scope, List<SemanticError> errors)
        {
            //--------------------------------------------------
            // Poner El Valor De Retorno De La Expresión A 'Error'
            // Por Default.
            //--------------------------------------------------
            this.ExpressionType = PredefinedTypes.ErrorType;

            //--------------------------------------------------
            // Crear Un Nuevo Scope Incluyendo Las Declaraciones
            // Del LetInEnd Actual.
            //--------------------------------------------------
            Scope InnerScope = scope.CreateChildScope();

            //--------------------------------------------------
            // Si Algún Bloque De Declaraciones Produce Error,
            // Parar La Comprobación Para Los restantes.
            //--------------------------------------------------
            foreach (var declaration in Declarations)
            {
                declaration.CheckSemantics(InnerScope, errors);
                if (!declaration.IsOk)
                {
                    return;
                }
            }

            //--------------------------------------------------
            // Hacer 'CheckSemantics' Al Bloque De Expresiones.
            //--------------------------------------------------
            this.Expressions.CheckSemantics(InnerScope, errors);

            //--------------------------------------------------
            // Si Hay Errores, Parar De Reportar.
            //--------------------------------------------------
            if (this.Expressions.ExpressionType == PredefinedTypes.ErrorType)
            {
                return;
            }

            //--------------------------------------------------
            // ...
            //--------------------------------------------------
            this.ExpressionType = this.Expressions.ExpressionType;

            //--------------------------------------------------
            // El Tipo De La Expresión Debe Ser Visible En El
            // Scope Padre.
            //--------------------------------------------------
            if (!(this.ExpressionType is BuiltinType))
            {
                var TI = scope.FindTypeInfo(this.ExpressionType.Name);
                if (TI == null || !TI.TypeNode.Equals(this.ExpressionType))
                {
                    errors.Add(SemanticError.TypeNoLongerVisible(this.ExpressionType, this));
                    this.ExpressionType = PredefinedTypes.ErrorType;
                }
            }
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            //--------------------------------------------------
            // Generar Código Para Las Declaraciones.
            //--------------------------------------------------
            foreach (var declaration in this.Declarations)
                declaration.GenerateCode(moduleBuilder, program, generator);

            //--------------------------------------------------
            // Generar Código Para Las Expresiones
            //--------------------------------------------------
            this.Expressions.GenerateCode(moduleBuilder, program, generator);
        }
    }
}
