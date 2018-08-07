using System;

namespace BloodyUnitTests.CodeGeneration
{
    class StringAndCharTypeHandler : IRecursiveTypeHandler
    {
        public bool CanGetInstantiation(Type type)
        {
            return type == typeof(string) || type == typeof(char);
        }

        public string GetInstantiation(Type type, bool interestingValue)
        {
            if (type == typeof(string))
            {
                return interestingValue ? "\"value\"" : "\"\"";
            }
            return interestingValue ? "'X'" : "' '";
        }

        public bool IsInstantiationTerse(Type type)
        {
            return true;
        }

        public bool CanGetNameForIdentifier(Type type)
        {
            return type == typeof(string) || type == typeof(char);
        }

        public string GetNameForIdentifier(Type type)
        {
            return type == typeof(string) ? "String" : "Char";
        }

        public bool CanGetNameForCSharp(Type type)
        {
            return type == typeof(string) || type == typeof(char);
        }

        public string GetNameForCSharp(Type type)
        {
            return type == typeof(string) ? "string" : "char";
        }

        public void SetRoot(ITypeHandler handler)
        {

        }
    }
}