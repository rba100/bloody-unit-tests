using System;
using System.Linq;

namespace BloodyUnitTests.CodeGeneration
{
    class NumericTypeHandler : IRecursiveTypeHandler
    {
        private static readonly Type[] s_SupportedTypes =
        {
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal)
        };

        public void SetRoot(ITypeHandler handler)
        {
        }

        public bool CanGetInstantiation(Type type)
        {
            return s_SupportedTypes.Contains(type);
        }

        public string GetInstantiation(Type type, bool interestingValue)
        {
            return interestingValue ? "1337" : "0";
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
            throw new NotSupportedException();
        }

        public bool CanGetNameForCSharp(Type type)
        {
            return false;
        }

        public string GetNameForCSharp(Type type)
        {
            throw new NotSupportedException();
        }
    }
}