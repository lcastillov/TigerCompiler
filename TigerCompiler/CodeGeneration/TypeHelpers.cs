using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TigerCompiler.CodeGeneration
{
    public static class GenericStackHelper
    {
        public static ConstructorInfo GetDefaultConstructor(Type type)
        {
            try
            {
                return typeof(Stack<>).MakeGenericType(type).GetConstructor(Type.EmptyTypes);
            }
            catch (Exception)
            {
                var generic = typeof(Stack<>);
                var genericWithType = generic.MakeGenericType(type);
                return TypeBuilder.GetConstructor(
                    genericWithType, generic.GetConstructor(Type.EmptyTypes)
                );
            }
        }

        public static MethodInfo GetMethod(string methodName, Type type)
        {
            try
            {
                return typeof(Stack<>).MakeGenericType(type).GetMethod(methodName);
            }
            catch (Exception)
            {
                var generic = typeof(Stack<>);
                var genericWithType = generic.MakeGenericType(type);
                return TypeBuilder.GetMethod(
                    genericWithType, generic.GetMethod(methodName)
                );
            }
        }
    }
}
