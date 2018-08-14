using System;

namespace BloodyUnitTests.CodeGeneration
{
    class GuidTypeHandler : IRecursiveTypeHandler
    {
        private ITypeHandler m_RootHandler;

        public bool CanGetInstantiation(Type type)
        {
            return type == typeof(Guid);
        }

        public string GetInstantiation(Type type, bool interestingValue)
        {
            return interestingValue ? "new Guid(1,2,3,4,5,6,7,8,9,0,1)" : "Guid.Empty";
        }

        public bool IsInstantiationTerse(Type type)
        {
            return false;
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

        public INamespaceTracker GetNamespaceTracker()
        {
            return m_RootHandler.GetNamespaceTracker();
        }

        public void SetRoot(ITypeHandler handler)
        {
            m_RootHandler = handler;
        }
    }
}