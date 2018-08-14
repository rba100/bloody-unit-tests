using System;
using System.Collections.Generic;

namespace BloodyUnitTests.CodeGeneration
{
    class GeneralDictionaryTypeHandler : IRecursiveTypeHandler
    {
        private ITypeHandler m_RootHandler;

        public bool CanGetInstantiation(Type type)
        {
            return type.IsGenericType
                   && IsDictionaryAssignable(type);
        }

        public string GetInstantiation(Type type, bool interestingValue)
        {
            var dictTypes = GetDictionaryType(type);
            
            var keyTypeName = m_RootHandler.GetNameForCSharp(dictTypes.key);
            var valueTypeName = m_RootHandler.GetNameForCSharp(dictTypes.value);

            var keyTypeValue = m_RootHandler.GetInstantiation(dictTypes.key, true);
            var valueTypeValue = m_RootHandler.GetInstantiation(dictTypes.value, true);

            var invocationSuffix = interestingValue 
                ? $"{{ {{ {keyTypeValue} , {valueTypeValue} }} }}" 
                : "()";

            return $"new Dictionary<{keyTypeName},{valueTypeName}>{invocationSuffix}";
        }

        public bool IsInstantiationTerse(Type type)
        {
            return false;
        }

        public bool CanGetNameForIdentifier(Type type)
        {
            return type.IsGenericType
                && type.GetGenericTypeDefinition() != typeof(Dictionary<,>)
                && IsDictionaryAssignable(type);
        }

        public string GetNameForIdentifier(Type type)
        {
            var dictionaryType = GetDictionaryTypeForAssignment(type);
            return m_RootHandler.GetNameForIdentifier(dictionaryType);
        }

        public bool CanGetNameForCSharp(Type type)
        {
            return false;
        }

        public string GetNameForCSharp(Type type)
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

        private bool IsDictionaryAssignable(Type type)
        {
            return GetDictionaryType(type).key != null;
        }

        private Type GetDictionaryTypeForAssignment(Type type)
        {
            var gArgs = type.GetGenericArguments();
            if (gArgs.Length != 2) return (null);
            var dictType = typeof(Dictionary<,>).MakeGenericType(gArgs);
            return type.IsAssignableFrom(dictType) ? dictType : null;
        }

        private (Type key, Type value) GetDictionaryType(Type type)
        {
            var gArgs = type.GetGenericArguments();
            if (gArgs.Length != 2) return (null, null);
            var dictType = typeof(Dictionary<,>).MakeGenericType(gArgs);
            return type.IsAssignableFrom(dictType) ? (gArgs[0], gArgs[1]) : (null, null);
        }
    }
}