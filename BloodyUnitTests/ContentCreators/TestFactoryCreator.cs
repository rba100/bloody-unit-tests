using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BloodyUnitTests.Util;

namespace BloodyUnitTests.ContentCreators
{
    public class TestFactoryCreator : IContentCreator
    {
        private readonly CSharpService m_CSharpService = new CSharpService();

        public ClassContent Create(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            var lines = new List<string>();

            var ctor = type.GetConstructors().FirstOrDefault()
                       ?? type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                              .FirstOrDefault();
            if (ctor == null) return ClassContent.NoContent;

            var parameters = ctor.GetParameters();
            var interfaces = parameters.Where(p => p.ParameterType.IsInterface).ToArray();

            // Don't bother creating a factory unless there are dependencies
            if (interfaces.Length == 0) return ClassContent.NoContent;

            var typeName = m_CSharpService.GetNameForCSharp(type);

            var indent = new string(' ', 4);

            lines.Add($"private class {typeName}Factory");
            lines.Add("{");

            // Mocked interface dependencies
            foreach (var parameter in interfaces.OrderBy(IsSystemNamespace))
            {
                if (IsSystemNamespace(parameter))
                {
                    lines.Add(indent + GetPublicField(parameter));
                }
                else
                {
                    lines.Add(indent + GetPublicFieldInterfaceMock(parameter));
                }
            }

            if (interfaces.Any()) lines.Add(string.Empty);
            var args = parameters.Select(p =>
            {
                if (p.ParameterType.IsInterface) return StringUtils.ToUpperInitial(p.Name);
                return m_CSharpService.GetInstantiation(p.ParameterType);
            }).ToArray();

            // Create
            lines.Add($"{indent}public {typeName} Create()");
            lines.Add($"{indent}{{");
            var declarationStart = $"{indent}{indent}return new {typeName}(";
            var declarationStartOffset = new string(' ', declarationStart.Length);

            for (var i = 0; i < args.Length; i++)
            {
                var terminator = i == args.Length - 1 ? ");" : ",";
                lines.Add(i == 0
                              ? $"{declarationStart}{args[i]}{terminator}"
                              : $"{declarationStartOffset}{args[i]}{terminator}");
            }

            lines.Add($"{indent}}}");

            lines.Add(string.Empty);

            // Verify all
            lines.Add($"{indent}public void VerifyAllExpectations()");
            lines.Add($"{indent}{{");
            foreach (var i in interfaces.Except(interfaces.Where(IsSystemNamespace)))
            {
                if (!i.ParameterType.IsInterface) continue;
                lines.Add($"{indent}{indent}{StringUtils.ToUpperInitial(i.Name)}.VerifyAllExpectations();");
            }
            lines.Add($"{indent}}}");
            lines.Add("}");

            return new ClassContent(lines.ToArray(), m_CSharpService.GetNameSpaces()
                                                                   .Union(new[]
                                                                   {
                                                                       "Rhino.Mocks",
                                                                       "static Rhino.Mocks.MockRepository"
                                                                   })
                                                                   .ToArray());
        }

        private bool IsSystemNamespace(ParameterInfo info)
        {
            return info.ParameterType.Namespace?.StartsWith(nameof(System)) == true;
        }

        private string GetPublicFieldInterfaceMock(ParameterInfo parameter)
        {
            var t = parameter.ParameterType;
            var typeName = m_CSharpService.GetNameForCSharp(t);
            return $"public {typeName} {StringUtils.ToUpperInitial(parameter.Name)}" +
                   $" = {m_CSharpService.GetMockInstance(t)};";
        }

        private string GetPublicField(ParameterInfo parameter)
        {
            var t = parameter.ParameterType;
            var typeName = m_CSharpService.GetNameForCSharp(t);
            return $"public {typeName} {StringUtils.ToUpperInitial(parameter.Name)}" +
                   $" = {m_CSharpService.GetInstantiation(t)};";
        }
    }
}
