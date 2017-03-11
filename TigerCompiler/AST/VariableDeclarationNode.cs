using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    class VariableDeclarationNode : DeclarationNode
    {
        public VariableDeclarationNode() { }

        public VariableDeclarationNode(IToken token) : base(token) { }

        public VariableDeclarationNode(VariableDeclarationNode node) : base(node) { }

        public IdNode ID
        {
            get { return this.Children[0] as IdNode; }
        }

        public ExpressionNode Expression
        {
            get { return this.Children[1] as ExpressionNode; }
        }

        public VariableInfo VariableInfo { get; set; }

        public override void CheckSemantics(Scope scope, List<SemanticError> errors)
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
            // Hacer 'CheckSemantics' Al Valor De La Variable.
            //--------------------------------------------------
            this.Expression.CheckSemantics(scope, errors);

            //--------------------------------------------------
            // Si El Valor De La Expresión Tiene Algún Error, 
            // Parar De Reportar Errores.
            //--------------------------------------------------
            if (this.Expression.ExpressionType == PredefinedTypes.ErrorType)
                this.IsOk = false;

            if (!this.IsOk)
                return;

            if (this.ChildCount == 3)
            {
                //--------------------------------------------------
                // Si La Variable Tiene El Tipo Explícitamente Entonces
                // Es De La Siguiente Forma:
                //
                // var id : type-id := expr
                //--------------------------------------------------
                var typeID = this.Children[2] as IdNode;

                //--------------------------------------------------
                // Si El Tipo No Existe, Entonces Reportar El Error.
                //--------------------------------------------------
                var TI = scope.FindTypeInfo(typeID.Name);

                if (TI == null)
                {
                    errors.Add(SemanticError.TypeDoesNotExist(typeID.Name, this));
                    this.IsOk = false;
                    return;
                }

                if (this.Expression.ExpressionType == PredefinedTypes.NilType)
                {
                    //--------------------------------------------------
                    // Si El Valor De La Expresión Es <nil>, Entonces  El
                    // Tipo De La Variable, Escrito Explícitamente; Puede
                    // Ser Cualquiera, Excepto 'int'.
                    //
                    // var id : type-id := nil
                    //--------------------------------------------------
                    if (TI.TypeNode == PredefinedTypes.IntType)
                    {
                        errors.Add(SemanticError.InvalidIntNilAssignation(this));
                        this.IsOk = false;
                        return;
                    }
                }
                else if (TI.TypeNode != this.Expression.ExpressionType)
                {
                    //--------------------------------------------------
                    // Si Los Tipos Son Diferentes, Reportar Error.
                    //--------------------------------------------------
                    this.IsOk = false;
                    errors.Add(SemanticError.InvalidTypeConvertion(TI.TypeNode, this.Expression.ExpressionType, this));
                    return;
                }

                //--------------------------------------------------
                // Crear 'VariableInfo'.
                //--------------------------------------------------
                this.VariableInfo = new VariableInfo(this.ID.Name, TI.TypeNode);
            }
            else
            {
                //--------------------------------------------------
                // En Este Caso Se Omite El Tipo De La Variable, Por
                // Tanto Se Debe Inferir De La Parte Derecha. Se Debe
                // Comprobar Que La Parte Derecha No Sea <nil> o <void>.
                //
                // var id := expr
                //--------------------------------------------------
                if (this.Expression.ExpressionType == PredefinedTypes.NilType ||
                    this.Expression.ExpressionType == PredefinedTypes.VoidType)
                {
                    errors.Add(SemanticError.InvalidTypeInference(this.Expression.ExpressionType, this));
                    this.IsOk = false;
                    return;
                }

                //--------------------------------------------------
                // Crear 'VariableInfo'.
                //--------------------------------------------------
                this.VariableInfo = new VariableInfo(this.ID.Name, this.Expression.ExpressionType);
            }

            //--------------------------------------------------
            // Actualizar 'Scope' Con La Variable Actual.
            //--------------------------------------------------
            scope.Add(this.VariableInfo);
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            //--------------------------------------------------
            // Generar Código Para La Parte Derecha De La Asignación.
            //--------------------------------------------------
            this.Expression.GenerateCode(moduleBuilder, program, generator);

            //--------------------------------------------------
            // Declarar La Variable.
            //--------------------------------------------------
            this.VariableInfo.DeclareVariable(program);

            //--------------------------------------------------
            // Asignar La Variable.
            //--------------------------------------------------
            generator.Emit(OpCodes.Stsfld, this.VariableInfo.FieldBuilder);
        }
    }
}
