using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TigerCompiler.AST;

namespace TigerCompiler.Semantics
{
    class SemanticError
    {
        public string Message { get; set; }

        public TigerNode Node { get; set; }

        public int Line { get; set; }

        public int Column { get; set; }

        public static SemanticError InvalidNumber(string literal, TigerNode node)
        {
            return new SemanticError
            {
                Message = string.Format("'{0}' is not a valid number.", literal),
                Node = node
            };
        }

        public static SemanticError UndefinedVariableUsed(string name, TigerNode node)
        {
            return new SemanticError
            {
                Message = string.Format("Variable '{0}' does not exist.", name),
                Node = node
            };
        }

        public static SemanticError FunctionUsedAsVariable(string name, TigerNode node)
        {
            return new SemanticError
            {
                Message = string.Format("Function '{0}' used as variable or constant.", name),
                Node = node
            };
        }

        public static SemanticError FunctionDoesNotExist(string name, TigerNode node)
        {
            return new SemanticError
            {
                Message = string.Format("Function '{0}' does not exist.", name),
                Node = node
            };
        }

        public static SemanticError FunctionOrConstantUsedAsVariable(string name, TigerNode node)
        {
            return new SemanticError
            {
                Message = string.Format("Can not assign value to function or constant '{0}'", name),
                Node = node
            }; ;
        }

        public static SemanticError WrongParameterNumber(string name, int formalCount, int actualCount, TigerNode node)
        {
            return new SemanticError
            {
                Message = string.Format("Function '{0}' takes {1} arguments, got {2} instead.", name, formalCount, actualCount),
                Node = node
            };
        }

        public static SemanticError InvalidUseOfBreak(TigerNode node)
        {
            return new SemanticError
            {
                Message = "Break used out of a while or for expression.",
                Node = node
            };
        }
        //----------------------------------------------------------------------------------------------------
        // MINE
        //----------------------------------------------------------------------------------------------------
        public static SemanticError WrongFieldNumberInRecord(string name, int formalCount, int actualCount, TigerNode node)
        {
            return new SemanticError
            {
                Message = string.Format("Record '{0}' have {1} arguments, got {2} instead.", name, formalCount, actualCount),
                Node = node
            };
        }

        public static SemanticError WrongFieldNameInRecord(string given, string expected , TigerNode node)
        {
            return new SemanticError
            {
                Message = string.Format("Incorrect field name in record. Name '{0}' given, '{1}' expected.", given, expected),
                Node = node
            };
        }

        public static SemanticError InvalidIndexingOperation(TypeNode type, TigerNode node)
        {
            return new SemanticError
            {
                Message = string.Format("Cannot apply indexing with [] to an expression of type '{0}'.", type.Name),
                Node = node
            };
        }

        public static SemanticError TypeDoesNotExist(string typeID, TigerNode node)
        { 
            return new SemanticError()
            {
                Message = string.Format("The type name '{0}' could not be found.", typeID),
                Node = node
            };
        }

        public static SemanticError InvalidIntNilAssignation(TigerNode node)
        {
            return new SemanticError()
            {
                Message = "Cannot convert null to 'int' because it is a non-nullable value type.",
                Node = node
            };
        }

        public static SemanticError InvalidTypeConvertion(TypeNode left, TypeNode right, TigerNode node)
        {
            return new SemanticError()
            {
                Message = string.Format("Cannot convert '{0}' to '{1}'.", right.Name, left.Name),
                Node = node
            };
        }

        public static SemanticError InvalidTypeInference(TypeNode type, TigerNode node)
        {
            return new SemanticError()
            {
                Message = string.Format("Cannot assign <{0}> to an implicitly-typed variable.", type.Name),
                Node = node
            };
        }

        public static SemanticError PreviousVariableOrFunctionDeclaration(string name, TigerNode node)
        {
            return new SemanticError()
            {
                Message = string.Format("A variable or function '{0}' is already defined.", name),
                Node = node
            };
        }

        public static SemanticError PreviousTypeDeclaration(string name, TigerNode node)
        {
            return new SemanticError()
            {
                Message = string.Format("A type '{0}' is already defined in this scope.", name),
                Node = node
            };
        }

        public static SemanticError PreviousFieldDeclaration(string name, TigerNode node)
        {
            return new SemanticError()
            {
                Message = string.Format("The field '{0}' is already defined in this record.", name),
                Node = node
            };
        }

        public static SemanticError PreviousParameterDeclaration(string parameter, string function, TigerNode node)
        {
            return new SemanticError()
            {
                Message = string.Format("The parameter '{0}' is already defined in function '{1}'.", parameter, function),
                Node = node
            };
        }

