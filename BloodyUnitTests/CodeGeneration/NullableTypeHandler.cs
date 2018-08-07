using System;

namespace BloodyUnitTests.CodeGeneration
{
    class NullableTypeHandler : IRecursiveTypeHandler
    {
        private ITypeHandler m_RootHandler;

        private Type GetNullableType(Type type)
        {
            return Nullable.GetUnderlyingType(type);
        }

        public bool CanGetInstantiation(Type type)
        {
            return GetNullableType(type) != null;
        }

        public string GetInstantiation(Type type, bool interestingValue)
        {
            var innerType = GetNullableType(type);
            return $"({m_RootHandler.GetNameForCSharp(innerType)}?) {m_RootHandler.GetInstantiation(innerType, interestingValue)}";
        }

        public bool IsInstantiationTerse(Type type)
        {
            var innerType = GetNullableType(type);
            return m_RootHandler.IsInstantiationTerse(innerType);
        }

        public bool CanGetNameForIdentifier(Type type)
        {
            return GetNullableType(type) != null;
        }

        public string GetNameForIdentifier(Type type)
        {
            var name = $"{m_RootHandler.GetNameForCSharp(GetNullableType(type))}Nullable";
            return name;
        }

        public bool CanGetNameForCSharp(Type type)
        {
            return GetNullableType(type) != null;
        }

        public string GetNameForCSharp(Type type)
        {
            return $"{m_RootHandler.GetNameForCSharp(GetNullableType(type))}?";
        }

        public void SetRoot(ITypeHandler handler)
        {
            m_RootHandler = handler;
        }
    }
}