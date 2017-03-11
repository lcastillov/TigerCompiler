using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using TigerCompiler.AST;

namespace TigerCompiler.Semantics
{
    internal class VariableInfo : ItemInfo
    {
        internal TypeNode TypeNode { get; private set; }
        internal bool ReadOnly { get; private set; }
        internal FieldBuilder FieldBuilder { get; set; }

        public VariableInfo(string name, TypeNode type, bool readOnly = false)
            : base(name)
        {
            this.TypeNode = type;
            this.ReadOnly = readOnly;
        }

        public void DeclareVariable(TypeBuilder program)
        {
            if (this.FieldBuilder == null)
            {
                this.FieldBuilder = program.DefineField(
                    string.Format("VAR_{0}", ++TigerNode.VariablesDeclared),
                    this.TypeNode.UnderlyingSystemType,
                    FieldAttributes.Static
                );
            }
        }
    }
}
