using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TigerCompiler.AST;

namespace TigerCompiler.Semantics
{
    internal class FunctionInfo : ItemInfo
    {
        internal TypeNode ReturnType { get; set; }
        internal VariableInfo[] Parameters { get; private set; }
        internal bool IsStandard { get; private set; }

        public MethodBuilder MethodBuilder { get; set; }

        public Scope Scope { get; set; }

        public FunctionInfo(string name, TypeNode ReturnType, VariableInfo[] Parameters, bool IsStandard = false)
            : base(name)
        {
            this.ReturnType = ReturnType;
            this.Parameters = Parameters;
            this.IsStandard = IsStandard;
        }
    }
}
