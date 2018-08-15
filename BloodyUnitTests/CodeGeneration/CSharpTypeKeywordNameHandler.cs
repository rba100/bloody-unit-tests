using System;
using System.Collections.Generic;

namespace BloodyUnitTests.CodeGeneration
{
    class CSharpTypeKeywordNameHandler : IRecursiveTypeHandler
    {
        private ITypeHandler m_RootHandler;

        private static readonly IDictionary<Type,string> s_KeywordDictionary
            = new Dictionary<Type, string>
        {
            { typeof(string), "string" },
            { typeof(char), "char" },
            { typeof(object), "object" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(bool), "bool" },
            { typeof(decimal), "decimal" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" }
        };

        public bool CanGetNameForCSharp(Type type)
        {
            return s_KeywordDictionary.ContainsKey(type);
        }

        public string GetNameForCSharp(Type type)
        {
            return s_KeywordDictionary[type];
        }

        public bool CanGetInstantiation(Type type)
        {
            return false;
        }

        public string GetInstantiation(Type type, bool interestingValue)
        {
            throw new NotSupportedException();
        }

        public bool IsInstantiationTerse(Type type)
        {
            throw new NotSupportedException();
        }

        public bool CanGetNameForIdentifier(Type type)
        {
            return false;
        }

        public string GetNameForIdentifier(Type type)
        {
            throw new NotSupportedException();
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