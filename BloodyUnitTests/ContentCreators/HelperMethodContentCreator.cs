using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BloodyUnitTests.ContentCreators
{
    class HelperMethodContentCreator : IContentCreator
    {
        private readonly CSharpWriter m_CSharpWriter = new CSharpWriter();

        public ClassContent Create(Type type)
        {
            return GetHelperObjectCreatorsForType(type);
        }

        private ClassContent GetHelperObjectCreatorsForType(Type type)
        {
            var lines = new List<string>();

            var simpleClasses = GetSupportedDependencies(type);

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
                var declaration = GetBuilderMethod(simpleClass);
                if (!declaration.Any()) continue;
                lines.AddRange(declaration);
            }

            return new ClassContent(lines.ToArray(), namesSpaces);
        }

        private Type[] GetSupportedDependencies(Type type)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                       .OfType<MethodBase>()
                       .Where(m => m.DeclaringType != typeof(object))
                       .Union(type.GetConstructors())
                       .SelectMany(m => m.GetParameters())
                       .Select(p => p.ParameterType)
                       .Where(p => p.IsClass)
                       .Distinct()
                       .Where(t => t.Namespace?.StartsWith(nameof(System)) != true && !t.IsArray)
                       .Where(m_CSharpWriter.CanInstantiate)
                       .ToArray();
        }

        private string[] GetBuilderMethod(Type type)
        {
            var lines = new List<string>();
            var parameters = type.GetConstructors()
                                 .OrderByDescending(c => c.GetParameters().Length)
                                 .First()
                                 .GetParameters();

            if (parameters.Length == 0) return new string[0];

            var arguments = parameters.Select(p => m_CSharpWriter.GetInstance(p.ParameterType)).ToArray();

            int namedParameterStart = parameters.Length;
            for (var i = parameters.Length-1; i >=0; i--)
            {
                var pType = parameters[i].ParameterType;
                if (pType == typeof(bool) || pType == typeof(int))
                {
                    namedParameterStart = i;
                }
                else break;
            }

            for (var i = 0; i < arguments.Length; i++)
            {
                var t = parameters[i].ParameterType;
                if (t == typeof(string))
                {
                    arguments[i] = $"\"{parameters[i].Name}\"";
                }

                if (i >= namedParameterStart)
                {
                    arguments[i] = $"{parameters[i].Name}: {arguments[i]}";
                }
                //else if (t.IsArray)
                //{
                //    arguments[i] = $"new {m_CSharpWriter.GetTypeNameForCSharp(t.GetElementType())}[0]";
                //}
                //else if (t == typeof(object))
                //{
                //    arguments[i] = "new object()";
                //}
            }

            var indent = new string(' ', 4);
            lines.Add($"private static {m_CSharpWriter.GetTypeNameForCSharp(type)} Create{m_CSharpWriter.GetTypeNameForIdentifier(type, VarScope.Member)}()");
            lines.Add($"{{");
            lines.Add($"{indent}return new {m_CSharpWriter.GetTypeNameForCSharp(type)}({string.Join(", ", arguments)});");
            lines.Add($"}}");
            return lines.ToArray();
        }
    }
}
