using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TigerCompiler.Semantics;

namespace TigerCompiler.AST
{
    abstract class TypeNode : UtilNode
    {
        public Type UnderlyingSystemType { get; set; }

        public TypeNode() { }

        public TypeNode(IToken token) : base(token) { }

        public TypeNode(TypeNode node) : base(node) { }

        public string Name { get; set; }

        public FieldBuilder VirtualStack1 { get; set; }
        public FieldBuilder VirtualStack2 { get; set; }
    }
}
