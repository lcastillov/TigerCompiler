using Antlr.Runtime;
using Antlr.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    class FunctionDeclarationNode : DeclarationNode
    {
        public FunctionDeclarationNode() { }

        public FunctionDeclarationNode(IToken token) : base(token) { }

        public FunctionDeclarationNode(FunctionDeclarationNode node) : base(node) { }

        public IdNode ID
        {
            get
            {
                return this.Children[0] as IdNode;
            }
        }

        public FieldTypeNode[] Parameters
        {
            get
            {
                if (((CommonTree)this.Children[1]).Children == null)
                    return new FieldTypeNode[0];
                return ((CommonTree)this.Children[1]).Children.Cast<FieldTypeNode>().ToArray();
            }
        }

        public ExpressionNode Body
        {
            get
            {
                return this.Children[2] as ExpressionNode;
            }
        }

        public IdNode TypeID { get; set; }

        public FunctionInfo FunctionInfo { get; set; }

        public override void CheckSemantics(Semantics.Scope scope, List<Semantics.SemanticError> errors)
        {
            //--------------------------------------------------
            // Por Default, El Nodo No Tiene Errores.
            //--------------------------------------------------
            this.IsOk = true;

            //--------------------------------------------------
            // Si Existe Una Función O Una Variable Con El Mismo
            // Nombre En El Scope Local, Reportar Error.
            //--------------------------------------------------
            if (scope.FindLocalFunctionInfo(this.ID.Name) != null || scope.FindLocalVariableInfo(this.ID.Name) != null)
            {
                errors.Add(SemanticError.PreviousVariableOrFunctionDeclaration(this.ID.Name, this));
                this.IsOk = false;
            }

            //--------------------------------------------------
            // Buscar Parámetros Repetidos En La Declaración De La Función.
            //--------------------------------------------------
            var ParameterName = this.Parameters.Select(x => x.ID.Name).ToArray<string>();
            for (int i = 0; i < ParameterName.Length; i++)
            {
                int j = 0;
                while (j < i && ParameterName[i] != ParameterName[j])
                    j = j + 1;
                if (j < i)
                {
                    errors.Add(SemanticError.PreviousParameterDeclaration(ParameterName[i], this.ID.Name, this.Parameters[i]));
                    this.IsOk = false;
                }
            }

            //--------------------------------------------------
            // Hacer 'CheckSemantics' A Los Parámetros...
            //--------------------------------------------------
            foreach (var parameter in this.Parameters)
            {
                parameter.CheckSemantics(scope, errors);
                this.IsOk &= parameter.IsOk;
            }

            if (!this.IsOk)
                return;

            //--------------------------------------------------
            // Crea El 'FunctionInfo' Correspondiente A La Función
            // Actual. Notar Que El Tipo De Retorno Se Pone En <none>.
            //--------------------------------------------------
            this.FunctionInfo = new FunctionInfo(
                this.ID.Name,
                PredefinedTypes.VoidType,
                this.Parameters.Select(x => x.VariableInfo).ToArray()
            );

            //--------------------------------------------------
            // Si La Función Tiene El Tipo De Retorno Explícitamente
            // Entonces Es De La Siguiente Forma:
            //
            // function foo( parameters ) : type-id = expr
            //
            // ... Y Podemos Actualizar El 'FunctionInfo';
            //--------------------------------------------------
            if (this.ChildCount == 4)
            {
                var typeID = this.Children[3] as IdNode;

                //--------------------------------------------------
                // Si El Tipo No Existe, Entonces Reportar El Error.
                //--------------------------------------------------
                var TI = scope.FindTypeInfo(typeID.Name);

                if (TI == null)
                {
                    errors.Add(SemanticError.TypeDoesNotExist(typeID.Name, typeID));
                    this.IsOk = false;
                    return;
                }

                //--------------------------------------------------
                // Actualizar El Tipo De Retorno De La Función.
                //--------------------------------------------------
                this.FunctionInfo.ReturnType = TI.TypeNode;
            }

            //--------------------------------------------------
            // Actualizar El Scope Actual.
            //--------------------------------------------------
            scope.Add(this.FunctionInfo);
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            for (int i = 0; i < this.Parameters.Length; i++)
            {
                this.Parameters[i].VariableInfo.DeclareVariable(program);
            }
            
            this.Body.GenerateCode(moduleBuilder, program, generator);

            generator.Emit(OpCodes.Ret);
        }
    }
}
