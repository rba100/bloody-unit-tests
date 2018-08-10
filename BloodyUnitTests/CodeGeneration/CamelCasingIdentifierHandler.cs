using System;

namespace BloodyUnitTests.CodeGeneration
{
    class CamelCasingIdentifierHandler : ITypeHandler
    {
        private readonly ITypeHandler m_InnerHandler;

        public CamelCasingIdentifierHandler(ITypeHandler innerHandler)
        {
            m_InnerHandler = innerHandler ?? throw new ArgumentNullException(nameof(innerHandler));
        }

        public bool CanGetInstantiation(Type type)
        {
            return m_InnerHandler.CanGetInstantiation(type);
        }

        public string GetInstantiation(Type type, bool interestingValue)
        {
            return m_InnerHandler.GetInstantiation(type, interestingValue);
        }

        public bool IsInstantiationTerse(Type type)
        {
            return m_InnerHandler.IsInstantiationTerse(type);
        }

        public bool CanGetNameForIdentifier(Type type)
        {
            return m_InnerHandler.CanGetNameForIdentifier(type);
        }

        public string GetNameForIdentifier(Type type)
        {
            var name = m_InnerHandler.GetNameForIdentifier(type);
            return StringUtils.ToLowerInitial(name);
        }

        public bool CanGetNameForCSharp(Type type)
        {
            return m_InnerHandler.CanGetNameForCSharp(type);
        }

        public string GetNameForCSharp(Type type)
        {
            return m_InnerHandler.GetNameForCSharp(type);
        }
    }
}