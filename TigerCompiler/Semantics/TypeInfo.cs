using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TigerCompiler.AST;

namespace TigerCompiler.Semantics
{
    internal class TypeInfo : ItemInfo
    {
        internal TypeNode TypeNode { get; private set; }
        internal TypeBuilder TypeDefinition { get; private set; }
        internal bool IsBuiltIn { get; set; }

        public TypeInfo(string name, TypeNode type, bool isBuiltIn = false)
            : base(name)
        {
            this.TypeNode = type;
            this.IsBuiltIn = isBuiltIn;
        }
    }
}
