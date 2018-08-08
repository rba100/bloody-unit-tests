using System;

namespace BloodyUnitTests.CodeGeneration
{
    class CSharpTypeKeywordNameHandler : IRecursiveTypeHandler
    {
        public bool CanGetInstantiation(Type type)
        {
            return false;
        }

        public string GetInstantiation(Type type, bool interestingValue)
        {
            throw new NotImplementedException();
        }

        public bool IsInstantiationTerse(Type type)
        {
            throw new NotImplementedException();
        }

        public bool CanGetNameForIdentifier(Type type)
        {
            return false;
        }

        public string GetNameForIdentifier(Type type)
        {
            throw new NotImplementedException();
        }

        public bool CanGetNameForCSharp(Type type)
        {
            // C# type keywords
            if (type == typeof(string)) return true;
            if (type == typeof(object)) return true;
            if (type == typeof(int)) return true;
            if (type == typeof(uint)) return true;
            if (type == typeof(long)) return true;
            if (type == typeof(ulong)) return true;
            if (type == typeof(short)) return true;
            if (type == typeof(ushort)) return true;
            if (type == typeof(bool)) return true;
            if (type == typeof(decimal)) return true;
            if (type == typeof(float)) return true;
            if (type == typeof(double)) return true;
            if (type == typeof(byte)) return true;

            return false;
        }

        public string GetNameForCSharp(Type type)
        {
            // C# type keywords
            if (type == typeof(string)) return "string";
            if (type == typeof(object)) return "object";
            if (type == typeof(int)) return "int";
            if (type == typeof(uint)) return "uint";
            if (type == typeof(long)) return "long";
            if (type == typeof(ulong)) return "ulong";
            if (type == typeof(short)) return "short";
            if (type == typeof(ushort)) return "ushort";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(decimal)) return "decimal";
            if (type == typeof(float)) return "float";
            if (type == typeof(double)) return "double";
            if (type == typeof(byte)) return "byte";

            throw new NotSupportedException();
        }

        public void SetRoot(ITypeHandler handler)
        {
        }
    }
}