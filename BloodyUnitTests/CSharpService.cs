using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using BloodyUnitTests.CodeGeneration;
using BloodyUnitTests.Util;

namespace BloodyUnitTests
{
    internal class CSharpService
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

        public string GetIdentifier(Type type, VarScope varScope)
        {
            switch (varScope)
            {
                case VarScope.Local:
                    return GetIdentifier(type);
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

        public string GetLocalVariableDeclaration(Type type, bool setToNull, bool nonDefault)
        {
            var declaredType = $"{(setToNull || TypeIsFunction(type) ? GetNameForCSharp(type) : "var")}";
            var identifier = GetIdentifier(type, VarScope.Local);
            // ReSharper disable once PossibleNullReferenceException
            if (setToNull && type.IsValueType) return $"{declaredType} {identifier};";
            if (setToNull) return $"{declaredType} {identifier} = null;";
            return $"{declaredType} {identifier} = {GetInstantiation(type, nonDefault)};";
        }

        private bool TypeIsFunction(Type type)
        {
            if (!type.IsGenericType) return false;
            var genTypeDef = type.GetGenericTypeDefinition();
            return genTypeDef == typeof(Func<>) || genTypeDef == typeof(Action<>);
        }

        public string GetMockInstance(Type interfaceType)
        {
            m_TypeHandler.GetNamespaceTracker().RecordNamespace("Rhino.Mocks");
            m_TypeHandler.GetNamespaceTracker().RecordNamespace("static Rhino.Mocks.MockRepository");
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
                if (i != 0) sb.Append($",{Environment.NewLine}{offset}");
                sb.Append(arguments[i]);
            }
            sb.AppendLine(");");

            return sb.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        }

        public string[] GetVariableDeclarationsForParameters(IEnumerable<ParameterInfo> parameters, bool setToNull, bool nonDefault)
        {
            return parameters.Where(ShouldUseVariableForParameter)
                             .Select(p => p.ParameterType)
                             .Select(t => t.IsByRef ? t.GetElementType() : t)
                             .Distinct()
                             .Select(t => GetLocalVariableDeclaration(t, setToNull, nonDefault))
                             .ToArray();
        }

        public bool NoCircularDependenciesOrAbstract(Type type)
        {
            return NoCircularDependenciesOrAbstract(type, new List<Type>());
        }

        /// <summary>
        /// Returns true for types which this class can construct.
        /// </summary>
        private bool NoCircularDependenciesOrAbstract(Type type, ICollection<Type> typeHistory)
        {
            if (type == typeof(string)) return true;

            if (type.IsClass && type.IsAbstract) return false;
            if (!type.IsClass) return true;

            // No circular dependencies
            if (typeHistory.Contains(type)) return false;
            typeHistory.Add(type);

            var ctor = type.GetConstructors()
                           .OrderBy(c => c.GetParameters().Length)
                           .FirstOrDefault();
            if (ctor == null) return false;

            return ctor.GetParameters()
                       .All(p => !p.IsOut && NoCircularDependenciesOrAbstract(p.ParameterType, typeHistory));
        }

        public string[] GetMethodArguments(MethodBase methodBase, bool useVariables, bool nonDefault)
        {
            var parameters = methodBase.GetParameters();

            var paramterTypesClean = parameters
                .Select(p => p.ParameterType.IsByRef
                            ? p.ParameterType.GetElementType()
                            : p.ParameterType);

            var arguments = useVariables
                ? paramterTypesClean.Select(t => StringUtils.ToLowerInitial(m_TypeHandler.GetNameForIdentifier(t))).ToArray()
                : paramterTypesClean.Select(t => m_TypeHandler.GetInstantiation(t, nonDefault)).ToArray();

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

        public string[] GetNameSpaces()
        {
            return m_TypeHandler.GetNamespaceTracker().GetNamespaces();
        }
    }

    enum VarScope { Local, Member }
}
