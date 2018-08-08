using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BloodyUnitTests.ContentCreators
{
    class InvalidArgumentTestCreator : IContentCreator
    {
        private readonly CSharpWriter m_CSharpWriter = new CSharpWriter();

        public ClassContent Create(Type type)
        {
            var lines = new List<string>();

            var testCases = GetTestCaseSource(type);
            if (!testCases.Any()) return ClassContent.NoContent();

            var testCaseSource = $"{type.Name}_invalid_argument_testcases";
            lines.Add($"public static IEnumerable<TestCaseData> {testCaseSource}()");
            lines.Add("{");
            foreach (var line in testCases)
            {
                lines.Add(new String(' ', 4) + line);
            }

            lines.Add("}");

            lines.Add(string.Empty);

            lines.Add($"[TestCaseSource(nameof({testCaseSource}))]");
            lines.Add($"public void {type.Name}_invalid_argument_test(TestDelegate testDelegate)");
            lines.Add("{");

            lines.Add("    Assert.Throws<ArgumentException>(testDelegate);");
            lines.Add("}");
            return new ClassContent(lines.ToArray(), new string[0]);
        }

        private string[] GetTestCaseSource(Type type)
        {
            var hasArrayAssignables = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                       .Where(type.IsMethodTestable)
                                       .Where(m => m.GetParameters()
                                                    .Select(p => p.ParameterType)
                                                    .Any(t => m_CSharpWriter.IsArrayAssignable(t) 
                                                          && !m_CSharpWriter.GetArrayElementType(t).IsValueType))
                                       .ToArray();

            var hasDateTimeParameter = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                           .Where(type.IsMethodTestable)
                                           .Where(m => m.GetParameters()
                                                        .Select(p => p.ParameterType)
                                                        .Any(t=>t == typeof(DateTime) || t == typeof(DateTime?)))
                                           .ToArray();

            var testableMethods = hasArrayAssignables.Union(hasDateTimeParameter).Distinct().ToArray();

            var lines = new List<string>();

            if (!testableMethods.Any()) return new string[0];

            var parametersNeedingVariables = testableMethods.Select(i => i.GetParameters())
                                                            .SelectMany(p => p)
                                                            .Where(m_CSharpWriter.ShouldUseVariableForParameter)
                                                            .GroupBy(p=>p.ParameterType).Select(g=>g.First())
                                                            .ToArray();

            var inParameters = parametersNeedingVariables.Where(p => !p.IsOut).ToArray();
            var outParamters = parametersNeedingVariables.Where(p => p.IsOut).Except(inParameters).ToArray();

            var variableDeclarations = m_CSharpWriter.GetVariableDeclarationsForParameters(inParameters, setToNull: false, nonDefault: false);
            var outVariableDeclarations = m_CSharpWriter.GetVariableDeclarationsForParameters(outParamters, setToNull: true, nonDefault: false);

            lines.AddRange(variableDeclarations);
            lines.AddRange(outVariableDeclarations);

            if (variableDeclarations.Union(outVariableDeclarations).Any()) lines.Add(string.Empty);

            var instanceName = m_CSharpWriter.GetIdentifier(type, VarScope.Local);

            lines.AddRange(m_CSharpWriter.GetStubbedInstantiation(type));
            lines.Add(string.Empty);

            bool testCasesExist = false;
            foreach (var info in testableMethods)
            {
                var methodName = info.Name;
                var parameters = info.GetParameters();
                var arguments = m_CSharpWriter.GetMethodArguments(info, true, false);
                for (var i = 0; i < parameters.Length; i++)
                {
                    if (m_CSharpWriter.HasParamKeyword(parameters[i])) continue;
                    var pType = parameters[i].ParameterType;
                    var copyOfArguments = new List<string>(arguments);
                    string testNameSuffix;
                    if (m_CSharpWriter.IsArrayAssignable(pType))
                    {
                        var arrayElementType = m_CSharpWriter.GetArrayElementType(pType);
                        if(arrayElementType.IsValueType) continue;
                        var instance = m_CSharpWriter.GetNameForCSharp(arrayElementType);
                        copyOfArguments[i] = $"new {instance}[] {{ null }}";
                        testNameSuffix = "contains null";
                    }
                    else if (pType == typeof(DateTime) || pType == typeof(DateTime?))
                    {
                        copyOfArguments[i] = $"DateTime.MinValue";
                        testNameSuffix = "not UTC";
                    }
                    else { continue;}
                    testCasesExist = true;

                    lines.Add($"yield return new TestCaseData(new TestDelegate(() => " +
                              $"{instanceName}.{methodName}({string.Join(", ", copyOfArguments)})))" +
                              $".SetName(\"{methodName} with {parameters[i].Name} {testNameSuffix}\");");
                }
            }

            return testCasesExist ? lines.ToArray() : new string[0];
        }
    }
}
