using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TigerCompiler.AST;

namespace TigerCompiler.Semantics
{
    class Scope
    {
        List<ItemInfo> declarations;

        public Scope Parent { get; set; }

        public List<Scope> Children { get; set; }

        public int ParentIndex { get; set; }

        public Scope(Scope parent)
        {
            Children = new List<Scope>();
            declarations = new List<ItemInfo>();
            this.Parent = parent;
            this.RegisterBuiltinTypes();
            this.RegisterStandardLibrary();
        }

        private void RegisterBuiltinTypes()
        {
            if (Parent != null)
            {
                foreach (var item in this.Parent.declarations)
                {
                    var TI = item as TypeInfo;
                    if (TI != null && TI.IsBuiltIn)
                        this.Add(TI);
                }
            }
            else
            {
                Add(new TypeInfo("int", PredefinedTypes.IntType, true));
                Add(new TypeInfo("string", PredefinedTypes.StrType, true));
            }
        }

        private void RegisterStandardFunction(string name, TypeNode ReturnType, params VariableInfo[] parameters)
        {
            this.Add(new FunctionInfo(name, ReturnType, parameters, true));
        }

        private void RegisterStandardLibrary()
        {
            if (this.Parent != null)
            {
                foreach (var item in this.Parent.declarations)
                {
                    var FI = item as FunctionInfo;
                    if (FI != null && FI.IsStandard)
                        this.Add(FI);
                }
            }
            else
            {
                //--------------------------------------------------
                // getline():string
                //--------------------------------------------------
                this.RegisterStandardFunction("getline", PredefinedTypes.StrType);
                //--------------------------------------------------
                // function print(s : string)
                // Print the string on the standard output.
                //--------------------------------------------------
                this.RegisterStandardFunction("print", PredefinedTypes.VoidType,
                    new VariableInfo("s", PredefinedTypes.StrType));
                //--------------------------------------------------
                // function printi(i : int)
                // Print the integer on the standard output.
                //--------------------------------------------------
                this.RegisterStandardFunction("printi", PredefinedTypes.VoidType,
                    new VariableInfo("i", PredefinedTypes.IntType));
                //--------------------------------------------------
                // printline(s : string)
                //--------------------------------------------------
                this.RegisterStandardFunction("printline", PredefinedTypes.VoidType,
                    new VariableInfo("s", PredefinedTypes.StrType));
                //--------------------------------------------------
                // printiline(i : int)
                //--------------------------------------------------
                this.RegisterStandardFunction("printiline", PredefinedTypes.VoidType,
                    new VariableInfo("i", PredefinedTypes.IntType));
                //--------------------------------------------------
                // function ord(s : string) : int
                // Return the ASCII value of the first character of s,
                // or 1 if s is empty.
                //--------------------------------------------------
                this.RegisterStandardFunction("ord", PredefinedTypes.IntType,
                    new VariableInfo("s", PredefinedTypes.StrType));
                //--------------------------------------------------
                // function chr(i : int) : string
                // Return a single-character string for ASCII value i.
                // Terminate program if i is out of range.
                //--------------------------------------------------
                this.RegisterStandardFunction("chr", PredefinedTypes.StrType,
                    new VariableInfo("i", PredefinedTypes.IntType));
                //--------------------------------------------------
                // function size(s : string) : int
                // Return the number of characters in s.
                //--------------------------------------------------
                this.RegisterStandardFunction("size", PredefinedTypes.IntType,
                    new VariableInfo("s", PredefinedTypes.StrType));
                //--------------------------------------------------
                // function substring(s:string,f:int,n:int):string
                // Return the substring of s starting at the character
                // f (first character is numbered zero) and going for
                // n characters.
                //--------------------------------------------------
                this.RegisterStandardFunction("substring", PredefinedTypes.StrType,
                    new VariableInfo("s", PredefinedTypes.StrType),
                    new VariableInfo("f", PredefinedTypes.IntType),
                    new VariableInfo("n", PredefinedTypes.IntType));
                //--------------------------------------------------
                // function concat (s1:string, s2:string):string
                // Return a new string consisting of s1 followed by s2.
                //--------------------------------------------------
                this.RegisterStandardFunction("concat", PredefinedTypes.StrType,
                    new VariableInfo("s1", PredefinedTypes.StrType),
                    new VariableInfo("s2", PredefinedTypes.StrType));
                //--------------------------------------------------
                // function not(i : int) : int
                // Return 1 if i is zero, 0 otherwise.
                //--------------------------------------------------
                this.RegisterStandardFunction("not", PredefinedTypes.IntType,
                    new VariableInfo("i", PredefinedTypes.IntType));
                //--------------------------------------------------
                // function exit(i : int)
                // Terminate execution of the program with code i.
                //--------------------------------------------------
                this.RegisterStandardFunction("exit", PredefinedTypes.VoidType,
                    new VariableInfo("i", PredefinedTypes.IntType));
            }
        }

