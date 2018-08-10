using System;
using System.Linq;

namespace BloodyUnitTests.CodeGeneration
{
    class SimpleDelegateTypeHandler : IRecursiveTypeHandler
    {
        private ITypeHandler m_RootHandler;

        private static readonly Type s_FuncType = typeof(Func<>);
        private static readonly Type s_ActionType = typeof(Action<>);

        private bool CanHandle(Type type)
        {
            if (!type.IsGenericType) return false;
            var genArgs = type.GetGenericArguments().FirstOrDefault() ?? typeof(object);
            var funcType = s_FuncType.MakeGenericType(genArgs);
            var actionType = s_ActionType.MakeGenericType(genArgs);
            return type.IsAssignableFrom(funcType) || type.IsAssignableFrom(actionType);
        }

        public bool CanGetInstantiation(Type type)
        {
            return CanHandle(type);
        }

        public string GetInstantiation(Type type, bool interestingValue)
        {
            var genArg = type.GetGenericArguments().FirstOrDefault() ?? typeof(object);
            var funcType = s_FuncType.MakeGenericType(genArg);
            if (type.IsAssignableFrom(funcType)) return $"() => {m_RootHandler.GetInstantiation(genArg, interestingValue)}";
            return $"_ => {{ }}";
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