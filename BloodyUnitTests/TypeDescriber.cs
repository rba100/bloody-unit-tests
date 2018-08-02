using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BloodyUnitTests
{
    internal class TypeDescriber
    {
        public bool HasParamKeyword(ParameterInfo arg)
        {
            return arg.IsOut || arg.ParameterType.IsByRef;
        }

        public string ParamKeyword(ParameterInfo arg)
        {
            if (arg.IsOut) return "out";
            if (arg.ParameterType.IsByRef) return "ref";
            throw new InvalidOperationException("Parameter is neither 'out' or 'ref'");
        }

        public string GetVariableName(string typeName, Scope scope)
        {
            switch (scope)
            {
                case Scope.Local:
                    return ToLowerInitial(typeName);
                case Scope.Member:
                    return ToUpperInitial(typeName);
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
            }
        }

        public string GetVariableName(Type type, Scope scope)
        {
            switch (scope)
            {
                case Scope.Local:
                    return GetLocalVariableName(type);
                case Scope.Member:
                    return GetVariableName(type);
                default:
                    throw new ArgumentOutOfRangeException(nameof(scope), scope, null);
            }
        }

        /// <summary>
        /// When a value is needed as an argument to a function, can we just use
        /// a literal or in-line instantiation for the supplied type or should we create a variable first?
        /// </summary>
        public bool IsImmediateValueTolerable(Type type)
        {
            return type == typeof(char)
                || type == typeof(string)
                || type == typeof(int)
                || type == typeof(uint)
                || type == typeof(Int64)
                || type == typeof(UInt64)
                || type == typeof(float)
                || type == typeof(double)
                || type == typeof(decimal)
                || type == typeof(bool)
                || IsArrayAssignable(type);
        }

        public string GetInstance(Type possibleReftype)
        {
            return GetInstance(possibleReftype, false);
        }

        public string GetInstance(Type possibleReftype, bool nonDefault)
        {
            if (possibleReftype == null) throw new ArgumentNullException(nameof(possibleReftype));

            var type = possibleReftype.IsByRef
                ? possibleReftype.GetElementType()
                : possibleReftype;

            if (IsArrayAssignable(type))
            {
                if (type == typeof(Type[])) return nonDefault ? "new Type[] { typeof(string) }" : "Type.EmptyTypes";
                // ReSharper disable once PossibleNullReferenceException
                var gArgs = type.GetGenericArguments();
                var elementType = gArgs.SingleOrDefault() ?? type.GetElementType();

                return nonDefault
                    ? $"new {GetTypeNameForCSharp(elementType)}[] {{ {GetInstance(elementType, true)} }}"
                    : $"new {GetTypeNameForCSharp(elementType)}[0]";
            }

            if (type == typeof(string))
                return nonDefault ? "\"value\"" : "\"\"";

            if (type == typeof(char))
                return nonDefault ? "X" : "' '";

            if (type == typeof(int)
                || type == typeof(uint)
                || type == typeof(Int64)
                || type == typeof(UInt64)
                || type == typeof(float)
                || type == typeof(double)
                || type == typeof(decimal)) return nonDefault ? "7" : "0";

            if (type == typeof(bool)) return nonDefault.ToString().ToLower();

            if (type == typeof(IntPtr)) return nonDefault ? "new IntPtr(1)" : "IntPtr.Zero";

            if (type == typeof(Type)) return "typeof(object)";

            // ReSharper disable once PossibleNullReferenceException
            if (type.IsInterface)
                return $"MockRespository.GenerateStub<{GetTypeNameForCSharp(type)}>()";

            if (type.IsClass)
            {
                if (type.IsGenericType)
                {
                    var genArgs = type.GetGenericArguments().FirstOrDefault() ?? typeof(object);

                    // List<T>
                    var listType = typeof(List<>).MakeGenericType(genArgs);
                    if (type.IsAssignableFrom(listType)) return $"new {GetTypeNameForCSharp(listType)}()";
                    // Func<T>
                    var funcType = typeof(Func<>).MakeGenericType(genArgs);
                    if (type.IsAssignableFrom(funcType)) return $"() => {GetInstance(genArgs)}";
                    // Action<T>
                    var actionType = typeof(Action<>).MakeGenericType(genArgs);
                    if (type.IsAssignableFrom(actionType)) return $"(_) => {{}}";
                }

                // If it's a POCO type then we will assume there is a helper method called 'CreateThing()'
                if (IsPoco(type)) return $"Create{GetVariableName(type, Scope.Member)}()";

                // If it has a parameterless constructor then we have an easy way out.
                if (type.GetConstructor(Type.EmptyTypes) != null) return $"new {GetTypeNameForCSharp(type)}()";

                // Fallback: prepare an non-compiling instantiation and let the user fix it
                return $"new {GetTypeNameForCSharp(type)}(/* ... */)";
            }

            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null) return $"({nullableType.Name}?) {GetInstance(nullableType)}";

            if (type == typeof(DateTime)) return nonDefault
                ? "DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc)"
                : "DateTime.UtcNow";

            if (type.IsEnum)
            {
                var names = Enum.GetNames(type);
                return nonDefault && names.Length > 1 ? $"{type.Name}.{names[1]}" : $"{type.Name}.{names[0]}";
            }

            return $"default({GetTypeNameForCSharp(type)})";
        }

        public string GetLocalVariableDeclaration(Type rawType, bool setToNull)
        {
            var type = rawType.IsByRef ? rawType.GetElementType() : rawType;
            var declaredType = $"{(setToNull ? GetTypeNameForCSharp(type) : "var")}";
            var identifier = GetVariableName(type, Scope.Local);

            // ReSharper disable once PossibleNullReferenceException
            if (setToNull && type.IsValueType) return $"{declaredType} {identifier};";
            if (setToNull) return $"{declaredType} {identifier} = null;";
            return $"{declaredType} {identifier} = {GetInstance(type)};";
        }

        public string GetMockVariableDeclaration(Type interfaceType)
        {
            return $"var mock{GetVariableName(interfaceType, Scope.Member)} = {GetMockInstance(interfaceType)}";
        }

        private string GetMockInstance(Type interfaceType)
        {
            return $"MockRepository.GenerateMock<{GetTypeNameForCSharp(interfaceType)}>();";
        }

        public string GetTypeNameForCSharp(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var unRefType = type.IsByRef ? type.GetElementType() : type;

            // ReSharper disable once AssignNullToNotNullAttribute
            var nullableType = Nullable.GetUnderlyingType(unRefType);
            if (nullableType != null) return $"{GetTypeNameForCSharp(nullableType)}?";

            var typeDisplayName = unRefType.Name;
            if (unRefType.IsGenericType)
            {
                var genericParams = unRefType.GetGenericArguments();
                int index = typeDisplayName.IndexOf('`');
                typeDisplayName = index == -1 ? typeDisplayName : typeDisplayName.Substring(0, index);
                typeDisplayName = typeDisplayName + "<" + string.Join(", ", genericParams.Select(GetTypeNameForCSharp)) + ">";
            }

            return typeDisplayName;
        }

        public bool IsPoco(Type type)
        {
            if (!type.IsClass || type.IsAbstract) return false;

            return IsSimpleType(type, new List<Type>());
        }

        public string[] GetStubbedInstantiation(Type type)
        {
            var sb = new StringBuilder();

            var variableName = GetVariableName(type, Scope.Local);
            var nameForCSharp = GetTypeNameForCSharp(type);
            var ctor = type.GetConstructors()
                           .FirstOrDefault() ??
                       type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                           .First();

            var arguments = GetMethodArguments(ctor, useVariables: false, nonDefault: false);

            var declarationStart = $"var {variableName} = new {nameForCSharp}(";

            var offset = new string(' ', declarationStart.Length);

            sb.Append(declarationStart);
            for (int i = 0; i < arguments.Length; i++)
            {
                sb.Append(arguments[i]);
                if (i < arguments.Length - 1)
                {
                    sb.AppendLine(",");
                    sb.Append(offset);
                }
            }
            sb.AppendLine(");");

            return sb.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        }

        public string[] GetNeededVariableDeclarations(IEnumerable<ParameterInfo> parameters, bool setToNull)
        {
            return parameters.Where(p => !p.ParameterType.IsValueType || HasParamKeyword(p) || p.ParameterType.IsEnum)
                             .Where(p => !IsImmediateValueTolerable(p.ParameterType) || HasParamKeyword(p))
                             .Select(p => p.ParameterType)
                             .Distinct()
                             .Select(t => GetLocalVariableDeclaration(t, setToNull))
                             .ToArray();
        }

        /// <summary>
        /// Returns true for 'simple types', which are defined as
        /// value types, arrays, and classes which only
        /// require simple types for their most argumentative constructor.
        /// </summary>
        private bool IsSimpleType(Type type, IList<Type> typeHistory)
        {
            if (type.IsValueType) return true;
            if (type == typeof(string)) return true;
            if (IsArrayAssignable(type)) return true;
            if (!type.IsClass) return false;

            if (typeHistory.Contains(type)) return false;
            typeHistory.Add(type);

            if (type.IsArray && IsSimpleType(type.GetElementType(), typeHistory)) return true;

            var ctor = type.GetConstructors()
                           .OrderByDescending(c => c.GetParameters().Length)
                           .FirstOrDefault();

            if (ctor == null) return false;

            return ctor.GetParameters()
                       .All(p => !p.IsOut && IsSimpleType(p.ParameterType, typeHistory));
        }

        public string[] GetMethodArguments(MethodBase methodBase, bool useVariables, bool nonDefault)
        {
            var parameters = methodBase.GetParameters();

            var arguments = useVariables
                ? parameters.Select(p => p.ParameterType).Select(t => GetVariableName(t, Scope.Local)).ToArray()
                : parameters.Select(p => p.ParameterType).Select(t => GetInstance(t, nonDefault)).ToArray();

            for (var index = 0; index < parameters.Length; index++)
            {
                var pInfo = parameters[index];
                var type = pInfo.ParameterType;
                // Add ref/out keywords where needed
                if (HasParamKeyword(pInfo))
                {
                    var argument = arguments[index];
                    arguments[index] = $"{ParamKeyword(pInfo)} {argument}";
                }
                // If not a ref then check if we can use a literal / immediate value instead of variable
                else if (IsImmediateValueTolerable(type))
                {
                    arguments[index] = GetInstance(type, nonDefault);
                }
            }

            return arguments;
        }

        public bool ShouldUseVariableForParameter(ParameterInfo parameter)
        {
            return !parameter.IsOut
                && !parameter.ParameterType.IsByRef
                && !IsImmediateValueTolerable(parameter.ParameterType);
        }

        private static bool IsArrayAssignable(Type type)
        {
            var gArgs = type.GetGenericArguments();
            if (!type.HasElementType && gArgs.Length != 1) return false;
            var elementType = gArgs.SingleOrDefault() ?? type.GetElementType();
            // ReSharper disable once PossibleNullReferenceException
            return type.IsAssignableFrom(elementType.MakeArrayType());
        }

        private string GetVariableName(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var unRefType = type.IsByRef ? type.GetElementType() : type;

            // ReSharper disable once AssignNullToNotNullAttribute
            var nullableType = Nullable.GetUnderlyingType(unRefType);
            if (nullableType != null) return $"{GetVariableName(nullableType)}Nullable";

            var typeDisplayName = unRefType.Name;
            if (unRefType.IsGenericType)
            {
                var genericParams = unRefType.GetGenericArguments();
                int index = typeDisplayName.IndexOf('`');
                typeDisplayName = index == -1 ? typeDisplayName : typeDisplayName.Substring(0, index);
                typeDisplayName = typeDisplayName + string.Join("", genericParams.Select(GetVariableName));
            }

            if (unRefType.IsInterface && typeDisplayName.StartsWith("I"))
            {
                return new string(typeDisplayName.Skip(1).ToArray());
            }

            return typeDisplayName;
        }

        private string GetLocalVariableName(Type type)
        {
            var varName = GetVariableName(type);
            var prefix = type.IsInterface ? "stub" : "dummy";
            var unRefType = type.IsByRef ? type.GetElementType() : type;
            if (type.IsClass && type.Namespace != "System") return ToLowerInitial(varName);
            if (unRefType == typeof(DateTime)) return "dateTimeUtc";
            var output = ToLowerInitial($"{prefix}{varName}");
            return output == type.Name ? output + "1" : output;
        }

        private string ToLowerInitial(string str)
        {
            return str[0].ToString().ToLower() + new string(str.Skip(1).ToArray());
        }

        private string ToUpperInitial(string str)
        {
            return str[0].ToString().ToUpper() + new string(str.Skip(1).ToArray());
        }
    }

    enum Scope { Local, Member }
}
