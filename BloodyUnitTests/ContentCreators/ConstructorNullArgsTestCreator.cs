using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BloodyUnitTests.ContentCreators
{
    public class ConstructorNullArgsTestCreator : IContentCreator
    {
        private readonly TypeDescriber m_TypeDescriber = new TypeDescriber();

        public ClassContent Create(Type type)
        {
            return new ClassContent(GetNullConstructorArgsTestInner(type), new []{ type.Namespace });
        }

        private string[] GetNullConstructorArgsTestInner(Type type)
        {
            var typeName = type.Name;
            List<string> lines = new List<string>();

            var testCases = GetConstructorNullTestCaseSource(type);
            if(!testCases.Any()) return lines.ToArray();

            var testCaseSource = $"{typeName}_constructor_null_argument_testcases";
            lines.Add($"public static IEnumerable<TestCaseData> {testCaseSource}()");
            lines.Add("{");

            lines.Add("    // ReSharper disable ObjectCreationAsStatement");
            foreach (var line in testCases)
            {
                lines.Add(new String(' ', 4) + line);
            }
            lines.Add("    // ReSharper restore ObjectCreationAsStatement");

            lines.Add("}");

            lines.Add(string.Empty);

            lines.Add($"[TestCaseSource(nameof({testCaseSource}))]");
            lines.Add($"public void {typeName}_constructor_null_argument_test(TestDelegate testDelegate)");
            lines.Add("{");

            lines.Add("    Assert.Throws<ArgumentNullException>(testDelegate);");
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
                                                   .Into(c => m_TypeDescriber.GetNeededVariableDeclarations(c, false));

            foreach (var declaration in variableDeclarations)
            {
                lines.Add(declaration);
            }

            if (variableDeclarations.Any()) lines.Add(string.Empty);

            
            foreach (var ctor in constructors)
            {
                var typeName = ctor.DeclaringType?.Name;
                var parameterTypes = ctor.GetParameters();
                var arguments = m_TypeDescriber.GetMethodArguments(ctor, true, false);

                for (int i = 0; i < parameterTypes.Length; i++)
                {
                    if (parameterTypes[i].ParameterType.IsValueType) continue;
                    var name = parameterTypes[i].Name;
                    var copyOfArguments = new List<string>(arguments) { [i] = "null" };

                    lines.Add($"yield return new TestCaseData(new TestDelegate(() => new " +
                              $"{typeName}({string.Join(", ", copyOfArguments)}))).SetName(\"null {name}\");");
                }
            }

            return lines.ToArray();
        }
    }
}