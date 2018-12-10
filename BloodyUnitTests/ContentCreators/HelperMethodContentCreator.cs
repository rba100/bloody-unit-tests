using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BloodyUnitTests.CodeGeneration;

namespace BloodyUnitTests.ContentCreators
{
    class HelperMethodContentCreator : IContentCreator
    {
        private readonly CSharpService m_CSharpService = new CSharpService();

        public ClassContent Create(Type type)
        {
            var lines = new List<string>();

            var classTypes = GetSupportedDependencies(type);
            var secondLevelDependencies = classTypes.SelectMany(GetSupportedDependencies)
                                                    .Concat(classTypes)
                                                    .Distinct().ToArray();

            for (var i = 0; i < secondLevelDependencies.Length; i++)
            {
                if (i > 0) lines.Add(string.Empty);
                var simpleClass = secondLevelDependencies[i];
                var declaration = GetBuilderMethod(simpleClass);
                lines.AddRange(declaration);
            }

            return new ClassContent(lines.ToArray(), m_CSharpService.GetNameSpaces());
        }

        private Type[] GetSupportedDependencies(Type type)
        {
            var methodTypes = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                  .OfType<MethodInfo>()
                                  .Where(m => m.DeclaringType != typeof(object))
                                  .SelectMany(m => m.GetParameters().Select(p => p.ParameterType)
                                                    .Concat(new[] { m.ReturnType }));

            var ctorTypes = type.GetConstructors().SelectMany(m => m.GetParameters().Select(p => p.ParameterType));

            return methodTypes.Concat(ctorTypes)
                              .Select(t => t.IsByRef ? t.GetElementType() : t)
                              .Where(t => t != null && t.IsClass)
                              .Distinct()
                              .Where(t => t.Namespace?.StartsWith(nameof(System)) != true && !t.IsArray)
                              .Where(m_CSharpService.NoCircularDependenciesOrAbstract)
                              .ToArray();
        }

        private string[] GetBuilderMethod(Type type)
        {
            var lines = new List<string>();
            var parameters = type.GetConstructors()
                                 .OrderByDescending(c => c.GetParameters().Length)
                                 .FirstOrDefault()?
                                 .GetParameters();

            if (parameters == null
                || parameters.Length == 0) return new string[0];

            var arguments = parameters.Select(p => m_CSharpService.GetInstantiation(p.ParameterType)).ToArray();

            // Thing(5) isn't obvious for what 5 does, so we add named parameters for bool and numbers.
            //
            int namedParameterStart = parameters.Length;
            for (var i = parameters.Length - 1; i >= 0; i--)
            {
                var pType = parameters[i].ParameterType;
                if (pType == typeof(bool) || NumericTypeHandler.IsHandled(type))
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
            }

            var indent = new string(' ', 4);
            lines.Add($"private static {m_CSharpService.GetNameForCSharp(type)} Create{m_CSharpService.GetIdentifier(type, VarScope.Member)}()");
            lines.Add($"{{");
            lines.Add($"{indent}return new {m_CSharpService.GetNameForCSharp(type)}({string.Join(", ", arguments)});");
            lines.Add($"}}");
            return lines.ToArray();
        }
    }
}
