using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BloodyUnitTests.ContentCreators
{
    class HelperMethodContentCreator : IContentCreator
    {
        private readonly TypeDescriber m_TypeDescriber = new TypeDescriber();

        public ClassContent Create(Type type)
        {
            return GetHelperObjectCreatorsForType(type);
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
                                    .Where(t => t.Namespace?.StartsWith(nameof(System)) != true && !t.IsArray)
                                    .Where(m_TypeDescriber.IsPoco)
                                    .ToArray();

            var namesSpaces = simpleClasses
                .SelectMany(c => c.GetConstructors())
                .SelectMany(c => c.GetParameters())
                .Select(p => p.ParameterType).Select(t => t.Namespace)
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
    }
}