        public static SemanticError IdentifierExpectedKeywordGiven(string keyword, TigerNode node)
        {
            return new SemanticError()
            {
                Message = string.Format("Identifier expected; '{0}' is a keyword.", keyword),
                Node = node
            };
        }

        public static SemanticError InvalidAccessToAField(string fieldName, TypeNode type, TigerNode node)
        {
            return new SemanticError()
            {
                Message = string.Format("'{0}' does not contain a definition for '{1}'.", type.Name, fieldName),
                Node = node
            };
        }

        public static SemanticError CycleInTypeDeclaration(string type, TigerNode node)
        {
            return new SemanticError()
            {
                Message = string.Format("Cycle detected defininig type '{0}'.", type),
                Node = node
            };
        }

        public static SemanticError ProcedureCannotReturn(string name, TigerNode node)
        {
            return new SemanticError()
            {
                Message = string.Format("Procedure '{0}' cannot return.", name),
                Node = node
            };
        }

        public static SemanticError FunctionMustReturn(string name, TigerNode node)
        {
            return new SemanticError()
            {
                Message = string.Format("Function '{0}' must return.", name),
                Node = node
            };
        }

        internal static SemanticError VariableMustHaveAType(string name, TigerNode node)
        {
            return new SemanticError()
            {
                Message = string.Format("Cannot assign void expression to variable '{0}'.", name),
                Node = node
            };
        }

        internal static SemanticError InvalidCompareOperation(TypeNode left, TypeNode right, TigerNode node)
        {
            return new SemanticError()
            {
                Message = string.Format("Cannot compare expressions of types '{0}' and '{1}'.", left.Name, right.Name),
                Node = node
            };
        }

        internal static SemanticError InvalidArrayAccess(TigerNode node)
        {
            return new SemanticError()
            {
                Message = string.Format("Invalid array access. Index expression must be of type int."),
                Node = node
            };
        }

        internal static SemanticError InvalidUseOfBinaryArithmeticOperator(string operatorName, string operandPosition, TigerNode node)
        {
            return new SemanticError()
            {
                Message = string.Format("Invalid use of {0} operator with a non-integer {1} value.", operatorName, operandPosition),
                Node = node
            };
        }

        internal static SemanticError InvalidUseOfUnaryMinusOperator(TigerNode node)
        {
            return new SemanticError()
            {
                Message = string.Format("The expression of the unary minus operator must be an integer."),
                Node = node
            };
        }

        internal static SemanticError TypeNoLongerVisible(TypeNode type, TigerNode node)
        {
            return new SemanticError()
            {
                Message = string.Format("Type '{0}' is not longer visible at this scope.", type.Name),
                Node = node
            };
        }

        internal static SemanticError ExpectedType(TypeNode expected, TypeNode given, TigerNode node)
        {
            return new SemanticError()
            {
                Message = string.Format("Expected type '{0}', '{1}' given.", expected.Name, given.Name),
                Node = node
            };
        }

        internal static SemanticError IncompatibleTypesInIfThenElse(TigerNode node)
        {
            return new SemanticError()
            {
                Message = string.Format("Expressions in if-then-else must be of the same type or both not return a value."),
                Node = node
            };
        }

        internal static SemanticError RecordTypeExpected(TypeNode type, TigerNode node)
        {
            return new SemanticError()
            {
                Message = string.Format("Record type expected, type '{0}' given.", type.Name),
                Node = node
            };
        }

        internal static SemanticError ArrayTypeExpected(TypeNode type, TigerNode node)
        {
            return new SemanticError()
            {
                Message = string.Format("Array type expected, type '{0}' given.", type.Name),
                Node = node
            };
        }

        internal static SemanticError InvalidUseOfBinaryLogicalOperator(string operatorPosition, TigerNode node)
        {
            return new SemanticError()
            {
                Message = string.Format("Invalid use of binary logical operator with a non-integer {0} value.", operatorPosition),
                Node = node
            };
        }

        internal static SemanticError InvalidUseOfAssignmentToAReadonlyVariable(TigerNode node)
        {
            return new SemanticError()
            {
                Message = string.Format("Invalid use of assignment to a readonly variable."),
                Node = node
            };
        }

        //----------------------------------------------------------------------------------------------------
        // MINE
        //----------------------------------------------------------------------------------------------------
        public static bool CompatibleTypes(TypeNode L, TypeNode R)
        {
            if (L == R)
                return true;
            if (R == PredefinedTypes.NilType)
            {
                return (L != PredefinedTypes.IntType) &&
                       (L != PredefinedTypes.VoidType);
            }
            return false;
        }
    }
}
