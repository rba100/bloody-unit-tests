using System;

namespace BloodyUnitTests.CodeGeneration
{
    class RhinoMockingTypeHandler : IRecursiveTypeHandler
    {
        private IRecursiveTypeHandler m_RootHandler;

        public void SetRoot(IRecursiveTypeHandler handler)
        {
            m_RootHandler = handler;
        }

        public bool CanGetInstantiation(Type type)
        {
            return type.IsInterface;
        }

        public string GetInstantiation(Type type, bool interestingValue)
        {
            return $"GenerateStub<{m_RootHandler.GetNameForCSharp(type)}>()";
        }

        public bool IsInstantiationTerse(Type type)
        {
            return false; // GenerateStub<blah blah> is a bit long
        }

        public bool CanGetNameForIdentifier(Type type)
        {
            return false;
        }

        public bool CanGetNameForCSharp(Type type)
        {
            return false;
        }

        public string GetNameForIdentifier(Type type, VarScope scope)
        {
            throw new NotSupportedException();
        }

        public string GetNameForCSharp(Type type)
        {
            throw new NotSupportedException();
        }
    }
}