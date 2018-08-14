using System;
using System.Linq;

namespace BloodyUnitTests.CodeGeneration
{
    class ValueTupleTypeHandler : IRecursiveTypeHandler
    {
        private ITypeHandler m_RootHandler;

        private static readonly Type s_2 = typeof(ValueTuple<,>);
        private static readonly Type s_3 = typeof(ValueTuple<,,>);
        private static readonly Type s_4 = typeof(ValueTuple<,,,>);
        private static readonly Type s_5 = typeof(ValueTuple<,,,,>);
        private static readonly Type s_6 = typeof(ValueTuple<,,,,,>);
        private static readonly Type s_7 = typeof(ValueTuple<,,,,,,>);
        private static readonly Type s_8 = typeof(ValueTuple<,,,,,,,>);

        private static readonly Type[] s_HandledTypes = { s_2, s_3, s_4, s_5, s_6, s_7, s_8 };

        public bool CanGetInstantiation(Type type)
        {
            return type.IsGenericType && s_HandledTypes.Contains(type.GetGenericTypeDefinition());
        }

        public string GetInstantiation(Type type, bool interestingValue)
        {
            var genArgs = type.GetGenericArguments();
            var instances = genArgs.Select(t=>m_RootHandler.GetInstantiation(t,interestingValue));
            return $"({string.Join(", ", instances)})";
        }

        public bool IsInstantiationTerse(Type type)
        {
            var instance = GetInstantiation(type, true);
            return instance.Length <= 15;
        }

        public bool CanGetNameForIdentifier(Type type)
        {
            return CanGetInstantiation(type);
        }

        public string GetNameForIdentifier(Type type)
        {
            var genArgs = type.GetGenericArguments();
            var typeNames = string.Join("", genArgs.Select(m_RootHandler.GetNameForIdentifier));
            return $"{typeNames}";
        }

        public bool CanGetNameForCSharp(Type type)
        {
            return CanGetInstantiation(type);
        }

        public string GetNameForCSharp(Type type)
        {
            var genArgs = type.GetGenericArguments();
            var typeNames = string.Join(", ", genArgs.Select(m_RootHandler.GetNameForCSharp));
            return $"({typeNames})";
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