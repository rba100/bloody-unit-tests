using System;
using System.Linq;

namespace BloodyUnitTests.CodeGeneration
{
    class FallbackRecursiveTypeHandler : IRecursiveTypeHandler
    {
        private IRecursiveTypeHandler m_RootHandler;

        public void SetRoot(IRecursiveTypeHandler handler)
        {
            m_RootHandler = handler;
        }

        public bool CanGetInstantiation(Type type)
        {
            return type.IsClass;
        }

        public string GetInstantiation(Type type, bool interestingValue)
        {
            if (type.IsClass
                && !type.IsAbstract
                && type.GetConstructors().Any(ctor => ctor.GetParameters().Length == 0))
            {
                return $"new {m_RootHandler.GetNameForCSharp(type)}()";
            }

            // Assume a helper method exists
            return $"Create{m_RootHandler.GetNameForIdentifier(type, VarScope.Member)}()";
        }

        public bool IsInstantiationTerse(Type type)
        {
            return false;
        }

        public bool CanGetNameForIdentifier(Type type)
        {
            return true;
        }

        public string GetNameForIdentifier(Type type, VarScope scope)
        {
            return scope == VarScope.Local ? StringUtils.ToLowerInitial(type.Name) : type.Name;
        }

        public bool CanGetNameForCSharp(Type type)
        {
            return true;
        }

        public string GetNameForCSharp(Type type)
        {
            return type.Name;
        }
    }
}