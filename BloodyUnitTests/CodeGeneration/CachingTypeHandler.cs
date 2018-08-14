using System;
using System.Collections.Generic;

namespace BloodyUnitTests.CodeGeneration
{
    class CachingTypeHandler : ITypeHandler
    {
        private readonly ITypeHandler m_InnerTypeHandler;

        private readonly Dictionary<Type, bool> m_IsTerseCache = new Dictionary<Type, bool>();
        private readonly Dictionary<Type, string> m_NameForCSharpCache = new Dictionary<Type, string>();
        private readonly Dictionary<Type, string> m_NameForIdentifierCache = new Dictionary<Type, string>();
        private readonly Dictionary<Type, string> m_InstantiationCache = new Dictionary<Type, string>();
        private readonly Dictionary<Type, string> m_InterestingInstantiationCache = new Dictionary<Type, string>();

        public CachingTypeHandler(ITypeHandler innerTypeHandler)
        {
            m_InnerTypeHandler = innerTypeHandler
                                 ?? throw new ArgumentNullException(nameof(innerTypeHandler));
        }

        public bool CanGetInstantiation(Type type)
        {
            return true; // cached!
        }

        public string GetInstantiation(Type type, bool interestingValue)
        {
            if (interestingValue)
            {
                if (!m_InterestingInstantiationCache.ContainsKey(type))
                {
                    m_InterestingInstantiationCache[type] = m_InnerTypeHandler.GetInstantiation(type, true);
                }

                return m_InterestingInstantiationCache[type];
            }

            if (!m_InstantiationCache.ContainsKey(type))
            {
                m_InstantiationCache[type] = m_InnerTypeHandler.GetInstantiation(type, false);
            }

            return m_InstantiationCache[type];
        }

        public bool IsInstantiationTerse(Type type)
        {
            if (!m_IsTerseCache.ContainsKey(type))
            {
                m_IsTerseCache[type] = m_InnerTypeHandler.IsInstantiationTerse(type);
            }

            return m_IsTerseCache[type];
        }

        public bool CanGetNameForIdentifier(Type type)
        {
            return true;
        }

        public string GetNameForIdentifier(Type type)
        {
            if (!m_NameForIdentifierCache.ContainsKey(type))
            {
                m_NameForIdentifierCache[type] = m_InnerTypeHandler.GetNameForIdentifier(type);
            }

            return m_NameForIdentifierCache[type];
        }

        public bool CanGetNameForCSharp(Type type)
        {
            return true;
        }

        public string GetNameForCSharp(Type type)
        {
            if (!m_NameForCSharpCache.ContainsKey(type))
            {
                m_NameForCSharpCache[type] = m_InnerTypeHandler.GetNameForCSharp(type);
            }

            return m_NameForCSharpCache[type];
        }

        public INamespaceTracker GetNamespaceTracker()
        {
            return m_InnerTypeHandler.GetNamespaceTracker();
        }
    }
}