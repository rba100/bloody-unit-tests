using System;

namespace BloodyUnitTests.CodeGeneration
{
    class RhinoMockingTypeHandler : IRecursiveTypeHandler
    {
        private ITypeHandler m_RootHandler;

        public void SetRoot(ITypeHandler handler)
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

        public string GetMock(Type type)
        {
            return $"GenerateMock<{m_RootHandler.GetNameForCSharp(type)}>()";
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

        public string GetNameForIdentifier(Type type)
        {
            throw new NotSupportedException();
        }

        public string GetNameForCSharp(Type type)
        {
            throw new NotSupportedException();
        }
    }
}