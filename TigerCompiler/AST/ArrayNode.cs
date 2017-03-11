using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    class ArrayNode : ExpressionNode
    {
        public ArrayNode() { }

        public ArrayNode(IToken token) : base(token) { }

        public ArrayNode(ArrayNode node) : base(node) { }

        public IdNode TypeID
        {
            get
            {
                return this.Children[0] as IdNode;
            }
        }

        public ExpressionNode Size
        {
            get
            {
                return this.Children[1] as ExpressionNode;
            }
        }

        public ExpressionNode Value
        {
            get
            {
                return this.Children[2] as ExpressionNode;
            }
        }

        public override void CheckSemantics(Scope scope, List<SemanticError> errors)
        {
            //--------------------------------------------------
            // Hacer 'CheckSemantics' A Los Hijos.
            //--------------------------------------------------
            this.Size.CheckSemantics(scope, errors);
            this.Value.CheckSemantics(scope, errors);

            //--------------------------------------------------
            // Poner El Valor De Retorno De La Expresión A 'Error'
            // Por Default.
            //--------------------------------------------------
            this.ExpressionType = PredefinedTypes.ErrorType;

            //--------------------------------------------------
            // Si Ha Ocurrido Un Error En Alguno De Los Hijos,
            // Parar De Reportar Errores.
            //--------------------------------------------------
            if (this.Size.ExpressionType == PredefinedTypes.ErrorType ||
                this.Value.ExpressionType == PredefinedTypes.ErrorType)
            {
                return;
            }

            //--------------------------------------------------
            // Buscar Por El Tipo Del Array
            //--------------------------------------------------
            var TI = scope.FindTypeInfo(this.TypeID.Name);

            //--------------------------------------------------
            // Si El Tipo No Está Definido, Reportar Error.
            //--------------------------------------------------
            if (TI == null)
            {
                errors.Add(SemanticError.TypeDoesNotExist(this.TypeID.Name, this.TypeID));
                return;
            }

            var ArrayType = TI.TypeNode as ArrayTypeNode;

            //--------------------------------------------------
            // Si El Tipo No Es Un Array.
            //--------------------------------------------------
            if (ArrayType == null)
            {
                errors.Add(SemanticError.ArrayTypeExpected(TI.TypeNode, this.TypeID));
                return;
            }

            //--------------------------------------------------
            // 'Size' Debe Ser De Tipo <int>.
            //--------------------------------------------------
            if (this.Size.ExpressionType != PredefinedTypes.IntType)
            { 
                errors.Add(SemanticError.ExpectedType(PredefinedTypes.IntType,
                    this.Size.ExpressionType, this.Size));
                return;
            }

            //--------------------------------------------------
            // 'Value' Debe Ser Del Mismo Tipo Que El Array.
            //--------------------------------------------------
            if (!SemanticError.CompatibleTypes(ArrayType.ArrayOf, this.Value.ExpressionType))
            {
                errors.Add(SemanticError.ExpectedType(ArrayType.ArrayOf, 
                    this.Value.ExpressionType, this.Value));
                return;
            }

            this.ExpressionType = ArrayType;
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            var a = generator.DeclareLocal(this.ExpressionType.UnderlyingSystemType);
            var i = generator.DeclareLocal(typeof(int));
            var n = generator.DeclareLocal(typeof(int));
            var sLoop = generator.DefineLabel();
            var eLoop = generator.DefineLabel();
            
            //--------------------------------------------------
            // [Stack Status]
            // 1. 0
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Ldc_I4, 0);

            //--------------------------------------------------
            // [Stack Status]
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Stloc, i.LocalIndex);

            //--------------------------------------------------
            // [Stack Status]
            // 1. Size
            // (...)
            //--------------------------------------------------
            this.Size.GenerateCode(moduleBuilder, program, generator);

            //--------------------------------------------------
            // [Stack Status]
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Stloc, n.LocalIndex);

            //--------------------------------------------------
            // [Stack Status]
            // 1. Size
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Ldloc, n.LocalIndex);

            //--------------------------------------------------
            // [Stack Status]
            // 1. Array
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Newarr,
                (this.ExpressionType as ArrayTypeNode).ArrayOf.UnderlyingSystemType
            );

            //--------------------------------------------------
            // [Stack Status]
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Stloc, a.LocalIndex);

            generator.MarkLabel(sLoop);

            //--------------------------------------------------
            // [Stack Status]
            // 2. Size
            // 1. i
            //--------------------------------------------------
            generator.Emit(OpCodes.Ldloc, i.LocalIndex);
            generator.Emit(OpCodes.Ldloc, n.LocalIndex);

            //--------------------------------------------------
            // [Stack Status]
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Bge, eLoop);

            //--------------------------------------------------
            // [Stack Status]
            // 1. Array
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Ldloc, a.LocalIndex);

            //--------------------------------------------------
            // [Stack Status]
            // 2. i
            // 1. Array
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Ldloc, i.LocalIndex);

            //--------------------------------------------------
            // [Stack Status]
            // 3. Value
            // 2. i
            // 1. Array
            // (...)
            //--------------------------------------------------
            this.Value.GenerateCode(moduleBuilder, program, generator);

            //--------------------------------------------------
            // [Stack Status]
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Stelem, (this.ExpressionType as ArrayTypeNode).ArrayOf.UnderlyingSystemType);

            //--------------------------------------------------
            // [Stack Status]
            // 1. i
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Ldloc, i.LocalIndex);

            //--------------------------------------------------
            // [Stack Status]
            // 2. 1
            // 1. i
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Ldc_I4, 1);

            //--------------------------------------------------
            // [Stack Status]
            // 1. i + 1
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Add);

            //--------------------------------------------------
            // [Stack Status]
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Stloc, i.LocalIndex);

            //--------------------------------------------------
            // [Stack Status]
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Br, sLoop);

            generator.MarkLabel(eLoop);

            //--------------------------------------------------
            // [Stack Status]
            // 1. Array
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Ldloc, a.LocalIndex);
        }
    }
}
