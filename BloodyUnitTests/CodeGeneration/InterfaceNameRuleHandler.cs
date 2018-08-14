using System;
using System.Linq;

namespace BloodyUnitTests.CodeGeneration
{
    class InterfaceNameRuleHandler : ITypeHandler
    {
        private readonly ITypeHandler m_InnerHandler;

        public InterfaceNameRuleHandler(ITypeHandler innerHandler)
        {
            m_InnerHandler = innerHandler;
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
            if(type.IsInterface && name.StartsWith("I")) name = new string(name.Skip(1).ToArray());
            return name;
        }

        public bool CanGetNameForCSharp(Type type)
        {
            return m_InnerHandler.CanGetNameForCSharp(type);
        }

        public string GetNameForCSharp(Type type)
        {
            return m_InnerHandler.GetNameForCSharp(type);
        }

        public INamespaceTracker GetNamespaceTracker()
        {
            return m_InnerHandler.GetNamespaceTracker();
        }
    }
}