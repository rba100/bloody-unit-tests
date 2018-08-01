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

        public string GetVariableName(Type type, Scope scope)
        {
            return scope == Scope.Local
                ? ToLowerInitial(GetTypeNameForIdentifier(type))
                : GetTypeNameForIdentifier(type);
        }

        public string GetDummyVariableName(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var unRefType = type.IsByRef ? type.GetElementType() : type;

            // ReSharper disable once AssignNullToNotNullAttribute
            var nullableType = Nullable.GetUnderlyingType(unRefType);

            var name = unRefType.Name;
            if (unRefType.IsGenericType)
            {
                var genericParams = unRefType.GetGenericArguments();
                int index = name.IndexOf('`');
                name = index == -1 ? name : name.Substring(0, index);
                name = name + string.Join("", genericParams.Select(p => p.Name));
            }

            if (nullableType != null) return $"{GetDummyVariableName(nullableType)}Nullable";
            if (type.IsInterface)
            {
                if (type.Name.StartsWith("I")) return $"stub{GetTypeNameForIdentifier(type)}";
                return $"stub{GetTypeNameForIdentifier(type)}";
            }
            if (type.IsClass && type.Namespace != "System") return ToLowerInitial(name);
            if (unRefType == typeof(DateTime)) return "dummyDateTimeUtc";
            return $"dummy{name}";
        }

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

        public string GetDummyInstantiation(Type possibleReftype)
        {
            if (possibleReftype == null) throw new ArgumentNullException(nameof(possibleReftype));

            var type = possibleReftype.IsByRef
                ? possibleReftype.GetElementType()
                : possibleReftype;

            if (IsArrayAssignable(type))
            {
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

            // ReSharper disable once PossibleNullReferenceException
            if (type.IsInterface)
                return $"MockRespository.GenerateStub<{GetTypeNameForCSharp(type)}>()";

            if (type.IsClass)
            {
                if (IsPoco(type)) return $"Create{GetTypeNameForCSharp(type)}()";
                return $"new {GetTypeNameForCSharp(type)}(/* ... */)";
            }

            var nullableType = Nullable.GetUnderlyingType(type);
            if (nullableType != null) return $"({nullableType.Name}?) {GetDummyInstantiation(nullableType)}";

            if (type == typeof(DateTime)) return "DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc)";

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
            var name = type.Name;
            if (!type.IsClass || type.IsAbstract) return false;

            return IsPocoOrValueType(type, new List<Type>());
        }

        private bool IsPocoOrValueType(Type type, IList<Type> typeHistory)
        {
            if (type.IsValueType) return true;
            if (type == typeof(string)) return true;
            if (!type.IsClass) return false;
            if (IsArrayAssignable(type)) return true;

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

        public static bool IsArrayAssignable(Type type)
        {
            var gArgs = type.GetGenericArguments();
            if (!type.HasElementType && gArgs.Length != 1) return false;
            var elementType = gArgs.SingleOrDefault() ?? type.GetElementType();
            // ReSharper disable once PossibleNullReferenceException
            return type.IsAssignableFrom(elementType.MakeArrayType());
        }

        private string GetTypeNameForIdentifier(Type type)
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
                typeDisplayName = typeDisplayName + string.Join("", genericParams.Select(GetTypeNameForIdentifier));
            }

            if (unRefType.IsInterface && typeDisplayName.StartsWith("I"))
            {
                return new string(typeDisplayName.Skip(1).ToArray());
            }

            return typeDisplayName;
        }

        private string ToLowerInitial(string str)
        {
            return str[0].ToString().ToLower() + new string(str.Skip(1).ToArray());
        }

    }

    enum Scope { Local, Member }
}
