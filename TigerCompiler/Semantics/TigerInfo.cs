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
    internal abstract class ItemInfo
    {
        public ItemInfo(string name)
        {
            Name = name;
        }

        internal string Name { get; private set; }
    }
}
