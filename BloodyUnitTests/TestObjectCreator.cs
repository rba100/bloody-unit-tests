using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        public string[] GetObjectCreatorsForMethods(Type type)
        {
            var lines = new List<string>();

            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            var classes = methods.SelectMany(m => m.GetParameters())
                                 .Select(p => p.ParameterType)
                                 .Where(t => t.IsClass
                                          && t != typeof(string) 
                                          && t != typeof(object))
                                 .Where(IsRelativelySimple)
                                 .Distinct();

            foreach (var c in classes)
            {
                lines.AddRange(GetObjectCreator(c, 0));
                lines.Add("");
            }

            return lines.ToArray();
        }

        private string[] GetObjectCreator(Type type, int indentSize)
        {
            var parameters = type.GetConstructors()
                                 .OrderByDescending(c=>c.GetParameters().Length)
                                 .First()
                                 .GetParameters();

            var arguments = parameters.Select(p => m_TypeDescriber.GetDummyInstantiation(p.ParameterType)).ToArray();

            for (var i = 0; i < arguments.Length; i++)
            {
                var t = parameters[i].ParameterType;
                if (t == typeof(string))
                {
                    arguments[i] = $"\"{parameters[i].Name}\"";
                }else if (t.IsClass)
                    arguments[i] = $"Create{m_TypeDescriber.GetVariableName(t, Scope.Member)}()";
            }

            var indent = new string(' ', indentSize);
            var lines = new List<string>();
            lines.Add($"{indent}private {m_TypeDescriber.GetTypeNameForCSharp(type)} Create{m_TypeDescriber.GetVariableName(type, Scope.Member)}()");
            lines.Add($"{indent}{{");
            lines.Add($"{indent}{indent} return new {m_TypeDescriber.GetTypeNameForCSharp(type)}({string.Join(", ", arguments)})");
            lines.Add($"{indent}}}");
            return lines.ToArray();
        }

        private bool IsRelativelySimple(Type type)
        {
            if (type.IsValueType) return true;
            if (type == typeof(string)) return true;
            if (!type.IsClass) return false;

            var ctor = type.GetConstructors()
                           .OrderByDescending(c => c.GetParameters().Length)
                           .FirstOrDefault();

            if (ctor == null) return false;

            return ctor.GetParameters()
                       .All(p => !p.IsOut && IsRelativelySimple(p.ParameterType));
        }

        private string GetInterfaceMemberDeclaration(ParameterInfo parameter)
        {
            var t = parameter.ParameterType;
            return $"public {m_TypeDescriber.GetTypeNameForCSharp(t)} {m_TypeDescriber.GetVariableName(t, Scope.Member)}" +
                   $" = MockRepository.GenerateMock<{m_TypeDescriber.GetTypeNameForCSharp(t)}>();";
        }
    }
}
