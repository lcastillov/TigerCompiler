using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using TigerCompiler.CodeGeneration;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    class CallNode : AtomicNode
    {
        public CallNode() : base() { }

        public CallNode(IToken token) : base(token) { }

        public CallNode(CallNode node) : base(node) { }

        public IdNode ID
        {
            get
            {
                return this.Children[0] as IdNode;
            }
        }

        public IEnumerable<ExpressionNode> Arguments
        {
            get
            {
                return this.Children.Skip(1).Cast<ExpressionNode>();
            }
        }

        public FunctionInfo FunctionInfo { get; set; }

        public override void CheckSemantics(Scope scope, List<SemanticError> errors)
        {
            //--------------------------------------------------
            // Buscar El FunctionInfo Correspondiente.
            //--------------------------------------------------
            this.FunctionInfo = scope.FindFunctionInfo(ID.Name);

            //--------------------------------------------------
            // Poner Por Defecto Que Hay Errores.
            //--------------------------------------------------
            this.ExpressionType = PredefinedTypes.ErrorType;

            //--------------------------------------------------
            // Si La Función No Existe, Reportar Error.
            //--------------------------------------------------
            if (this.FunctionInfo == null)
            {
                errors.Add(SemanticError.FunctionDoesNotExist(this.ID.Name, this));
                return;
            }

            //--------------------------------------------------
            // Si La Cantidad De Argumentos Es Diferente A La
            // Cantidad De Parámetros, Reportar Error.
            //--------------------------------------------------
            var parameters = this.FunctionInfo.Parameters;
            if (this.Arguments.Count() != parameters.Length)
            {
                errors.Add(SemanticError.WrongParameterNumber(this.ID.Name, parameters.Length, this.Arguments.Count(), this));
                return;
            }

            //--------------------------------------------------
            // Hacer 'CheckSemantics' A Cada Argumento.
            //--------------------------------------------------
            bool IsOk = true;
            foreach (var argument in this.Arguments)
            {
                argument.CheckSemantics(scope, errors);
                if (argument.ExpressionType == PredefinedTypes.ErrorType)
                {
                    IsOk = false;
                }
            }

            //--------------------------------------------------
            // Si Hay Errores En Los Argumentos, Parar De
            // Reportar Errores.
            //--------------------------------------------------
            if (!IsOk)
                return;

            //--------------------------------------------------
            // Comprobar Que Los Tipos De Los Argumentos Son Iguales
            // A Los Tipos De Los Parámetros Correspondientes.
            //--------------------------------------------------
            for (int i = 0; i < parameters.Length; i++)
            {
                var arg = this.Arguments.ElementAt(i);
                if (!SemanticError.CompatibleTypes(parameters[i].TypeNode, arg.ExpressionType))
                {
                    errors.Add(SemanticError.InvalidTypeConvertion(parameters[i].TypeNode, arg.ExpressionType, arg));
                    IsOk = false;
                }
            }

            if (IsOk)
                this.ExpressionType = this.FunctionInfo.ReturnType;
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            if (FunctionInfo.IsStandard)
            {
                foreach (var Argument in this.Arguments)
                    Argument.GenerateCode(moduleBuilder, program, generator);
                generator.Emit(OpCodes.Call, this.FunctionInfo.MethodBuilder);
            }
            else
            {
                //--------------------------------------------------
                // Guarda Los Argumentos En El Stack Auxiliar.
                //--------------------------------------------------
                for (int i = 0; i < this.Arguments.Count(); i++)
                {
                    var Argument = this.Arguments.ElementAt(i);
                    generator.Emit(OpCodes.Ldsfld, Argument.ExpressionType.VirtualStack2);
                    Argument.GenerateCode(moduleBuilder, program, generator);
                    generator.Emit(OpCodes.Callvirt,
                            GenericStackHelper.GetMethod("Push", Argument.ExpressionType.UnderlyingSystemType)
                        );
                }

                //--------------------------------------------------
                // Salva El Valor De Las Variables Locales (Stack1)
                //--------------------------------------------------
                VariableInfo[] Locals = this.FunctionInfo.Scope.GetDescendingVariableInfos().ToArray();
                for (int i = 0; i < Locals.Length; i++)
                {
                    Locals[i].DeclareVariable(program);
                    generator.Emit(OpCodes.Ldsfld, Locals[i].TypeNode.VirtualStack1);
                    generator.Emit(OpCodes.Ldsfld, Locals[i].FieldBuilder);
                    generator.Emit(OpCodes.Callvirt,
                        GenericStackHelper.GetMethod("Push", Locals[i].TypeNode.UnderlyingSystemType)
                    );
                }

                //--------------------------------------------------
                // Traer De Stack2 Los Argumentos Para El LLamado De La Función.
                //--------------------------------------------------
                for (int i = this.Arguments.Count() - 1; i >= 0; i--)
                {
                    var Argument = this.Arguments.ElementAt(i);
                    generator.Emit(OpCodes.Ldsfld, Argument.ExpressionType.VirtualStack2);
                    generator.Emit(OpCodes.Callvirt,
                        GenericStackHelper.GetMethod("Pop", Argument.ExpressionType.UnderlyingSystemType)
                    );
                    generator.Emit(OpCodes.Stsfld, this.FunctionInfo.Parameters[i].FieldBuilder);
                }

                //--------------------------------------------------
                // [Stack Status]
                // 1. func( arg_1, arg_2, ..., arg_n ) [Si 'func' Retorna]
                // (...)
                //--------------------------------------------------
                generator.Emit(OpCodes.Call, this.FunctionInfo.MethodBuilder);

                LocalBuilder Ret = null;

                if (this.FunctionInfo.ReturnType != PredefinedTypes.VoidType)
                {
                    Ret = generator.DeclareLocal(this.FunctionInfo.ReturnType.UnderlyingSystemType);
                    generator.Emit(OpCodes.Stloc, Ret.LocalIndex);
                }

                for (int i = Locals.Length - 1; i >= 0; i--)
                {
                    generator.Emit(OpCodes.Ldsfld, Locals[i].TypeNode.VirtualStack1);
                    generator.Emit(OpCodes.Callvirt,
                        GenericStackHelper.GetMethod("Pop", Locals[i].TypeNode.UnderlyingSystemType)
                    );
                    generator.Emit(OpCodes.Stsfld, Locals[i].FieldBuilder);
                }

                if (this.FunctionInfo.ReturnType != PredefinedTypes.VoidType)
                {
                    generator.Emit(OpCodes.Ldloc, Ret.LocalIndex);
                }
            }
        }
    }
}
