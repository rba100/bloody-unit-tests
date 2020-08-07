using System;

namespace BloodyUnitTests.Engine.CodeGeneration
{
    class DateTimeTypeHandler : IRecursiveTypeHandler
    {
        private ITypeHandler m_RootHandler;

        private bool CanHandle(Type type)
        {
            return type == typeof(DateTime);
        }

        public bool CanGetInstantiation(Type type)
        {
            return CanHandle(type);
        }

        public string GetInstantiation(Type type, bool interestingValue)
        {
            return interestingValue 
                ? "DateTime.MaxValue.ToUniversalTime()"
                : "DateTime.UtcNow";
        }

        public bool IsInstantiationTerse(Type type)
        {
            return false;
        }

        public bool CanGetNameForIdentifier(Type type)
        {
            return CanHandle(type);
        }

        public string GetNameForIdentifier(Type type)
        {
            return "DateTimeUtc";
        }

        public bool CanGetNameForCSharp(Type type)
        {
            return CanHandle(type);
        }

        public string GetNameForCSharp(Type type)
        {
            return "DateTime";
        }

        public void SetRoot(ITypeHandler handler)
        {
            m_RootHandler = handler;
        }

        public INamespaceTracker GetNamespaceTracker()
        {
            return m_RootHandler.GetNamespaceTracker();
        }
    }
}