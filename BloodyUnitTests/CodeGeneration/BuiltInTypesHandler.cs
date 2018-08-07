using System;

namespace BloodyUnitTests.CodeGeneration
{
    class BuiltInTypesHandler : IRecursiveTypeHandler
    {
        public bool CanGetInstantiation(Type type)
        {
            return type == typeof(bool)
                   || type == typeof(IntPtr)
                   || type == typeof(Type);
        }

        public string GetInstantiation(Type type, bool interestingValue)
        {
            if (type == typeof(bool)) return interestingValue.ToString().ToLower();
            if (type == typeof(IntPtr)) return interestingValue ? "new IntPtr(1)" : "IntPtr.Zero";
            if (type == typeof(Type)) return "typeof(object)";

            throw new NotSupportedException();
        }

        public bool IsInstantiationTerse(Type type)
        {
            return true;
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
            return false;
        }

        public string GetNameForCSharp(Type type)
        {
            throw new NotImplementedException();
        }

        public void SetRoot(ITypeHandler handler)
        {
        }
    }
}