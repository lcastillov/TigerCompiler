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
    class LSBNode : AccessNode
    {
        public LSBNode() { }

        public LSBNode(IToken token) : base(token) { }

        public LSBNode(LSBNode node) : base(node) { }

        public AccessNode LeftVariable
        {
            get
            {
                return this.Children[0] as AccessNode;
            }
        }

        public ExpressionNode Index
        {
            get
            {
                return this.Children[1] as ExpressionNode;
            }
        }

        public override void CheckSemantics(Semantics.Scope scope, List<Semantics.SemanticError> errors)
        {
            //--------------------------------------------------
            // Hacer 'CheckSemantics' A Los Hijos.
            //--------------------------------------------------
            this.LeftVariable.CheckSemantics(scope, errors);
            this.Index.CheckSemantics(scope, errors);

            //--------------------------------------------------
            // ...
            //--------------------------------------------------
            this.ReadOnly = this.LeftVariable.ReadOnly;

            //--------------------------------------------------
            // Poner Por Defecto Que Hay Errores.
            //--------------------------------------------------
            this.ExpressionType = PredefinedTypes.ErrorType;


            //--------------------------------------------------
            // Si Ha Ocurrido Un Error En Alguno De Los Hijos,
            // Parar De Reportar Errores.
            //--------------------------------------------------
            if (this.LeftVariable.ExpressionType == PredefinedTypes.ErrorType ||
                this.Index.ExpressionType == PredefinedTypes.ErrorType)
            {
                return;
            }

            var AT = this.LeftVariable.ExpressionType as ArrayTypeNode;

            //--------------------------------------------------
            // Si La Variable No Es Un Array, Reportar Error.
            //--------------------------------------------------
            if (AT == null)
            {
                errors.Add(SemanticError.InvalidIndexingOperation(this.LeftVariable.ExpressionType, this));
                return;
            }

            //--------------------------------------------------
            // 'Index' Debe Ser De Tipo <int>
            //--------------------------------------------------
            if (this.Index.ExpressionType != PredefinedTypes.IntType)
            {
                errors.Add(SemanticError.InvalidArrayAccess(this.Index));
            }
            else
            {
                this.ExpressionType = AT.ArrayOf;
            }
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            //--------------------------------------------------
            // [Stack Status]
            // 1. left_value
            // (...)
            //--------------------------------------------------
            this.LeftVariable.GenerateCode(moduleBuilder, program, generator);

            //--------------------------------------------------
            // [Stack Status]
            // 2. index
            // 1. left_value
            // (...)
            //--------------------------------------------------
            this.Index.GenerateCode(moduleBuilder, program, generator);

            //--------------------------------------------------
            // [Stack Status]
            // 1. left_value[index]
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Ldelem, this.ExpressionType.UnderlyingSystemType);
        }

        public override void GenerateCode2(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator, ExpressionNode value, bool isRoot)
        {
            //--------------------------------------------------
            // [Stack Status]
            // 1. left_value
            // (...)
            //--------------------------------------------------
            this.LeftVariable.GenerateCode2(moduleBuilder, program, generator, value, false);

            //--------------------------------------------------
            // [Stack Status]
            // 2. index
            // 1. left_value
            // (...)
            //--------------------------------------------------
            this.Index.GenerateCode(moduleBuilder, program, generator);

            if (isRoot)
            {
                //--------------------------------------------------
                // [Stack Status]
                // 3. value
                // 2. index
                // 1. left_value
                // (...)
                //--------------------------------------------------
                value.GenerateCode(moduleBuilder, program, generator);

                generator.Emit(OpCodes.Stelem, this.ExpressionType.UnderlyingSystemType);
            }
            else
            {

                //--------------------------------------------------
                // [Stack Status]
                // 1. left_value[index]
                // (...)
                //--------------------------------------------------
                generator.Emit(OpCodes.Ldelem, this.ExpressionType.UnderlyingSystemType);
            }
        }
    }
}
