using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BloodyUnitTests.CodeGeneration;

namespace BloodyUnitTests
{
    internal class CSharpWriter
    {
        private readonly ITypeHandler m_TypeHandler = TypeHandlerFactory.Create();

        public bool HasParamKeyword(ParameterInfo arg)
        {
            return arg.IsOut || arg.ParameterType.IsByRef;
        }

        private string ParamKeyword(ParameterInfo arg)
        {
            if (arg.IsOut) return "out";
            if (arg.ParameterType.IsByRef) return "ref";
            throw new InvalidOperationException("Parameter is neither 'out' or 'ref'");
        }

        public string GetIdentifier(string typeName, VarScope varScope)
        {
            switch (varScope)
            {
                case VarScope.Local:
                    return StringUtils.ToLowerInitial(typeName);
                case VarScope.Member:
                    return StringUtils.ToUpperInitial(typeName);
                default:
                    throw new ArgumentOutOfRangeException(nameof(varScope), varScope, null);
            }
        }

        public string GetIdentifier(Type type, VarScope varScope)
        {
            switch (varScope)
            {
                case VarScope.Local:
                    return StringUtils.ToLowerInitial(GetIdentifier(type));
                case VarScope.Member:
                    return StringUtils.ToUpperInitial(GetIdentifier(type));
                default:
                    throw new ArgumentOutOfRangeException(nameof(varScope), varScope, null);
            }
        }

        private bool IsInstantiationTerse(Type type)
        {
            return m_TypeHandler.IsInstantiationTerse(type);
        }

        public string GetInstantiation(Type type)
        {
            return m_TypeHandler.GetInstantiation(type, false);
        }

        public string GetInstantiation(Type possibleReftype, bool nonDefault)
        {
            return m_TypeHandler.GetInstantiation(possibleReftype, nonDefault);
        }

        private string GetLocalVariableDeclaration(Type type, bool setToNull)
        {
            var declaredType = $"{(setToNull ? GetNameForCSharp(type) : "var")}";
            var identifier = GetIdentifier(type, VarScope.Local);
            // ReSharper disable once PossibleNullReferenceException
            if (setToNull && type.IsValueType) return $"{declaredType} {identifier};";
            if (setToNull) return $"{declaredType} {identifier} = null;";
            return $"{declaredType} {identifier} = {GetInstantiation(type)};";
        }

        public string GetMockInstance(Type interfaceType)
        {
            return $"GenerateMock<{GetNameForCSharp(interfaceType)}>()";
        }

        public string GetNameForCSharp(Type type)
        {
            return m_TypeHandler.GetNameForCSharp(type);
        }

        public string[] GetStubbedInstantiation(Type type)
        {
            var sb = new StringBuilder();

            var ctor = type.GetConstructors()
                           .FirstOrDefault() ??
                       type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                           .First();

            var arguments = GetMethodArguments(ctor, useVariables: false, nonDefault: false);

            var declarationStart = $"var {GetIdentifier(type, VarScope.Local)} = new {GetNameForCSharp(type)}(";

            var offset = new string(' ', declarationStart.Length);

            sb.Append(declarationStart);
            for (int i = 0; i < arguments.Length; i++)
            {
                sb.Append(arguments[i]);
                if (i < arguments.Length - 1)
                {
                    sb.Append($",{Environment.NewLine}{offset}");
                }
            }
            sb.AppendLine(");");

            return sb.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        }

        public string[] GetVariableDeclarationsForParameters(IEnumerable<ParameterInfo> parameters, bool setToNull)
        {
            return parameters.Where(p => !p.ParameterType.IsValueType
                                         || HasParamKeyword(p)
                                         || p.ParameterType.IsEnum
                                         || p.ParameterType == typeof(DateTime)
                                         || p.ParameterType == typeof(DateTime?))
                             .Where(p => !IsInstantiationTerse(p.ParameterType) || HasParamKeyword(p))
                             .Select(p => p.ParameterType)
                             .Select(t => t.IsByRef ? t.GetElementType() : t)
                             .Distinct()
                             .Select(t => GetLocalVariableDeclaration(t, setToNull))
                             .ToArray();
        }

        public bool CanInstantiate(Type type)
        {
            return CanInstantiate(type, new List<Type>());
        }

        /// <summary>
        /// Returns true for types which this class can construct.
        /// </summary>
        private bool CanInstantiate(Type type, ICollection<Type> typeHistory)
        {
            if (m_TypeHandler.CanGetInstantiation(type)) return true;

            // No circular dependencies
            if (typeHistory.Contains(type)) return false;
            typeHistory.Add(type);

            // Deal with classes
            if (!type.IsClass) return false;

            var ctor = type.GetConstructors()
                           .OrderByDescending(c => c.GetParameters().Length)
                           .FirstOrDefault();
            if (ctor == null) return false;

            return ctor.GetParameters()
                       .All(p => !p.IsOut && CanInstantiate(p.ParameterType, typeHistory));
        }

        public string[] GetMethodArguments(MethodBase methodBase, bool useVariables, bool nonDefault)
        {
            var parameters = methodBase.GetParameters();

            var arguments = useVariables
                ? parameters.Select(p => p.ParameterType).Select(t => StringUtils.ToLowerInitial(m_TypeHandler.GetNameForIdentifier(t))).ToArray()
                : parameters.Select(p => p.ParameterType).Select(t => m_TypeHandler.GetInstantiation(t, nonDefault)).ToArray();

            for (var index = 0; index < parameters.Length; index++)
            {
                var pInfo = parameters[index];
                var pType = pInfo.ParameterType;
                // MUST tie up with ShouldUseVariableForParameter()
                if (HasParamKeyword(pInfo))
                {
                    var argument = arguments[index];
                    arguments[index] = $"{ParamKeyword(pInfo)} {argument}";
                }
                else if (IsInstantiationTerse(pType))
                {
                    arguments[index] = GetInstantiation(pType, nonDefault);
                }
            }

            return arguments;
        }

        public bool ShouldUseVariableForParameter(ParameterInfo parameter)
        {
            return HasParamKeyword(parameter)
                   || !IsInstantiationTerse(parameter.ParameterType);
        }

        public bool IsArrayAssignable(Type type)
        {
            return GetArrayElementType(type) != null;
        }

        public Type GetArrayElementType(Type type)
        {
            var gArgs = type.GetGenericArguments();
            if (!type.HasElementType && gArgs.Length != 1) return null;
            var elementType = gArgs.SingleOrDefault() ?? type.GetElementType();
            // ReSharper disable once PossibleNullReferenceException
            return type.IsAssignableFrom(elementType.MakeArrayType()) ? elementType : null;
        }

        private string GetIdentifier(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            return m_TypeHandler.GetNameForIdentifier(type);
        }
    }

    enum VarScope { Local, Member }
}
