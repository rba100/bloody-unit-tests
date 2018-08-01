using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BloodyUnitTests
{
    class TypeDescriber
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

        public string GetVariableName(ParameterInfo info, Scope scope)
        {
            switch (scope)
            {
                case Scope.Local:
                    return ToLowerInitial(info.Name);
                case Scope.Member:
                    return ToUpperInitial(info.Name);
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

        /// <summary>
        /// Returns a string that instantiates the given type. Value types may use default literals.
        /// </summary>
        /// <remarks>
        /// For complex types that do not have simple invocations it is assumed a helper method exists 
        /// e.g. GetInstance(typeof(MyType)) => "CreateMyType()"
        /// </remarks>
        /// <example>
        /// GetInstance(typeof(Int32)) => "0"
        /// GetInstance(typeof(IDisposable)) => "GenerateStub[IDisposable]()"
        /// </example>
        public string GetInstance(Type possibleReftype)
        {
            if (possibleReftype == null) throw new ArgumentNullException(nameof(possibleReftype));

            var type = possibleReftype.IsByRef
                ? possibleReftype.GetElementType()
                : possibleReftype;

            if (IsArrayAssignable(type))
            {
                // ReSharper disable once PossibleNullReferenceException
                var gArgs = type.GetGenericArguments();
                var elementType = gArgs.SingleOrDefault() ?? type.GetElementType();
                return $"new {GetTypeNameForCSharp(elementType)}[0]";
            }

            if (type == typeof(string))
                return "\"\"";

            if (type == typeof(char))
                return "' '";

            if (type == typeof(int)
                || type == typeof(uint)
                || type == typeof(Int64)
                || type == typeof(UInt64)
                || type == typeof(float)
                || type == typeof(double)
                || type == typeof(decimal)) return "0";

            if (type == typeof(bool)) return "false";

            if (type == typeof(IntPtr)) return "IntPtr.Zero";

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
                // Otherwise prepare an instantiation but let the user fill out the arguments later
                return $"new {GetTypeNameForCSharp(type)}(/* ... */)";
            }

            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null) return $"({nullableType.Name}?) {GetInstance(nullableType)}";

            if (type == typeof(DateTime)) return "DateTime.UtcNow";

            return $"default(typeof({GetTypeNameForCSharp(type)}))";
        }

        public string GetTypeNameForCSharp(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var unRefType = type.IsByRef ? type.GetElementType() : type;

            // ReSharper disable once AssignNullToNotNullAttribute
            var nullableType = Nullable.GetUnderlyingType(unRefType);
            if (nullableType != null) return $"{nullableType.Name}?";

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

            // Recurses all dependencies, allowing value types and array types to count as 'POCO'
            return IsPocoOrValueType(type, new List<Type>());
        }

        private bool IsPocoOrValueType(Type type, IList<Type> typeHistory)
        {
            if (type.IsValueType) return true;
            if (type == typeof(string)) return true;
            if (IsArrayAssignable(type)) return true;
            if (!type.IsClass) return false;

            if (typeHistory.Contains(type)) return false;
            typeHistory.Add(type);

            if (type.IsArray && IsPocoOrValueType(type.GetElementType(), typeHistory)) return true;

            var ctor = type.GetConstructors()
                           .OrderByDescending(c => c.GetParameters().Length)
                           .FirstOrDefault();

            if (ctor == null) return false;

            return ctor.GetParameters()
                       .All(p => !p.IsOut && IsPocoOrValueType(p.ParameterType, typeHistory));
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
