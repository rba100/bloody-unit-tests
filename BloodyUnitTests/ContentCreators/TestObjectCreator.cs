using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BloodyUnitTests.ContentCreators
{
    public class TestObjectCreator
    {
        private readonly TypeDescriber m_TypeDescriber = new TypeDescriber();

        public ClassContent TestFactoryDeclaration(Type type)
        {
            return new ClassContent(TestFactoryDeclarationInner(type), new string[0]);
        }

        public ClassContent HelperMethodContent(Type type)
        {
            return GetHelperObjectCreatorsForType(type);
        }

        private string[] TestFactoryDeclarationInner(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            var lines = new List<string>();

            var ctor = type.GetConstructors().First();
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
            var args = parameters.Select(p =>
            {
                if (p.ParameterType.IsInterface) return m_TypeDescriber.GetVariableName(p, Scope.Member);
                return m_TypeDescriber.GetInstance(p.ParameterType);
            });

            // Create
            lines.Add($"{indent}public {typeName} Create()");
            lines.Add($"{indent}{{");
            lines.Add($"{indent}{indent} return new {typeName}({string.Join(", ", args)})");
            lines.Add($"{indent}}}");

            lines.Add(String.Empty);

            // Verify all
            lines.Add($"{indent}public void VerifyAllExpectations()");
            lines.Add($"{indent}{{");
            foreach (var i in interfaces.Where(i => !i.ParameterType.Namespace.StartsWith(nameof(System))))
            {
                if (!i.ParameterType.IsInterface) continue;
                lines.Add($"{indent}{indent}{m_TypeDescriber.GetVariableName(i, Scope.Member)}.VerifyAllExpectations();");
            }
            lines.Add($"{indent}}}");
            lines.Add("}");

            return lines.ToArray();
        }

        private ClassContent GetHelperObjectCreatorsForType(Type type)
        {
            var lines = new List<string>();

            var simpleClasses = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                    .OfType<MethodBase>()
                                    .Where(m => m.DeclaringType != typeof(object))
                                    .Union(type.GetConstructors())
                                    .SelectMany(m => m.GetParameters())
                                    .Select(p => p.ParameterType)
                                    .Distinct()
                                    .Where(t => t.Namespace?.StartsWith(nameof(System)) != true)
                                    .Where(m_TypeDescriber.IsPoco)
                                    .ToArray();

            var namesSpaces = simpleClasses
                .SelectMany(c => c.GetConstructors())
                .SelectMany(c => c.GetParameters())
                .Select(p => p.ParameterType).Select(t=>t.Namespace)
                .Append(type.Namespace)
                .Distinct()
                .ToArray();

            for (var i = 0; i < simpleClasses.Length; i++)
            {
                if (i > 0) lines.Add(string.Empty);
                var simpleClass = simpleClasses[i];
                var declaration = GetObjectCreator(simpleClass);
                if (!declaration.Any()) continue;
                lines.AddRange(declaration);
            }

            return new ClassContent(lines.ToArray(), namesSpaces);
        }

        private string[] GetObjectCreator(Type type)
        {
            var lines = new List<string>();
            var parameters = type.GetConstructors()
                                 .OrderByDescending(c => c.GetParameters().Length)
                                 .First()
                                 .GetParameters();

            if (parameters.Length == 0) return lines.ToArray();

            var arguments = parameters.Select(p => m_TypeDescriber.GetInstance(p.ParameterType)).ToArray();

            for (var i = 0; i < arguments.Length; i++)
            {
                var t = parameters[i].ParameterType;
                if (t == typeof(string))
                {
                    arguments[i] = $"\"{parameters[i].Name}\"";
                }
                else if (t.IsArray)
                {
                    arguments[i] = $"new {m_TypeDescriber.GetTypeNameForCSharp(t.GetElementType())}[0]";
                }
                else if (t == typeof(object))
                {
                    arguments[i] = "new object()";
                }
            }

            var indent = new string(' ', 4);
            lines.Add($"private {m_TypeDescriber.GetTypeNameForCSharp(type)} Create{m_TypeDescriber.GetVariableName(type, Scope.Member)}()");
            lines.Add($"{{");
            lines.Add($"{indent}return new {m_TypeDescriber.GetTypeNameForCSharp(type)}({string.Join(", ", arguments)});");
            lines.Add($"}}");
            return lines.ToArray();
        }

        private string GetInterfaceMemberDeclaration(ParameterInfo parameter)
        {
            var t = parameter.ParameterType;
            var typeName = m_TypeDescriber.GetTypeNameForCSharp(t);
            return $"public {typeName} {m_TypeDescriber.GetVariableName(parameter, Scope.Member)}" +
                   $" = {m_TypeDescriber.GetInstance(t)};";
        }
    }
}
