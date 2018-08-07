using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BloodyUnitTests
{
    internal class CSharpWriter
    {
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
                    return ToLowerInitial(typeName);
                case VarScope.Member:
                    return ToUpperInitial(typeName);
                default:
                    throw new ArgumentOutOfRangeException(nameof(varScope), varScope, null);
            }
        }

        public string GetTypeNameForIdentifier(Type type, VarScope varScope)
        {
            switch (varScope)
            {
                case VarScope.Local:
                    return GetLocalVariableName(type);
                case VarScope.Member:
                    return GetTypeNameForIdentifier(type);
                default:
                    throw new ArgumentOutOfRangeException(nameof(varScope), varScope, null);
            }
        }

        /// <summary>
        /// Returns true if a literal or in-line instantiation for the supplied type
        /// is likely to be terse when a method argument is needed.
        /// </summary>
        private bool IsImmediateValueTolerable(Type type)
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
                if (type == typeof(byte[])) return nonDefault ? "Encoding.UTF8.GetBytes(\"{}\")" : "new byte[0]";
                if (type == typeof(Type[])) return nonDefault ? "new Type[] { typeof(string) }" : "Type.EmptyTypes";

                // ReSharper disable once PossibleNullReferenceException
                var gArgs = type.GetGenericArguments();
                var elementType = gArgs.SingleOrDefault() ?? type.GetElementType();

                return nonDefault
                    ? $"new {GetTypeNameForCSharp(elementType)}[] {{ {GetInstance(elementType, true)} }}"
                    : $"new {GetTypeNameForCSharp(elementType)}[0]";
            }

            if (IsDictionaryAssignable(type))
            {
                var dictionaryTypes = GetDictionaryType(type);
                var keyType = GetTypeNameForCSharp(dictionaryTypes.key);
                var valueType = GetTypeNameForCSharp(dictionaryTypes.value);
                return $"new Dictionary<{keyType}, {valueType}>()";
            }

            if (type == typeof(string))
                return nonDefault ? "\"value\"" : "\"\"";

            if (type == typeof(char))
                return nonDefault ? "X" : "' '";

            if (type == typeof(int)
             || type == typeof(uint)
             || type == typeof(long)
             || type == typeof(ulong)
             || type == typeof(float)
             || type == typeof(double)
             || type == typeof(decimal)) return nonDefault ? "7" : "0";

            if (type == typeof(bool)) return nonDefault.ToString().ToLower();

            if (type == typeof(IntPtr)) return nonDefault ? "new IntPtr(1)" : "IntPtr.Zero";

            if (type == typeof(Type)) return "typeof(object)";

            // ReSharper disable once PossibleNullReferenceException
            if (type.IsInterface)
            {
                if (type.IsGenericType)
                {
                    // Lists cover lots of collection-like interfaces
                    var genArgs = type.GetGenericArguments().FirstOrDefault() ?? typeof(object);
                    var listType = typeof(List<>).MakeGenericType(genArgs);
                    if (type.IsAssignableFrom(listType)) return $"new {GetTypeNameForCSharp(listType)}()";
                }

                return $"GenerateStub<{GetTypeNameForCSharp(type)}>()";
            }

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
                    if (type.IsAssignableFrom(actionType)) return $"(_) => {{ }}";
                }

                // If it has a parameterless constructor but no others then use it
                var gotParameterless = type.GetConstructor(Type.EmptyTypes) != null;
                var onlyParameterless = gotParameterless && type.GetConstructors().Length == 1;

                // If the *only* ctor is parameterless then that's what we've got to do
                if (onlyParameterless) return $"new {GetTypeNameForCSharp(type)}()";

                // If we've got this far with no joy but CanInstantiate still thinks
                // we can handle it then assume we have a helper method elsewhere.
                if (CanInstantiate(type)) return $"Create{GetTypeNameForIdentifier(type, VarScope.Member)}()";

                // OK, fine, we'll use the parameterless ctor if we have one.
                if (gotParameterless) return $"new {GetTypeNameForCSharp(type)}()";

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
                if (names.Any()) return nonDefault && names.Length > 1 
                    ? $"{type.Name}.{names[1]}" 
                    : $"{type.Name}.{names[0]}";
            }

            return $"default({GetTypeNameForCSharp(type)})";
        }

        public string GetLocalVariableDeclaration(Type rawType, bool setToNull)
        {
            var type = rawType.IsByRef ? rawType.GetElementType() : rawType;
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
            if (type == null) throw new ArgumentNullException(nameof(type));

            // C# type keywords
            if (type == typeof(string)) return "string";
            if (type == typeof(object)) return "object";
            if (type == typeof(int)) return "int";
            if (type == typeof(uint)) return "uint";
            if (type == typeof(long)) return "long";
            if (type == typeof(ulong)) return "ulong";
            if (type == typeof(short)) return "short";
            if (type == typeof(ushort)) return "ushort";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(decimal)) return "decimal";
            if (type == typeof(float)) return "float";
            if (type == typeof(double)) return "double";
            if (type == typeof(byte)) return "byte";

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
                             .Where(p => !IsImmediateValueTolerable(p.ParameterType) || HasParamKeyword(p))
                             .Select(p => p.ParameterType)
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
        private bool CanInstantiate(Type type, IList<Type> typeHistory)
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
                ? parameters.Select(p => p.ParameterType).Select(t => GetTypeNameForIdentifier(t, VarScope.Local)).ToArray()
                : parameters.Select(p => p.ParameterType).Select(t => GetInstance(t, nonDefault)).ToArray();

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
                else if (IsImmediateValueTolerable(pType))
                {
                    arguments[index] = GetInstance(pType, nonDefault);
                }
            }

            return arguments;
        }

        public bool ShouldUseVariableForParameter(ParameterInfo parameter)
        {
            return HasParamKeyword(parameter)
                   || !IsImmediateValueTolerable(parameter.ParameterType);
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
            if (gArgs.Length != 2) return (null,null);
            var dictType = typeof(Dictionary<,>).MakeGenericType(gArgs);
            return type.IsAssignableFrom(dictType) ? (gArgs[0], gArgs[1]) : (null,null);
        }

        private string GetTypeNameForIdentifier(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var unRefType = type.IsByRef ? type.GetElementType() : type;

            // ReSharper disable once AssignNullToNotNullAttribute
            var nullableType = Nullable.GetUnderlyingType(unRefType);
            if (nullableType != null) return $"{GetTypeNameForIdentifier(nullableType)}Nullable";

            var typeDisplayName = unRefType.Name;
            if (unRefType.IsGenericType)
            {
                // Handle types we can pretend are dictionaries
                var dictionaryType = GetDictionaryTypeForAssignment(type);
                if (dictionaryType != null && dictionaryType != type)
                {
                    return GetTypeNameForIdentifier(dictionaryType);
                }
                var genericParams = unRefType.GetGenericArguments();
                int index = typeDisplayName.IndexOf('`');
                typeDisplayName = index == -1 ? typeDisplayName : typeDisplayName.Substring(0, index);
                typeDisplayName = typeDisplayName + string.Join("", genericParams.Select(GetTypeNameForIdentifier));
            }

            if (unRefType.IsArray)
            {
                typeDisplayName = typeDisplayName.Replace("[]", "Array");
            }

            if (unRefType.IsInterface && typeDisplayName.StartsWith("I"))
            {
                return new string(typeDisplayName.Skip(1).ToArray());
            }

            return typeDisplayName;
        }

        private string GetLocalVariableName(Type type)
        {
            var varName = GetTypeNameForIdentifier(type);
            var unRefType = type.IsByRef ? type.GetElementType() : type;
            if (type.IsClass && type.Namespace != "System") return ToLowerInitial(varName);
            if (unRefType == typeof(DateTime) || unRefType == typeof(DateTime?)) varName += "Utc";
            var output = ToLowerInitial(varName);
            var prefix = type.IsInterface ? "stub" : "dummy";
            if (s_Keywords.Contains(output)) return $"{prefix}{varName}";
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

        private static readonly string[] s_Keywords = {
            "abstract", "add", "as", "ascending",
            "async", "await", "base", "bool",
            "break", "by", "byte", "case",
            "catch", "char", "checked", "class",
            "const", "continue", "decimal", "default",
            "delegate", "descending", "do", "double",
            "dynamic", "else", "enum", "equals",
            "explicit", "extern", "false", "finally",
            "fixed", "float", "for", "foreach",
            "from", "get", "global", "goto",
            "group", "if", "implicit", "in",
            "int", "interface", "internal", "into",
            "is", "join", "let", "lock",
            "long", "namespace", "new", "null",
            "object", "on", "operator", "orderby",
            "out", "override", "params", "partial",
            "private", "protected", "public", "readonly",
            "ref", "remove", "return", "sbyte",
            "sealed", "select", "set", "short",
            "sizeof", "stackalloc", "static", "string",
            "struct", "switch", "this", "throw",
            "true", "try", "typeof", "uint",
            "ulong", "unchecked", "unsafe", "ushort",
            "using", "value", "var", "virtual",
            "void", "volatile", "where", "while",
            "yield"
        };
    }

    enum VarScope { Local, Member }
}
