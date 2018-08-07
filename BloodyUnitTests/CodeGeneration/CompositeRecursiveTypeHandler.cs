using System;
using System.Collections.Generic;
using System.Linq;

namespace BloodyUnitTests.CodeGeneration
{
    class CompositeRecursiveTypeHandler : IRecursiveTypeHandler
    {
        private readonly IReadOnlyCollection<IRecursiveTypeHandler> m_InnerHandlers;

        public CompositeRecursiveTypeHandler(IReadOnlyCollection<IRecursiveTypeHandler> innerHandlers)
        {
            m_InnerHandlers = innerHandlers ?? throw new ArgumentNullException(nameof(innerHandlers));
            if (innerHandlers.Any(h => h == null))
            {
                throw new ArgumentException($@"{nameof(innerHandlers)} cannot contain null",
                                            nameof(innerHandlers));
            }

            foreach (var handler in innerHandlers) handler.SetRoot(this);
        }

        public void SetRoot(ITypeHandler handler)
        {
            foreach (var innerHandler in m_InnerHandlers)
            {
                innerHandler.SetRoot(handler);
            }
        }

        public bool CanGetInstantiation(Type type)
        {
            return m_InnerHandlers.Any(h => h.CanGetInstantiation(type));
        }

        public string GetInstantiation(Type type, bool interestingValue)
        {
            return m_InnerHandlers.First(h => h.CanGetInstantiation(type))
                                  .GetInstantiation(type, interestingValue);
        }

        public bool IsInstantiationTerse(Type type)
        {
            return m_InnerHandlers.First(h => h.CanGetInstantiation(type))
                                  .IsInstantiationTerse(type);
        }

        public bool CanGetNameForIdentifier(Type type)
        {
            return m_InnerHandlers.Any(h => h.CanGetNameForIdentifier(type));
        }

        public string GetNameForIdentifier(Type type)
        {
            return m_InnerHandlers.First(h => h.CanGetNameForIdentifier(type))
                                  .GetNameForIdentifier(type);
        }

        public bool CanGetNameForCSharp(Type type)
        {
            return m_InnerHandlers.Any(h => h.CanGetNameForCSharp(type));
        }

        public string GetNameForCSharp(Type type)
        {
            return m_InnerHandlers.First(h => h.CanGetNameForCSharp(type))
                                  .GetNameForCSharp(type);
        }
    }
}