using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TigerCompiler.Semantics
{
    internal static class StandardFunctions
    {
        internal static void GenerateCodeForStandardFunctions(TypeBuilder program, Scope Root)
        {
            getline(Root.FindFunctionInfo("getline"), program);
            print(Root.FindFunctionInfo("print"), program);
            printi(Root.FindFunctionInfo("printi"), program);
            printline(Root.FindFunctionInfo("printline"), program);
            printiline(Root.FindFunctionInfo("printiline"), program);
            not(Root.FindFunctionInfo("not"), program);
            ord(Root.FindFunctionInfo("ord"), program);
            chr(Root.FindFunctionInfo("chr"), program);
            size(Root.FindFunctionInfo("size"), program);
            substring(Root.FindFunctionInfo("substring"), program);
            concat(Root.FindFunctionInfo("concat"), program);
            exit(Root.FindFunctionInfo("exit"), program);
        }

        private static void not(FunctionInfo functionInfo, TypeBuilder program)
        {
            MethodBuilder MB = program.DefineMethod(
               functionInfo.Name, MethodAttributes.Static,
               typeof(int),
               new Type[] { typeof(int) }
            );

            ILGenerator generator = MB.GetILGenerator();

            var TRUE = generator.DefineLabel();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Brtrue, TRUE);
            generator.Emit(OpCodes.Ldc_I4, 1);
            generator.Emit(OpCodes.Ret);
            generator.MarkLabel(TRUE);
            generator.Emit(OpCodes.Ldc_I4, 0);
            generator.Emit(OpCodes.Ret);
            
            functionInfo.MethodBuilder = MB;
        }

        private static void chr(FunctionInfo functionInfo, TypeBuilder program)
        {
            MethodBuilder MB = program.DefineMethod(
               functionInfo.Name, MethodAttributes.Static, typeof(string), new Type[] { typeof(int) });

            ILGenerator generator = MB.GetILGenerator();

            var EXIT = generator.DefineLabel();

            //--------------------------------------------------
            // [Stack Status]
            // 2. 0
            // 1. n
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldc_I4, 0);

            //--------------------------------------------------
            // [Stack Status]
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Blt, EXIT);

            //--------------------------------------------------
            // [Stack Status]
            // 2. 127
            // 1. n
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldc_I4, 127);

            //--------------------------------------------------
            // [Stack Status]
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Bgt, EXIT);

            //--------------------------------------------------
            // [Stack Status]
            // 1. n
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Ldarg_0);

            //--------------------------------------------------
            // [Stack Status]
            // 1. char(n)
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToChar", new Type[] { typeof(int) }));
            
            //--------------------------------------------------
            // [Stack Status]
            // 1. string(char(n))
            // (...)
            //--------------------------------------------------
            generator.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToString", new Type[] { typeof(char) }));
            
            generator.Emit(OpCodes.Ret);

            //--------------------------------------------------
            // EXIT
            //--------------------------------------------------
            generator.MarkLabel(EXIT);
            generator.Emit(OpCodes.Newobj, typeof(ArgumentException).GetConstructor(Type.EmptyTypes));
            generator.Emit(OpCodes.Throw);

            functionInfo.MethodBuilder = MB;
        }

        private static void ord(FunctionInfo functionInfo, TypeBuilder program)
        {
            MethodBuilder MB = program.DefineMethod(
               functionInfo.Name, MethodAttributes.Static,
               typeof(int),
               new Type[] { typeof(string) }
            );

            ILGenerator generator = MB.GetILGenerator();

            var NULL_OR_EMPTY = generator.DefineLabel();

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, typeof(String).GetMethod("IsNullOrEmpty"));
            generator.Emit(OpCodes.Brtrue, NULL_OR_EMPTY);

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldc_I4, 0);
            generator.Emit(OpCodes.Call, typeof(String).GetMethod("get_Chars", new Type[] { typeof(int) }));
            generator.Emit(OpCodes.Ret);

            generator.MarkLabel(NULL_OR_EMPTY);
            generator.Emit(OpCodes.Ldc_I4, -1);
            generator.Emit(OpCodes.Ret);

            functionInfo.MethodBuilder = MB;
        }

        private static void substring(FunctionInfo functionInfo, TypeBuilder program)
        {
            MethodBuilder MB = program.DefineMethod(
               functionInfo.Name, MethodAttributes.Static,
               typeof(string),
               new Type[] { typeof(string), typeof(int), typeof(int) }
            );

            ILGenerator generator = MB.GetILGenerator();

            MethodInfo substr = typeof(String).GetMethod("Substring", new Type[] { typeof(int), typeof(int) });
            
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Ldarg_2);
            generator.Emit(OpCodes.Call, substr);
            generator.Emit(OpCodes.Ret);

            functionInfo.MethodBuilder = MB;
        }

        private static void exit(FunctionInfo functionInfo, TypeBuilder program)
        {
            MethodBuilder MB = program.DefineMethod(
               functionInfo.Name, MethodAttributes.Static, typeof(void), new Type[] { typeof(int) });

            ILGenerator generator = MB.GetILGenerator();

            MethodInfo exit = typeof(Environment).GetMethod("Exit", new Type[] { typeof(int) });
            
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, exit);
            generator.Emit(OpCodes.Ret);

            functionInfo.MethodBuilder = MB;
        }

        private static void concat(FunctionInfo functionInfo, TypeBuilder program)
        {
            MethodBuilder MB = program.DefineMethod(
               functionInfo.Name, MethodAttributes.Static, typeof(string), new Type[] { typeof(string), typeof(string) });

            ILGenerator generator = MB.GetILGenerator();

            MethodInfo concat = typeof(String).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Call, concat);
            generator.Emit(OpCodes.Ret);

            functionInfo.MethodBuilder = MB;
        }

        private static void size(FunctionInfo functionInfo, TypeBuilder program)
        {
            MethodBuilder MB = program.DefineMethod(
               functionInfo.Name, MethodAttributes.Static, typeof(int), new Type[] { typeof(string) });

            ILGenerator generator = MB.GetILGenerator();

            MethodInfo getLength = typeof(String).GetMethod("get_Length");

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Callvirt, getLength);
            generator.Emit(OpCodes.Ret);

            functionInfo.MethodBuilder = MB;
        }

        private static void printiline(FunctionInfo functionInfo, TypeBuilder program)
        {
            MethodBuilder MB = program.DefineMethod(
               functionInfo.Name, MethodAttributes.Static, typeof(void), new Type[] { typeof(int) });

            ILGenerator generator = MB.GetILGenerator();

            MethodInfo writeLine = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(int) });

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, writeLine);
            generator.Emit(OpCodes.Ret);

            functionInfo.MethodBuilder = MB;
        }

        private static void printline(FunctionInfo functionInfo, TypeBuilder program)
        {
            MethodBuilder MB = program.DefineMethod(
                functionInfo.Name, MethodAttributes.Static, typeof(void), new Type[] { typeof(string) });

            ILGenerator generator = MB.GetILGenerator();

            MethodInfo writeLine = typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) });

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, writeLine);
            generator.Emit(OpCodes.Ret);

            functionInfo.MethodBuilder = MB;
        }

        private static void printi(FunctionInfo functionInfo, TypeBuilder program)
        {
            MethodBuilder MB = program.DefineMethod(
                functionInfo.Name, MethodAttributes.Static, typeof(void), new Type[] { typeof(int) });

            ILGenerator generator = MB.GetILGenerator();

            MethodInfo writeLine = typeof(Console).GetMethod("Write", new Type[] { typeof(int) });

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, writeLine);
            generator.Emit(OpCodes.Ret);

            functionInfo.MethodBuilder = MB;
        }

        private static void print(FunctionInfo functionInfo, TypeBuilder program)
        {
            MethodBuilder MB = program.DefineMethod(
                functionInfo.Name, MethodAttributes.Static, typeof(void), new Type[] { typeof(string) });

            ILGenerator generator = MB.GetILGenerator();

            MethodInfo writeLine = typeof(Console).GetMethod("Write", new Type[] { typeof(string) });

            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, writeLine);
            generator.Emit(OpCodes.Ret);

            functionInfo.MethodBuilder = MB;
        }

        private static void getline(FunctionInfo functionInfo, TypeBuilder program)
        {
            MethodBuilder MB = program.DefineMethod(
                functionInfo.Name,MethodAttributes.Static, typeof(string), Type.EmptyTypes );

            ILGenerator generator = MB.GetILGenerator();

            MethodInfo readLine = typeof(Console).GetMethod("ReadLine", Type.EmptyTypes);

            generator.Emit(OpCodes.Call, readLine);
            generator.Emit(OpCodes.Ret);

            functionInfo.MethodBuilder = MB;
        }
    }
}
