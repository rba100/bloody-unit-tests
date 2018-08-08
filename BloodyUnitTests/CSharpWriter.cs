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

        public string GetTypeNameForIdentifier(string typeName, VarScope varScope)
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

        public string GetTypeNameForIdentifier(Type type, VarScope varScope)
        {
            switch (varScope)
            {
                case VarScope.Local:
                    return StringUtils.ToLowerInitial(GetTypeNameForIdentifier(type));
                case VarScope.Member:
                    return StringUtils.ToUpperInitial(GetTypeNameForIdentifier(type));
                default:
                    throw new ArgumentOutOfRangeException(nameof(varScope), varScope, null);
            }
        }

        private bool IsInstantiationTerse(Type type)
        {
            return m_TypeHandler.IsInstantiationTerse(type);
        }

        public string GetInstance(Type type)
        {
            return m_TypeHandler.GetInstantiation(type, false);
        }

        public string GetInstance(Type possibleReftype, bool nonDefault)
        {
            return m_TypeHandler.GetInstantiation(possibleReftype, nonDefault);
        }

        public string GetLocalVariableDeclaration(Type type, bool setToNull)
        {
            var declaredType = $"{(setToNull ? GetTypeNameForCSharp(type) : "var")}";
            var identifier = GetTypeNameForIdentifier(type, VarScope.Local);
            // ReSharper disable once PossibleNullReferenceException
            if (setToNull && type.IsValueType) return $"{declaredType} {identifier};";
            if (setToNull) return $"{declaredType} {identifier} = null;";
            return $"{declaredType} {identifier} = {GetInstance(type)};";
        }

        public string GetMockInstance(Type interfaceType)
        {
            return $"GenerateMock<{GetTypeNameForCSharp(interfaceType)}>()";
        }

        public string GetTypeNameForCSharp(Type type)
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

            var declarationStart = $"var {GetTypeNameForIdentifier(type, VarScope.Local)} = new {GetTypeNameForCSharp(type)}(";

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
            if (type.IsValueType) return true;
            if (type == typeof(string)) return true;
            if (IsArrayAssignable(type)) return true;
            if (IsDictionaryAssignable(type)) return true;
            if (type.IsInterface) return true;
            if (type.IsAbstract) return false;

            // No circular dependencies
            if (typeHistory.Contains(type)) return false;
            typeHistory.Add(type);

            if (type.IsArray && CanInstantiate(type.GetElementType(), typeHistory)) return true;

            // Deal with classes
            if (!type.IsClass) return false;
            // Deal with certain types that we have specially handle
            if (IsFuncActionOrList(type)) return true;

            var ctor = type.GetConstructors()
                           .OrderByDescending(c => c.GetParameters().Length)
                           .FirstOrDefault();
            if (ctor == null) return false;

            return ctor.GetParameters()
                       .All(p => !p.IsOut && CanInstantiate(p.ParameterType, typeHistory));
        }

        private bool IsFuncActionOrList(Type type)
        {
            var genArgs = type.GetGenericArguments();
            if (genArgs.Length != 1) return false;
            var handledTypes = new[] {
                typeof(Func<>).MakeGenericType(genArgs),
                typeof(Action<>).MakeGenericType(genArgs),
                typeof(List<>).MakeGenericType(genArgs)
            };
            return handledTypes.Any(type.IsAssignableFrom);
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
                    arguments[index] = GetInstance(pType, nonDefault);
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

        private string GetTypeNameForIdentifier(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            return m_TypeHandler.GetNameForIdentifier(type);
        }
    }

    enum VarScope { Local, Member }
}
