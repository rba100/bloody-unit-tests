﻿using System;
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

            var methodTestCases = GetTestCaseSource(type, type.GetMethods, false);
            var ctorTestCases = GetTestCaseSource(type, type.GetConstructors, true);

            if(!methodTestCases.Union(ctorTestCases).Any()) return ClassContent.NoContent();

            if (methodTestCases.Any())
            {
                var testCaseSource = $"{type.Name}_method_invalid_argument_testcases";
                lines.Add($"public static IEnumerable<TestCaseData> {testCaseSource}()");
                lines.Add("{");
                foreach (var line in methodTestCases)
                {
                    lines.Add(new string(' ', 4) + line);
                }

                lines.Add("}");

                lines.Add(string.Empty);

                lines.Add($"[TestCaseSource(nameof({testCaseSource}))]");
                lines.Add($"public void {type.Name}_method_invalid_arguments(TestDelegate testDelegate)");
                lines.Add("{");

                lines.Add("    Assert.Throws<ArgumentException>(testDelegate);");
                lines.Add("}");
            }

            if(methodTestCases.Any() && ctorTestCases.Any()) lines.Add(string.Empty);

            if (ctorTestCases.Any())
            {
                var testCaseSource = $"{type.Name}_constructor_invalid_argument_testcases";
                lines.Add($"public static IEnumerable<TestCaseData> {testCaseSource}()");
                lines.Add("{");
                foreach (var line in ctorTestCases)
                {
                    lines.Add(new string(' ', 4) + line);
                }

                lines.Add("}");

                lines.Add(string.Empty);

                lines.Add($"[TestCaseSource(nameof({testCaseSource}))]");
                lines.Add($"public void {type.Name}_constructor_invalid_arguments(TestDelegate testDelegate)");
                lines.Add("{");

                lines.Add("    Assert.Throws<ArgumentException>(testDelegate);");
                lines.Add("}");
            }

            return new ClassContent(lines.ToArray(), m_CSharpWriter.GetNameSpaces());
        }

        private string[] GetTestCaseSource(Type type, Func<BindingFlags,MethodBase[]> methodGetter, bool isCtor)
        {
            var hasArrayAssignables = methodGetter(BindingFlags.Public | BindingFlags.Instance)
                                       .Where(t => isCtor || type.IsMethodTestable(t))
                                       .Where(m => m.GetParameters()
                                                    .Select(p => p.ParameterType)
                                                    .Any(t => m_CSharpWriter.IsArrayAssignable(t) 
                                                          && !m_CSharpWriter.GetArrayElementType(t).IsValueType))
                                       .ToArray();

            var hasDateTimeParameter = methodGetter(BindingFlags.Public | BindingFlags.Instance)
                                           .Where(t => isCtor || type.IsMethodTestable(t))
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

            if (!isCtor)
            {
                lines.AddRange(m_CSharpWriter.GetStubbedInstantiation(type));
                lines.Add(string.Empty);
            }

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

                    var invocation = isCtor ? $"new {m_CSharpWriter.GetNameForCSharp(type)}" : $"{instanceName}.{methodName}";

                    lines.Add($"yield return new TestCaseData(new TestDelegate(() => " +
                              $"{invocation}({string.Join(", ", copyOfArguments)})))" +
                              $".SetName(\"{methodName} with {parameters[i].Name} {testNameSuffix}\");");
                }
            }

            return testCasesExist ? lines.ToArray() : new string[0];
        }
    }
}
