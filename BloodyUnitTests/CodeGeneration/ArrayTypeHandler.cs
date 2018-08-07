﻿using System;
using System.Linq;

namespace BloodyUnitTests.CodeGeneration
{
    class ArrayTypeHandler : IRecursiveTypeHandler
    {
        private ITypeHandler m_RootHandler;

        private bool IsArrayAssignable(Type type)
        {
            var arrayType = GetArrayElementType(type);
            return arrayType != null && arrayType != type;
        }

        private Type GetArrayElementType(Type type)
        {
            var gArgs = type.GetGenericArguments();
            if (!type.HasElementType && gArgs.Length != 1) return null;
            var elementType = gArgs.SingleOrDefault() ?? type.GetElementType();
            // ReSharper disable once PossibleNullReferenceException
            return type.IsAssignableFrom(elementType.MakeArrayType()) ? elementType : null;
        }

        public bool CanGetInstantiation(Type type)
        {
            return type.IsArray || IsArrayAssignable(type);
        }

        public string GetInstantiation(Type type, bool interestingValue)
        {
            if (type == typeof(byte[])) return interestingValue ? "Encoding.UTF8.GetBytes(\"{}\")" : "new byte[0]";
            if (type == typeof(Type[])) return interestingValue ? "new Type[] { typeof(string) }" : "Type.EmptyTypes";

            var eType = GetArrayElementType(type);

            return !interestingValue 
                ? $"new {m_RootHandler.GetNameForCSharp(eType)}[0]"
                : $"new {m_RootHandler.GetNameForCSharp(eType)}[] {{ {m_RootHandler.GetInstantiation(eType, true)} }}";
        }

        public bool IsInstantiationTerse(Type type)
        {
            return false;
        }

        public bool CanGetNameForIdentifier(Type type)
        {
            return type.IsArray;
        }

        public string GetNameForIdentifier(Type type)
        {
            var elementTypeName = m_RootHandler.GetNameForIdentifier(GetArrayElementType(type));
            return $"{elementTypeName}s";
        }

        public bool CanGetNameForCSharp(Type type)
        {
            return type.IsArray;
        }

        public string GetNameForCSharp(Type type)
        {
            return $"{GetArrayElementType(type)}[]";
        }

        public void SetRoot(ITypeHandler handler)
        {
            m_RootHandler = handler;
        }
    }
}