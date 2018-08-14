using System;
using System.Linq;

namespace BloodyUnitTests.CodeGeneration
{
    class FallbackRecursiveTypeHandler : IRecursiveTypeHandler
    {
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
            return true;
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
            return $"Create{m_RootHandler.GetNameForIdentifier(type)}()";
        }

        public bool IsInstantiationTerse(Type type)
        {
            return false;
        }

        public bool CanGetNameForIdentifier(Type type)
        {
            return true;
        }

        public string GetNameForIdentifier(Type type)
        {
            return type.Name.Replace("&","");
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