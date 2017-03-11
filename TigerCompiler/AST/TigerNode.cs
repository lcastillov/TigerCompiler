using Antlr.Runtime;
using Antlr.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    abstract class TigerNode : CommonTree
    {
        public static int TypesDefined { get; set; }

        public static int VariablesDeclared { get; set; }

        public static int FunctionsDeclared { get; set; }

        public TigerNode() { }

        public TigerNode(IToken token) : base(token) { }

        public TigerNode(TigerNode node) : base(node) { }

        public abstract void CheckSemantics(Scope scope, List<SemanticError> errors);

        //----------------------------------------------------------------------------------------------------
        // 'moduleBuilder': Es Donde Se Definirán Todos Los Tipos.
        // 'program'      : Es Donde Estarán Las Declaraciones De Las Variables y Funciones.
        // 'generator'    : Es El Generador De La Función Actual. Al Principio Program::Main.
        //----------------------------------------------------------------------------------------------------
        public abstract void GenerateCode(ModuleBuilder moduleBuilder, TypeBuilder program, ILGenerator generator);
    }
}
