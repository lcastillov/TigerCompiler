using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TigerCompiler.AST
{
    interface IBreakableNode
    {
        List<Label> Labels { get; set; }
    }
}
