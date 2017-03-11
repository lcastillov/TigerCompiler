using System;
using Antlr.Runtime.Tree;
using Antlr.Runtime;
using TigerCompiler.Semantics;
using System.Reflection.Emit;
using System.Reflection;
using System.Collections.Generic;
using TigerCompiler.CodeGeneration;

namespace TigerCompiler.AST
{
    class ProgramNode : TigerNode
    {
        public ProgramNode() { }

        public ProgramNode(IToken token) : base(token) { }

        public ProgramNode(ProgramNode node) : base(node) { }

        public ExpressionNode Expression
        {
            get
            {
                return this.Children[0] as ExpressionNode;
            }
        }

        public override void CheckSemantics(Scope scope, System.Collections.Generic.List<Semantics.SemanticError> errors)
        {
            //--------------------------------------------------
            // Hacer 'CheckSemantics' A La Expresión Que Define
            // El Programa.
            //--------------------------------------------------
            this.Expression.CheckSemantics(scope, errors);
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            //--------------------------------------------------
            // Generate Stack Fields For Builtin Types (int & string).
            //--------------------------------------------------
            PredefinedTypes.IntType.VirtualStack1 = program.DefineField(
                string.Format("STACK_{0}", ++TigerNode.VariablesDeclared),
                typeof(Stack<>).MakeGenericType(new Type[] { typeof(int) }),
                FieldAttributes.Static
            );

            PredefinedTypes.IntType.VirtualStack2 = program.DefineField(
                string.Format("STACK_{0}", ++TigerNode.VariablesDeclared),
                typeof(Stack<>).MakeGenericType(new Type[] { typeof(int) }),
                FieldAttributes.Static
            );

            PredefinedTypes.StrType.VirtualStack1 = program.DefineField(
                string.Format("STACK_{0}", ++TigerNode.VariablesDeclared),
                typeof(Stack<>).MakeGenericType(new Type[] { typeof(string) }),
                FieldAttributes.Static
            );

            PredefinedTypes.StrType.VirtualStack2 = program.DefineField(
                string.Format("STACK_{0}", ++TigerNode.VariablesDeclared),
                typeof(Stack<>).MakeGenericType(new Type[] { typeof(string) }),
                FieldAttributes.Static
            );

            //--------------------------------------------------
            // [Stack Status]
            // 1. expression [Si 'expression' Retorna]
            //--------------------------------------------------
            this.Expression.GenerateCode(moduleBuilder, program, generator);

            //--------------------------------------------------
            // [Stack Status]
            //--------------------------------------------------
            if (this.Expression.ExpressionType != PredefinedTypes.VoidType)
            {
                generator.Emit(OpCodes.Pop);
            }

            //--------------------------------------------------
            // Main Return.
            //--------------------------------------------------
            generator.Emit(OpCodes.Ret);

            //--------------------------------------------------
            // Define Ctor For Program
            //--------------------------------------------------
            var ConstructorBuilder = program.DefineConstructor(MethodAttributes.SpecialName |
                                      MethodAttributes.Static |
                                      MethodAttributes.Private |
                                      MethodAttributes.HideBySig |
                                      MethodAttributes.RTSpecialName,
                                      CallingConventions.Standard, System.Type.EmptyTypes);

            var ConstructorGenerator = ConstructorBuilder.GetILGenerator();
            
            /* Stack1 - int */
            ConstructorGenerator.Emit(OpCodes.Newobj, GenericStackHelper.GetDefaultConstructor(typeof(int)));
            ConstructorGenerator.Emit(OpCodes.Stsfld, PredefinedTypes.IntType.VirtualStack1);

            /* Stack2 - int */
            ConstructorGenerator.Emit(OpCodes.Newobj, GenericStackHelper.GetDefaultConstructor(typeof(int)));
            ConstructorGenerator.Emit(OpCodes.Stsfld, PredefinedTypes.IntType.VirtualStack2);

            /* Stack1 - string */
            ConstructorGenerator.Emit(OpCodes.Newobj, GenericStackHelper.GetDefaultConstructor(typeof(string)));
            ConstructorGenerator.Emit(OpCodes.Stsfld, PredefinedTypes.StrType.VirtualStack1);

            /* Stack2 - string */
            ConstructorGenerator.Emit(OpCodes.Newobj, GenericStackHelper.GetDefaultConstructor(typeof(string)));
            ConstructorGenerator.Emit(OpCodes.Stsfld, PredefinedTypes.StrType.VirtualStack2);

            /* Stack1 & Stack2 - other types */
            foreach (var TI in Program.scope.GetDescendingTypeInfos())
                if (!(TI.TypeNode is AliasTypeNode || TI.TypeNode is BuiltinType)) {
                    /* Stack1 - <> */
                    ConstructorGenerator.Emit(OpCodes.Newobj,
                        GenericStackHelper.GetDefaultConstructor(TI.TypeNode.UnderlyingSystemType)
                    );
                    ConstructorGenerator.Emit(OpCodes.Stsfld, TI.TypeNode.VirtualStack1);
                    /* Stack2 - <> */
                    ConstructorGenerator.Emit(OpCodes.Newobj,
                        GenericStackHelper.GetDefaultConstructor(TI.TypeNode.UnderlyingSystemType)
                    );
                    ConstructorGenerator.Emit(OpCodes.Stsfld, TI.TypeNode.VirtualStack2);
                }

            ConstructorGenerator.Emit(OpCodes.Ret);
        }
    }
}

