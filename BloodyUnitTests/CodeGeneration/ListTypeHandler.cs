﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace BloodyUnitTests.CodeGeneration
{
    class ListTypeHandler : IRecursiveTypeHandler
    {
        private ITypeHandler m_RootHandler;

        public bool CanGetInstantiation(Type type)
        {
            return type.IsGenericType
                   && (type.GetGenericTypeDefinition() == typeof(List<>)
                       || type.GetGenericTypeDefinition() == typeof(IList<>));
        }

        public string GetInstantiation(Type type, bool interestingValue)
        {
            var elementType = type.GetGenericArguments().First();
            return $"new List<{m_RootHandler.GetNameForCSharp(elementType)}>()";
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

        public void SetRoot(ITypeHandler handler)
        {
            m_RootHandler = handler;
        }
    }
}