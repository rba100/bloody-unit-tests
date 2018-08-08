using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BloodyUnitTests.ContentCreators
{
    public class ConstructorNullArgsTestCreator : IContentCreator
    {
        private readonly CSharpWriter m_CSharpWriter = new CSharpWriter();

        public ClassContent Create(Type type)
        {
            return new ClassContent(GetNullConstructorArgsTestInner(type), new[] { type.Namespace });
        }

        private string[] GetNullConstructorArgsTestInner(Type type)
        {
            var typeName = type.Name;
            var lines = new List<string>();

            var testCases = GetConstructorNullTestCaseSource(type);
            if (!testCases.Any()) return new string[0];
            var indent = new string(' ', 4);
            var testCaseSource = $"{typeName}_constructor_null_argument_testcases";
            lines.Add($"public static IEnumerable<TestCaseData> {testCaseSource}()");
            lines.Add("{");
            lines.AddRange(testCases.Select(line => $"{indent}{line}"));
            lines.Add("}");

            lines.Add(string.Empty);

            lines.Add($"[TestCaseSource(nameof({testCaseSource}))]");
            lines.Add($"public void {typeName}_constructor_null_argument_test(TestDelegate testDelegate)");
            lines.Add("{");
            lines.Add($"{indent}Assert.Throws<ArgumentNullException>(testDelegate);");
            lines.Add("}");
            return lines.ToArray();
        }

        private string[] GetConstructorNullTestCaseSource(Type type)
        {
            var ctors = type.GetConstructors();
            if (!ctors.Any()) ctors = type.GetConstructors(BindingFlags.NonPublic | BindingFlags.CreateInstance);

            return ToConstructorNullArgsTestCaseSourceImp(ctors);
        }

        private string[] ToConstructorNullArgsTestCaseSourceImp(ConstructorInfo[] constructors)
        {
            var lines = new List<string>();

            var variableDeclarations = constructors.SelectMany(i => i.GetParameters())
                                                   .PipeInto(c => m_CSharpWriter.GetVariableDeclarationsForParameters(c, false, false));

            foreach (var declaration in variableDeclarations)
            {
                lines.Add(declaration);
            }

            if (variableDeclarations.Any()) lines.Add(string.Empty);

            bool testCasesExist = false;
            int preTestLineCount = lines.Count;
            foreach (var ctor in constructors)
            {
                var typeName = ctor.DeclaringType?.Name;
                var parameterTypes = ctor.GetParameters();
                var arguments = m_CSharpWriter.GetMethodArguments(ctor, true, false);

                for (int i = 0; i < parameterTypes.Length; i++)
                {
                    if (parameterTypes[i].ParameterType.IsValueType) continue;
                    testCasesExist = true;
                    var name = parameterTypes[i].Name;
                    var argumentsWithNulledParameter = new List<string>(arguments) { [i] = "null" };

                    lines.Add($"yield return new TestCaseData(new TestDelegate(() => new " +
                              $"{typeName}({string.Join(", ", argumentsWithNulledParameter)}))).SetName(\"null {name}\");");
                }
            }

            if (testCasesExist)
            {
                lines.Insert(preTestLineCount, "// ReSharper disable ObjectCreationAsStatement");
                lines.Add("// ReSharper restore ObjectCreationAsStatement");
            }

            return lines.ToArray();
        }
    }
}