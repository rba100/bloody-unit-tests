using System;
using System.Linq;

namespace BloodyUnitTests.Engine.CodeGeneration
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

        public static bool IsHandled(Type type)
        {
            return s_SupportedTypes.Contains(type);
        }


        private ITypeHandler m_RootHandler;

        public INamespaceTracker GetNamespaceTracker()
        {
            return m_RootHandler.GetNamespaceTracker();
        }

        public void SetRoot(ITypeHandler handler)
        {
            m_RootHandler = handler;
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
            return type == typeof(int);
        }

        public string GetNameForIdentifier(Type type)
        {
            return "Int";
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