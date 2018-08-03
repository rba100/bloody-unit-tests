using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BloodyUnitTests.ContentCreators
{
    class MethodNullArgsTestCreator : IContentCreator
    {
        private readonly TypeDescriber m_TypeDescriber = new TypeDescriber();

        public ClassContent Create(Type type)
        {
            var lines = new List<string>();

            var testCases = GetMethodNullTestCaseSource(type);
            if (!testCases.Any()) return ClassContent.NoContent();

            var testCaseSource = $"{type.Name}_method_null_argument_testcases";
            lines.Add($"public static IEnumerable<TestCaseData> {testCaseSource}()");
            lines.Add("{");
            foreach (var line in testCases)
            {
                lines.Add(new String(' ', 4) + line);
            }

            lines.Add("}");

            lines.Add(string.Empty);

            lines.Add($"[TestCaseSource(nameof({testCaseSource}))]");
            lines.Add($"public void {type.Name}_method_null_argument_test(TestDelegate testDelegate)");
            lines.Add("{");

            lines.Add("    Assert.Throws<ArgumentNullException>(testDelegate);");
            lines.Add("}");
            return new ClassContent(lines.ToArray(), new string[0]);
        }

        private string[] GetMethodNullTestCaseSource(Type type)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                              .Where(type.IsMethodTestable)
                              .ToArray();

            return ToMethodNullArgsTestCaseSourceImp(methods, type);
        }

        private string[] ToMethodNullArgsTestCaseSourceImp(MethodInfo[] infos, Type type)
        {
            var lines = new List<string>();

            if (!infos.Any()) return lines.ToArray();

            var parameterTypes = infos.Select(i => i.GetParameters())
                                      .Where(p => p.Count(t => !t.ParameterType.IsValueType) > 1)
                                      .SelectMany(p => p)
                                      .ToArray();

            var variablesNeeded = parameterTypes.Where(p => !p.IsOut).ToArray();
            var outVariablesNeeded = parameterTypes.Where(p => p.IsOut).Except(variablesNeeded).ToArray();

            var variableDeclarations = m_TypeDescriber.GetNeededVariableDeclarations(variablesNeeded, setToNull: false);
            var outVariableDeclarations = m_TypeDescriber.GetNeededVariableDeclarations(outVariablesNeeded, setToNull: true);
            lines.AddRange(variableDeclarations);
            lines.AddRange(outVariableDeclarations);
            if (variableDeclarations.Union(outVariableDeclarations).Any()) lines.Add(string.Empty);

            var instanceName = m_TypeDescriber.GetVariableName(type, Scope.Local);

            lines.AddRange(m_TypeDescriber.GetStubbedInstantiation(type));
            lines.Add(string.Empty);

            foreach (var info in infos)
            {
                var methodName = info.Name;
                var parameters = info.GetParameters();
                var arguments = m_TypeDescriber.GetMethodArguments(info, true, false);

                for (var i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].ParameterType.IsValueType) continue;
                    if (m_TypeDescriber.HasParamKeyword(parameters[i])) continue;

                    var copyOfArguments = new List<string>(arguments) { [i] = "null" };

                    lines.Add($"yield return new TestCaseData(new TestDelegate(() => " +
                              $"{instanceName}.{methodName}({string.Join(", ", copyOfArguments)})))" +
                              $".SetName(\"{methodName} with null {parameters[i].Name}\");");
                }
            }

            return lines.ToArray();
        }
    }
}
