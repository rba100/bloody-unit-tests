using System;
using System.Linq;

namespace BloodyUnitTests.CodeGeneration
{
    class GenericTypeNameHandler : IRecursiveTypeHandler
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
            return false;
        }

        public string GetInstantiation(Type type, bool interestingValue)
        {
            throw new NotImplementedException();
        }

        public bool IsInstantiationTerse(Type type)
        {
            throw new NotImplementedException();
        }

        public bool CanGetNameForIdentifier(Type type)
        {
            return type.IsGenericType;
        }

        public string GetNameForIdentifier(Type type)
        {
            var typeDisplayName = type.Name;
            var genArgs = type.GetGenericArguments();
            int index = typeDisplayName.IndexOf('`');
            typeDisplayName = index == -1 ? typeDisplayName : typeDisplayName.Substring(0, index);
            typeDisplayName = typeDisplayName
                              + string.Join("", genArgs.Select(p => m_RootHandler.GetNameForIdentifier(p)));
            return typeDisplayName;
        }

        public bool CanGetNameForCSharp(Type type)
        {
            return type.IsGenericType;
        }

        public string GetNameForCSharp(Type type)
        {
            var typeDisplayName = type.Name;
            var genArgs = type.GetGenericArguments();
            int index = typeDisplayName.IndexOf('`');
            typeDisplayName = index == -1 ? typeDisplayName : typeDisplayName.Substring(0, index);
            typeDisplayName = typeDisplayName + "<" + string.Join(", ", genArgs.Select(m_RootHandler.GetNameForCSharp)) + ">";
            return typeDisplayName;
        }
    }
}