        private List<Scope> GetToRoot()
        {
            List<Scope> scopes = new List<Scope>();
            for (Scope currentScope = this; currentScope != null; currentScope = currentScope.Parent)
                scopes.Add(currentScope);
            return scopes;
        }

        public IEnumerable<VariableInfo> GetDescendingVariableInfos()
        {
            foreach (var declaration in this.declarations) {
                var vi = declaration as VariableInfo;
                if (vi != null) yield return vi;
            }
            foreach (var child in this.Children)
                foreach (var vi in child.GetDescendingVariableInfos())
                {
                    yield return vi;
                }
        }

        public IEnumerable<TypeInfo> GetDescendingTypeInfos()
        {
            foreach (var declaration in this.declarations)
            {
                var ti = declaration as TypeInfo;
                if (ti != null) yield return ti;
            }
            foreach (var child in this.Children)
                foreach (var ti in child.GetDescendingTypeInfos())
                {
                    yield return ti;
                }
        }

        public Scope CreateChildScope()
        {
            var scope = new Scope(this)
            {
                ParentIndex = this.declarations.Count()
            };
            this.Children.Add(scope);
            return scope;
        }

        public void Add(ItemInfo item)
        {
            declarations.Add(item);
        }

        public FunctionInfo FindLocalFunctionInfo(string name)
        {
            foreach (var declaration in declarations) {
                var FI = (declaration as FunctionInfo);
                if (FI != null && FI.Name == name)
                    return FI;
            }
            return null;
        }

        public VariableInfo FindLocalVariableInfo(string name)
        {
            foreach (var declaration in declarations)
            {
                var VI = (declaration as VariableInfo);
                if (VI != null && VI.Name == name)
                    return VI;
            }
            return null;
        }

        public TypeInfo FindLocalTypeInfo(string name)
        {
            foreach (var declaration in declarations)
            {
                var TI = (declaration as TypeInfo);
                if (TI != null && TI.Name == name)
                    return TI;
            }
            return null;
        }

        public FunctionInfo FindFunctionInfo(string name)
        {
            var scopes = this.GetToRoot();
            int currentIndex = this.declarations.Count;
            foreach (var scope in scopes) {
                for (int i = 0; i < currentIndex; i++) {
                    var FI = (scope.declarations[i] as FunctionInfo);
                    if (FI != null && FI.Name == name)
                        return FI;
                }
                currentIndex = scope.ParentIndex;
            }
            return null;
        }

        public VariableInfo FindVariableInfo(string name)
        {
            var scopes = this.GetToRoot();
            int currentIndex = this.declarations.Count;
            foreach (var scope in scopes)
            {
                for (int i = 0; i < currentIndex; i++)
                {
                    var VI = (scope.declarations[i] as VariableInfo);
                    if (VI != null && VI.Name == name)
                        return VI;
                }
                currentIndex = scope.ParentIndex;
            }
            return null;
        }

        public TypeInfo FindTypeInfo(string name)
        {
            var scopes = this.GetToRoot();
            int currentIndex = this.declarations.Count;
            foreach (var scope in scopes)
            {
                for (int i = 0; i < currentIndex; i++)
                {
                    var TI = (scope.declarations[i] as TypeInfo);
                    if (TI != null && TI.Name == name)
                        return TI;
                }
                currentIndex = scope.ParentIndex;
            }
            return null;
        }
    }
}
