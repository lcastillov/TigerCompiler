using System;

namespace TigerCompiler.AST
{
    internal static class PredefinedTypes
    {
        internal static BuiltinType IntType = new BuiltinType()
        {
            Name = "int",
            UnderlyingSystemType = typeof(int)
        };

        internal static BuiltinType StrType = new BuiltinType()
        {
            Name = "string",
            UnderlyingSystemType = typeof(string)
        };

        internal static BuiltinType VoidType = new BuiltinType()
        {
            Name = "void",
            UnderlyingSystemType = typeof(void)
        };

        internal static BuiltinType NilType = new BuiltinType() {
            Name = "nil",
            UnderlyingSystemType = null
        };

        internal static BuiltinType ErrorType = new BuiltinType()
        {
            Name = "error",
            UnderlyingSystemType = null
        };
    }
}
