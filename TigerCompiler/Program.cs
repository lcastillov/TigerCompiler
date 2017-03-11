using Antlr.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using TigerCompiler.AST;
using TigerCompiler.Parsing;
using TigerCompiler.Semantics;

namespace TigerCompiler
{
    class Program
    {
        public static Scope scope;

        static void Main(string[] args)
        {
            Console.WriteLine("Tiger Compiler version 1.0");
            Console.WriteLine("Copyright (C) 2014-2015 Alejandro Valdes Camejo & Leandro Castillo Valdes");
            Console.WriteLine();

            if (args.Length != 1)
            {
                Console.WriteLine("(0,0): Wrong parameter number.");
                Environment.ExitCode = (int)ErrorCode.Error;
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("(0,0): File ‘{0}’ cannot be found.", args[0]);
                Environment.ExitCode = (int)ErrorCode.Error;
                return;
            }

            ProcessFile(args[0], Path.ChangeExtension(args[0], "exe"));
        }

        public static void ProcessFile(string inputPath, string outputPath)
        {
            ProgramNode root = ParseInput(inputPath);
            
            if (root == null)
            {
                Environment.ExitCode = (int)ErrorCode.Error;
                return;
            }

            if (!CheckSemantics(root))
            {
                Environment.ExitCode = (int)ErrorCode.Error;
                return;
            }

            GenerateCode(root, outputPath);
        }

        static bool CheckSemantics(ProgramNode root)
        {
            //--------------------------------------------------
            // Crear El Scope Más Global. Poner Como Raíz.
            //--------------------------------------------------
            scope = new Scope(null);

            //--------------------------------------------------
            // Lista Para Reportar Los Errores.
            //--------------------------------------------------
            List<SemanticError> errors = new List<SemanticError>();

            //--------------------------------------------------
            // Hacer Chequeo Semántico.
            //--------------------------------------------------
            root.CheckSemantics(scope, errors);

            //--------------------------------------------------
            // Si Ningún Error Fue Reportado, Entonces Todo OK.
            //--------------------------------------------------
            if (errors.Count == 0)
                return true;

            //--------------------------------------------------
            // Reportar Los Errores Semánticos.
            //--------------------------------------------------
            foreach (var error in errors)
                Console.WriteLine("({1}, {2}): {0}", error.Message, error.Node.Line, error.Node.CharPositionInLine);

            return false;
        }

        private static void GenerateCode(ProgramNode root, string outputPath)
        {
            outputPath = Path.GetFullPath(outputPath);

            string name = Path.GetFileNameWithoutExtension(outputPath);

            string filename = Path.GetFileName(outputPath);

            AssemblyName assemblyName = new AssemblyName(name);

            AssemblyBuilder assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(
                assemblyName, AssemblyBuilderAccess.RunAndSave, Path.GetDirectoryName(outputPath)
            );

            ModuleBuilder moduleBuilder = assembly.DefineDynamicModule(name, filename);

            TypeBuilder programType = moduleBuilder.DefineType("Program");

            MethodBuilder mainMethod = programType.DefineMethod("Main",
                MethodAttributes.Static, typeof(void), System.Type.EmptyTypes);

            assembly.SetEntryPoint(mainMethod);

            //--------------------------------------------------
            // Obtener El Generador Para 'Main'.
            //--------------------------------------------------
            ILGenerator MainILGenerator = mainMethod.GetILGenerator();

            //--------------------------------------------------
            // Generar El Código Para Las Funciones Estándares.
            //--------------------------------------------------
            StandardFunctions.GenerateCodeForStandardFunctions(
                programType, scope);

            //--------------------------------------------------
            // Generar El Metodo 'run' Para Program.
            //--------------------------------------------------
            MethodBuilder run = programType.DefineMethod(
                "run", MethodAttributes.Static | MethodAttributes.Public
            );

            //--------------------------------------------------
            // Desde Main Hacer Un LLamado A 'run'. Encapsular Con
            // try/catch
            //--------------------------------------------------
            LocalBuilder exc = MainILGenerator.DeclareLocal(typeof(Exception));

            MainILGenerator.BeginExceptionBlock();
            MainILGenerator.Emit(OpCodes.Call, run);
            MainILGenerator.BeginCatchBlock(typeof(Exception));
            MainILGenerator.Emit(OpCodes.Stloc_S, exc);
            MainILGenerator.Emit(OpCodes.Call, typeof(Console).GetProperty("Error").GetMethod);
            MainILGenerator.Emit(OpCodes.Ldstr, "Exception of type ‘{0}’ was thrown.");
            MainILGenerator.Emit(OpCodes.Ldloc_S, exc);
            MainILGenerator.Emit(OpCodes.Callvirt, typeof(Exception).GetMethod("GetType"));
            MainILGenerator.Emit(OpCodes.Callvirt, typeof(MemberInfo).GetProperty("Name").GetMethod);
            MainILGenerator.Emit(OpCodes.Call, typeof(String).GetMethod("Format", new Type[] { typeof(string), typeof(string) }));
            MainILGenerator.Emit(OpCodes.Callvirt, typeof(TextWriter).GetMethod("WriteLine", new Type[] { typeof(string) }));
            MainILGenerator.Emit(OpCodes.Ldc_I4, 1);
            MainILGenerator.Emit(OpCodes.Call, typeof(Environment).GetMethod("Exit"));
            MainILGenerator.EndExceptionBlock();
            
            MainILGenerator.Emit(OpCodes.Ret);
            
            //--------------------------------------------------
            // Comenzar La Generación De Código.
            //--------------------------------------------------
            root.GenerateCode(moduleBuilder, programType, run.GetILGenerator());

            //--------------------------------------------------
            // Crear 'Program'.
            //--------------------------------------------------
            programType.CreateType();

            moduleBuilder.CreateGlobalFunctions();
            
            assembly.Save(filename);

            //----------------------------------------------------------------------
            // Open ildasm
            //----------------------------------------------------------------------
            //string ildasm = @"C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\ildasm.exe";
            //(new Process()
            //{
            //    StartInfo = { FileName = ildasm, Arguments = @"..\..\program.exe" }
            //}).Start();
        }

        public static ProgramNode ParseInput(string inputPath)
        {
            try
            {
                ANTLRFileStream input = new ANTLRFileStream(inputPath);
                TigerLexer lexer = new TigerLexer(input);
                CommonTokenStream tokens = new CommonTokenStream(lexer);
                TigerParser parser = new TigerParser(tokens);
                parser.TreeAdaptor = new Adaptor();
                var program = (ProgramNode)parser.program().Tree;
                return program;
            }
            catch (ParsingException exc)
            {
                Console.WriteLine("({1},{2}): {0}", exc.Message, exc.RecognitionError.Line, exc.RecognitionError.CharPositionInLine);
                Environment.ExitCode = (int)ErrorCode.Error;
                return null;
            }
        }
    }
}
