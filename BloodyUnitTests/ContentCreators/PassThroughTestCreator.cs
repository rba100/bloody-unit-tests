
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using BloodyUnitTests.Reflection;

namespace BloodyUnitTests.ContentCreators
{
    public class PassThroughTestCreator : IContentCreator
    {
        public ClassContent Create(Type type)
        {
            var cSharpWriter = new CSharpWriter();

            var delegationReports = ReflectionUtils.GetMethodsThatPassthrough(type);

            var safeCombinations = delegationReports
                                   .Where(r => r.ctor.GetParameters()
                                                .Select(p => p.ParameterType)
                                                .All(cSharpWriter.NoCircularDependenciesOrAbstract))
                                   .ToArray();

            if(!safeCombinations.Any()) return ClassContent.NoContent;

            var linesOfCode = GenerateTestMethods(type, safeCombinations, cSharpWriter);

            return new ClassContent(linesOfCode, cSharpWriter.GetNameSpaces());
        }

        private string[] GenerateTestMethods(Type classToTest,
                                             (ConstructorInfo ctor, MethodDelegationReport[] delegations)[] safeCombinations,
                                             CSharpWriter cSharpWriter)
        {
            var lines = new List<string>();
            int testCount = 0;
            foreach (var ctorDelegation in safeCombinations)
            {
                foreach (var report in ctorDelegation.delegations)
                {
                    if (testCount > 0) lines.Add(string.Empty);
                    lines.AddRange(GenerateTestMethod(classToTest, 
                                                      ctorDelegation.ctor,
                                                      report.InnerMethodInterfaceType,
                                                      report.Caller,
                                                      report.InnerMethod,
                                                      cSharpWriter));
                    testCount++;
                }
            }

            return lines.ToArray();
        }

        private string[] GenerateTestMethod(Type classToTest,
                                            ConstructorInfo constructor,
                                            Type interfaceType,
                                            MethodInfo method,
                                            MethodInfo innerMethod,
                                            CSharpWriter cSharpWriter)
        {
            var isVoid = method.ReturnType == typeof(void);
            if (!isVoid && !method.ReturnType.IsAssignableFrom(innerMethod.ReturnType)) return new string[0];

            var mockVarName = $"mock{cSharpWriter.GetIdentifier(interfaceType, VarScope.Member)}";
            var mockVarNameOffset = new string(' ', mockVarName.Length);
            var resultDeclaration = isVoid ? string.Empty : "var result = ";
            var sutVarName = cSharpWriter.GetIdentifier(classToTest, VarScope.Local);


            var commonMethodVariables = method.GetParameters()
                                        .Where(p => p.ParameterType != interfaceType)
                                        .PipeInto(p => cSharpWriter.GetVariableDeclarationsForParameters(p, setToNull: false, nonDefault: true));
            

            var rootType = constructor.DeclaringType;

            var ctorArgs = cSharpWriter.GetMethodArguments(constructor, useVariables: false, nonDefault: true);
            var methodArgs = cSharpWriter.GetMethodArguments(method, useVariables: true, nonDefault: true);
            var innerMethodArgs = cSharpWriter.GetMethodArguments(innerMethod, useVariables: true, nonDefault: true);
            var ifParam = constructor.GetParameters()
                                     .Select((p, i) => new { p, i })
                                     .Single(o => o.p.ParameterType == interfaceType);

            var ifParamName = ifParam.p.Name;
            var ifIndex = ifParam.i;

            ctorArgs[ifIndex] = mockVarName;

            var ctorArgumentsFlat = string.Join(", ", ctorArgs);
            var methodArgumentsFlat = string.Join(", ", methodArgs);
            var innerMethodArgumentsFlat = string.Join(", ", innerMethodArgs);

            var indent = new string(' ', 4);

            var lines = new List<string>();
            lines.Add("[Test]");
            lines.Add($"public void {method.Name}_delegates_to_{ifParamName}_{innerMethod.Name}()");
            lines.Add("{");
            lines.AddRange(commonMethodVariables.IndentBy(4));
            if (!isVoid)
            {
                lines.Add($"{indent}var expectedResult = {cSharpWriter.GetInstantiation(innerMethod.ReturnType, true)};");
            }
            lines.Add($"{indent}var {mockVarName} = {cSharpWriter.GetMockInstance(interfaceType)};");
            lines.Add($"{indent}{mockVarName}.Expect(m=>m.{innerMethod.Name}({innerMethodArgumentsFlat}))");
            lines.Add($"{indent}{mockVarNameOffset}.Repeat.Once(){(isVoid ? ";" : "")}");
            if (!isVoid)
            {
                lines.Add($"{indent}{mockVarNameOffset}.Return(expectedResult);");
            }
            lines.Add($"{indent}var {sutVarName} = new {cSharpWriter.GetNameForCSharp(rootType)}({ctorArgumentsFlat});");
            lines.Add($"{indent}{resultDeclaration}{sutVarName}.{method.Name}({methodArgumentsFlat});");
            lines.Add($"{indent}{mockVarName}.VerifyAllExpectations();");
            if (!isVoid)
            {
                lines.Add($"{indent}Assert.AreEqual(expectedResult, result);");
            }
            lines.Add("}");
            return lines.ToArray();
        }
    }
}