using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace BloodyUnitTests
{
    public class TestObjectCreator
    {
        private readonly TypeDescriber m_TypeDescriber = new TypeDescriber();

        public string[] TestFactoryDeclaration(Type type, ConstructorInfo constructor = null)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            var lines = new List<string>();

            var ctor = constructor ?? type.GetConstructors().First();
            var parameters = ctor.GetParameters();
            var interfaces = parameters.Where(p => p.ParameterType.IsInterface).ToArray();

            var typeName = m_TypeDescriber.GetTypeNameForCSharp(type);

            var indent = new string(' ', 4);

            lines.Add($"internal class {typeName}Factory");
            lines.Add("{");

            // Mocked interface dependencies
            foreach (var parameter in interfaces)
            {
                lines.Add(indent + GetInterfaceMemberDeclaration(parameter));
            }

            if (interfaces.Any()) lines.Add(String.Empty);

            // Create
            lines.Add($"{indent}public {typeName} Create()");
            lines.Add($"{indent}{{");
            lines.Add($"{indent}{indent} return new {typeName}({string.Join(", ", interfaces.Select(i => m_TypeDescriber.GetVariableName(i.ParameterType, Scope.Member)))})");
            lines.Add($"{indent}}}");

            lines.Add(String.Empty);

            // Verify all
            lines.Add($"{indent}public void VerifyAllExpectations()");
            lines.Add($"{indent}{{");
            foreach (var parameter in interfaces)
            {
                var t = parameter.ParameterType;
                if (!t.IsInterface) continue;
                lines.Add($"{indent}{indent}{m_TypeDescriber.GetVariableName(t, Scope.Member)}.VerifyAllExpectations();");
            }
            lines.Add($"{indent}}}");
            lines.Add("}");

            return lines.ToArray();
        }

        public string[] GetHelperObjectCreatorsForType(Type type)
        {
            var lines = new List<string>();

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                              .Where(m=>m.DeclaringType != typeof(object));

            var classes = methods.SelectMany(m => m.GetParameters())
                                 .Select(p => p.ParameterType)
                                 .Where(t => t.IsClass
                                          && t.Namespace != "System")
                                 .Where(IsValueTypeOrPOCO)
                                 .Distinct()
                                 .ToArray();

            foreach (var c in classes)
            {
                var delcaration = GetObjectCreator(c);
                if (!delcaration.Any()) continue;
                lines.AddRange(delcaration);
                lines.Add("");
            }

            return lines.ToArray();
        }

        private string[] GetObjectCreator(Type type)
        {
            var lines = new List<string>();
            var parameters = type.GetConstructors()
                                 .OrderByDescending(c=>c.GetParameters().Length)
                                 .First()
                                 .GetParameters();

            if (parameters.Length == 0) return lines.ToArray();

            var arguments = parameters.Select(p => m_TypeDescriber.GetDummyInstantiation(p.ParameterType)).ToArray();

            for (var i = 0; i < arguments.Length; i++)
            {
                var t = parameters[i].ParameterType;
                if (t == typeof(string))
                {
                    arguments[i] = $"\"{parameters[i].Name}\"";
                }
                else if (t.IsArray)
                {
                    arguments[i] = $"new {m_TypeDescriber.GetTypeNameForCSharp(GetElementType(t))}[0]";
                }
                else if (t == typeof(object))
                {
                    arguments[i] = "new object()";
                }
                else if (t.IsClass)
                {
                    arguments[i] = $"Create{m_TypeDescriber.GetVariableName(t, Scope.Member)}()";
                }
            }

            var indent = new string(' ', 4);
            lines.Add($"private {m_TypeDescriber.GetTypeNameForCSharp(type)} Create{m_TypeDescriber.GetVariableName(type, Scope.Member)}()");
            lines.Add($"{{");
            lines.Add($"{indent}return new {m_TypeDescriber.GetTypeNameForCSharp(type)}({string.Join(", ", arguments)})");
            lines.Add($"}}");
            return lines.ToArray();
        }

        private bool IsValueTypeOrPOCO(Type type)
        {
            return IsValueTypeOrPOCOInner(type, new List<Type>());
        }

        private bool IsValueTypeOrPOCOInner(Type type, IList<Type> typeHistory)
        {
            if (typeHistory.Contains(type)) return false;
            typeHistory.Add(type);

            if (type.IsValueType) return true;
            if (type == typeof(string)) return true;
            if (type.IsArray && IsValueTypeOrPOCOInner(type.GetElementType(), typeHistory)) return true;
            if (IsArrayAssignable(type)) return true;
            if (!type.IsClass) return false;

            var ctor = type.GetConstructors()
                           .OrderByDescending(c => c.GetParameters().Length)
                           .FirstOrDefault();

            if (ctor == null) return false;

            return ctor.GetParameters()
                       .All(p => !p.IsOut && IsValueTypeOrPOCOInner(p.ParameterType, typeHistory));
        }

        internal static bool IsArrayAssignable(Type type)
        {
            var gArgs = type.GetGenericArguments();
            if (!type.HasElementType && gArgs.Length != 1) return false;
            var elementType = gArgs.SingleOrDefault() ?? type.GetElementType();
            // ReSharper disable once PossibleNullReferenceException
            return type.IsAssignableFrom(elementType.MakeArrayType());
        }

        private Type GetElementType(Type collectionType)
        {
            var gArgs = collectionType.GetGenericArguments();
            return gArgs.SingleOrDefault() ?? collectionType.GetElementType();
        }

        private string GetInterfaceMemberDeclaration(ParameterInfo parameter)
        {
            var t = parameter.ParameterType;
            return $"public {m_TypeDescriber.GetTypeNameForCSharp(t)} {m_TypeDescriber.GetVariableName(t, Scope.Member)}" +
                   $" = MockRepository.GenerateMock<{m_TypeDescriber.GetTypeNameForCSharp(t)}>();";
        }
    }
}
