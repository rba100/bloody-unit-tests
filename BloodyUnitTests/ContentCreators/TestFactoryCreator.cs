using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BloodyUnitTests.ContentCreators
{
    public class TestFactoryCreator : IContentCreator
    {
        private readonly CSharpWriter m_CSharpWriter = new CSharpWriter();

        public ClassContent Create(Type type)
        {
            return new ClassContent(TestFactoryDeclarationInner(type), new string[0]);
        }

        private string[] TestFactoryDeclarationInner(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            var lines = new List<string>();

            var ctor = type.GetConstructors().FirstOrDefault()
                       ?? type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                              .FirstOrDefault();
            if (ctor == null) return lines.ToArray();

            var parameters = ctor.GetParameters();
            var interfaces = parameters.Where(p => p.ParameterType.IsInterface).ToArray();

            // Don't bother creating a factory unless there are many dependencies
            if (interfaces.Length < 3) return lines.ToArray();

            var typeName = m_CSharpWriter.GetNameForCSharp(type);

            var indent = new string(' ', 4);

            lines.Add($"private class {typeName}Factory");
            lines.Add("{");

            // Mocked interface dependencies
            foreach (var parameter in interfaces)
            {
                lines.Add(indent + GetPublicFieldInterfaceMock(parameter));
            }

            if (interfaces.Any()) lines.Add(String.Empty);
            var args = parameters.Select(p =>
            {
                if (p.ParameterType.IsInterface) return m_CSharpWriter.GetIdentifier(p.Name, VarScope.Member);
                return m_CSharpWriter.GetInstantiation(p.ParameterType);
            });

            // Create
            lines.Add($"{indent}public {typeName} Create()");
            lines.Add($"{indent}{{");
            lines.Add($"{indent}{indent} return new {typeName}({string.Join(", ", args)});");
            lines.Add($"{indent}}}");

            lines.Add(String.Empty);

            // Verify all
            lines.Add($"{indent}public void VerifyAllExpectations()");
            lines.Add($"{indent}{{");
            foreach (var i in interfaces.Where(i => i.ParameterType.Namespace?.StartsWith(nameof(System)) != true))
            {
                if (!i.ParameterType.IsInterface) continue;
                lines.Add($"{indent}{indent}{m_CSharpWriter.GetIdentifier(i.Name, VarScope.Member)}.VerifyAllExpectations();");
            }
            lines.Add($"{indent}}}");
            lines.Add("}");

            return lines.ToArray();
        }

        private string GetPublicFieldInterfaceMock(ParameterInfo parameter)
        {
            var t = parameter.ParameterType;
            var typeName = m_CSharpWriter.GetNameForCSharp(t);
            return $"public {typeName} {m_CSharpWriter.GetIdentifier(parameter.Name, VarScope.Member)}" +
                   $" = {m_CSharpWriter.GetMockInstance(t)};";
        }
    }
}
