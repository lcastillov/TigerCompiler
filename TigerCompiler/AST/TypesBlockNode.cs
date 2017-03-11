using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    class TypesBlockNode : BlockDeclarationsNode
    {
        public TypesBlockNode() { }

        public TypesBlockNode(IToken token) : base(token) { }

        public TypesBlockNode(TypesBlockNode node) : base(node) { }

        public TypeDeclarationNode[] Declarations
        {
            get
            {
                return this.Children.Cast<TypeDeclarationNode>().ToArray();
            }
        }

        public override void CheckSemantics(Scope scope, List<SemanticError> errors)
        {
            //--------------------------------------------------
            // Por Default, El Nodo No Tiene Errores.
            //--------------------------------------------------
            this.IsOk = true;

            //--------------------------------------------------
            // Obtener Todos Los Nombres De Los Tipos En El
            // Bloque Actual.
            //--------------------------------------------------
            string[] names = this.Declarations.Select(x => x.ID.Name).ToArray<string>();

            //--------------------------------------------------
            // Comprobar Que Todos Los Nombres Se Declaren Solo
            // Una Vez.
            //--------------------------------------------------
            for (int i = 0; i < names.Length; i++)
            {
                bool hasPreviousDeclaration = (scope.FindLocalTypeInfo(names[i]) != null);
                for (int j = 0; j < i; j++)
                    if (names[i] == names[j]) hasPreviousDeclaration = true;
                if (hasPreviousDeclaration)
                {
                    errors.Add(SemanticError.PreviousTypeDeclaration(names[i], this.Declarations[i]));
                    this.IsOk = false;
                }
            }

            if (!this.IsOk)
                return;

            //--------------------------------------------------
            // Buscar Ciclos En Las Declaraciones AliasTypeNode
            // Y ArrayTypeNode.
            //--------------------------------------------------
            bool[] visited = new bool[this.Declarations.Length];

            //--------------------------------------------------
            // Actualizar 'Scope' Solamente Con Declaraciones De Records.
            //--------------------------------------------------
            for (int i = 0; i < this.Declarations.Length; i++)
                if (this.Declarations[i].TypeNode is RecordTypeNode)
                {
                    visited[i] = true;
                    scope.Add(new TypeInfo(this.Declarations[i].ID.Name, this.Declarations[i].TypeNode));
                }

            for (int i = 0; i < names.Length && this.IsOk; i++)
            {
                if (visited[i]) continue;

                List<int> path = new List<int>();

                int currentDeclaration = i;
                while (true)
                {
                    //--------------------------------------------------
                    // Si La Declaración Ha Sido Visitada Previamente,
                    // Entonces Estamos En Presencia  De  Un  Ciclo.
                    //--------------------------------------------------
                    if (visited[currentDeclaration])
                    {
                        errors.Add(SemanticError.CycleInTypeDeclaration(
                            this.Declarations[currentDeclaration].ID.Name, this
                        ));
                        this.IsOk = false;
                        break;
                    }

                    //--------------------------------------------------
                    // Marcar Como "Visitada" La Declaración Actual.
                    //--------------------------------------------------
                    path.Add(currentDeclaration);
                    visited[currentDeclaration] = true;

                    //--------------------------------------------------
                    // Buscar El Nombre Del Tipo Del Que Depende La
                    // Declaración Actual:
                    //
                    // type <type-id> = <type-id>
                    // type <type-id> = array of <type-id>
                    //--------------------------------------------------
                    var CDN = this.Declarations.ElementAt(currentDeclaration).TypeNode;
                    
                    string type = CDN is AliasTypeNode ? (CDN as AliasTypeNode).TypeID.Name :
                                                         (CDN as ArrayTypeNode).TypeID.Name ;

                    //--------------------------------------------------
                    // Buscar Primero Si Depende De Un Tipo En Este Bloque
                    // De Declaraciones.
                    //--------------------------------------------------
                    int namesIndex = Array.FindIndex(this.Declarations, x => x.ID.Name == type);

                    //--------------------------------------------------
                    // Si El Tipo No Esta En El Bloque De Declaraciones,
                    // Buscar En El Scope Global. De Lo Contrario, Buscar
                    // En El Scope Local.
                    //--------------------------------------------------
                    TypeInfo TI = null;
                    if (namesIndex < 0 || namesIndex >= names.Length)
                        TI = scope.FindTypeInfo(type);
                    else
                        TI = scope.FindLocalTypeInfo(type);

                    //--------------------------------------------------
                    // Si El Tipo Está En El Scope, Actualizar Cada Una
                    // De Las Declaraciones.
                    //--------------------------------------------------
                    if (TI != null)
                    {
                        var TN = TI.TypeNode;

                        for (int u = path.Count - 1; u >= 0; u--)
                        {
                            var declarationNode = this.Declarations[path[u]];
                            if (declarationNode.TypeNode is AliasTypeNode)
                                (declarationNode.TypeNode as AliasTypeNode).AliasOf = TN;
                            else
                            {
                                (declarationNode.TypeNode as ArrayTypeNode).ArrayOf = TN;
                                TN = declarationNode.TypeNode;
                            }
                            scope.Add(new TypeInfo(declarationNode.ID.Name, TN));
                        }
                        break;
                    }

                    //--------------------------------------------------
                    // Si No Se Pudo Encontrar El Tipo , Entonces Comprobar
                    // Si Depende De Alguna Declaración En El Mismo Bloque.
                    // Si No Es El Caso, Reportar Error.
                    //--------------------------------------------------
                    currentDeclaration = namesIndex;

                    if (currentDeclaration < 0 || currentDeclaration >= names.Length)
                    {
                        errors.Add(SemanticError.TypeDoesNotExist(type, CDN));
                        this.IsOk = false;
                        break;
                    }
                }
            }

            //--------------------------------------------------
            // Si Hay Errores, Parar De Reportar Errores.
            //--------------------------------------------------
            if (!this.IsOk)
                return;

            //--------------------------------------------------
            // Hacer 'CheckSemantics' A Cada TypeDeclarationNode.
            //--------------------------------------------------
            foreach (var declaration in this.Declarations)
            {
                declaration.CheckSemantics(scope, errors);
                this.IsOk &= declaration.IsOk;
            }
        }

        public override void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator)
        {
            foreach (var declaration in this.Declarations)
                declaration.GenerateCode(moduleBuilder, program, generator);
        }
    }
}
