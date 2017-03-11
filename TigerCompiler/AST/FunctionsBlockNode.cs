using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    class FunctionsBlockNode : BlockDeclarationsNode
    {
        public FunctionsBlockNode() { }

        public FunctionsBlockNode(IToken token) : base(token) { }

        public FunctionsBlockNode(FunctionsBlockNode node) : base(node) { }

        public IEnumerable<FunctionDeclarationNode> Declarations
        {
            get
            {
                return this.Children.Cast<FunctionDeclarationNode>();
            }
        }

        public override void CheckSemantics(Scope scope, List<SemanticError> errors)
        {
            //--------------------------------------------------
            // Por Default, El Nodo No Tiene Errores.
            //--------------------------------------------------
            this.IsOk = true;

            //--------------------------------------------------
            // Hacer  'CheckSemantics'  A  Cada  Función.
            //--------------------------------------------------
            foreach (var declaration in this.Declarations)
            {
                declaration.CheckSemantics(scope, errors);
                this.IsOk &= declaration.IsOk;
            }

            if (!this.IsOk)
                return;

            //--------------------------------------------------
            // Hacer 'CheckSemantics' Al Cuerpo De Las Funciones.
            // Esto Es Una Segunda Pasada.
            //--------------------------------------------------
            foreach (var declaration in this.Declarations)
            {
                Scope InnerScope = scope.CreateChildScope();
                foreach (var parameter in declaration.FunctionInfo.Parameters)
                    InnerScope.Add(parameter);

                //--------------------------------------------------
                // Mantener La Referencia Del Scope Definido En
                // El Function Info.
                //--------------------------------------------------
                declaration.FunctionInfo.Scope = InnerScope;

                declaration.Body.CheckSemantics(InnerScope, errors);

                //--------------------------------------------------
                // Parar De Reportar Errores Si El Cuerpo De La Función
                // Tiene Error.
                //--------------------------------------------------
                if (declaration.Body.ExpressionType == PredefinedTypes.ErrorType)
                    declaration.IsOk = false;

                this.IsOk &= declaration.IsOk;

                if (!declaration.IsOk)
                    continue;

                //--------------------------------------------------
                // Comprobar  Que  El  Tipo  De  Retorno De La Función
                // Sea Igual Al Del Cuerpo De La Misma. Comprobar Tres
                // Casos:
                // 1 - La Función No Devuelve & El Cuerpo Devuelve
                // 2 - La Función Devuelve & El Cuerpo No Devuelve
                // 3 - La Función Y El Cuerpo Devuelven Pero Tipos Incompatibles.
                //   - Tener En Cuenta El Caso <object_type> = <nil>
                //--------------------------------------------------
                if (declaration.FunctionInfo.ReturnType == PredefinedTypes.VoidType)
                {
                    //--------------------------------------------------
                    // La Función Es Un Procedimiento ( No Devuelve )...
                    //--------------------------------------------------
                    if (declaration.Body.ExpressionType != PredefinedTypes.VoidType)
                    {
                        errors.Add(SemanticError.ProcedureCannotReturn(declaration.ID.Name, declaration));
                        declaration.IsOk = false;
                    }
                }
                else
                {
                    //--------------------------------------------------
                    // La Función Devuelve ...
                    //--------------------------------------------------
                    if (declaration.Body.ExpressionType == PredefinedTypes.VoidType)
                    {
                        errors.Add(SemanticError.FunctionMustReturn(declaration.ID.Name, declaration));
                        declaration.IsOk = false;
                    }
                    else
                    {
                        if (declaration.Body.ExpressionType == PredefinedTypes.NilType)
                        {
                            if (declaration.FunctionInfo.ReturnType == PredefinedTypes.IntType)
                            {
                                errors.Add(SemanticError.InvalidIntNilAssignation(declaration));
                                declaration.IsOk = false;
                            }
                        }
                        else if (declaration.Body.ExpressionType != declaration.FunctionInfo.ReturnType)
                        {
                            errors.Add(SemanticError.InvalidTypeConvertion(declaration.FunctionInfo.ReturnType, declaration.Body.ExpressionType, declaration));
                            declaration.IsOk = false;
                        }
                    }
                }

                this.IsOk &= declaration.IsOk;
            }
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            List<MethodBuilder> FunctionBuilders = new List<MethodBuilder>();
            foreach (var functionDeclaration in this.Declarations)
            {
                MethodBuilder MB = program.DefineMethod(
                    string.Format("FUNCTION_{0}", ++TigerNode.FunctionsDeclared),
                    MethodAttributes.Static, functionDeclaration.FunctionInfo.ReturnType.UnderlyingSystemType,
                    System.Type.EmptyTypes);

                FunctionBuilders.Add(MB);

                functionDeclaration.FunctionInfo.MethodBuilder = MB;
            }

            for (int i = 0; i < this.Declarations.Count(); i++)
            {
                this.Declarations.ElementAt(i).GenerateCode(
                    moduleBuilder, program, FunctionBuilders[i].GetILGenerator()
                );
            }
        }
    }
}
