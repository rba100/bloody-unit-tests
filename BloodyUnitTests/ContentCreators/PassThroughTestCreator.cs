using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BloodyUnitTests.ContentCreators
{
    public class PassThroughTestCreator : IContentCreator
    {
        private readonly CSharpWriter m_CSharpWriter = new CSharpWriter();

        public ClassContent Create(Type type)
        {
            var (ctor, ifType) = GetPassthroughConstructor(type);
            if (ctor == null) return ClassContent.NoContent();
            return new ClassContent(GenerateTestMethods(type, ctor, ifType), new[] { ifType.Namespace, type.Namespace });
        }

        private string[] GenerateTestMethods(Type classToTest, ConstructorInfo constructor, Type interfaceType)
        {
            var lines = new List<string>();

            var methods = interfaceType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            for (var i = 0; i < methods.Length; i++)
            {
                if (i > 0) lines.Add(string.Empty);
                lines.AddRange(GenerateTestMethod(classToTest, constructor, interfaceType, methods[i]));
            }

            return lines.ToArray();
        }

        private string[] GenerateTestMethod(Type classToTest, 
                                            ConstructorInfo constructor,
                                            Type interfaceType,
                                            MethodInfo method)
        {
            var isVoid = method.ReturnType == typeof(void);

            var mockVarName = $"mock{m_CSharpWriter.GetIdentifier(interfaceType, VarScope.Member)}";
            var mockVarNameOffset = new string(' ', mockVarName.Length);
            var resultDeclaration = isVoid ? string.Empty : "var result = ";
            var sutVarName = m_CSharpWriter.GetIdentifier(classToTest.Name, VarScope.Local);


            var methodVariables = method.GetParameters()
                                        .Where(p => p.ParameterType != interfaceType)
                                        .PipeInto(p=> m_CSharpWriter.GetVariableDeclarationsForParameters(p, setToNull: false, nonDefault: true));

            var rootType = constructor.DeclaringType;

            var ctorArgs = m_CSharpWriter.GetMethodArguments(constructor, useVariables: false, nonDefault: true);
            var methodArgs = m_CSharpWriter.GetMethodArguments(method, useVariables: true, nonDefault: true);
            var ifParam = constructor.GetParameters()
                                     .Select((p, i) => new { p, i })
                                     .Single(o => o.p.ParameterType == interfaceType);

            var ifParamName = ifParam.p.Name;
            var ifIndex = ifParam.i;

            ctorArgs[ifIndex] = mockVarName;

            var ctorArgumentsFlat = string.Join(", ", ctorArgs);
            var methodArgumentsFlat = string.Join(", ", methodArgs);

            var indent = new string(' ', 4);

            var lines = new List<string>();
            lines.Add("[Test]");
            lines.Add($"public void {method.Name}_delegates_to_{ifParamName}()");
            lines.Add("{");
            lines.AddRange(methodVariables.IndentBy(4));
            if (!isVoid)
            {
                lines.Add($"{indent}var expectedResult = {m_CSharpWriter.GetInstantiation(method.ReturnType, true)};");
            }
            lines.Add($"{indent}var {mockVarName} = {m_CSharpWriter.GetMockInstance(interfaceType)};");
            lines.Add($"{indent}{mockVarName}.Expect(m=>m.{method.Name}({methodArgumentsFlat}))");
            lines.Add($"{indent}{mockVarNameOffset}.Repeat.Once(){(isVoid ? ";" : "")}");
            if (!isVoid)
            {
                lines.Add($"{indent}{mockVarNameOffset}.Return(expectedResult);");
            }
            lines.Add($"{indent}var {sutVarName} = new {m_CSharpWriter.GetNameForCSharp(rootType)}({ctorArgumentsFlat});");
            lines.Add($"{indent}{resultDeclaration}{sutVarName}.{method.Name}({methodArgumentsFlat});");
            lines.Add($"{indent}{mockVarName}.VerifyAllExpectations();");
            if (!isVoid)
            {
                lines.Add($"{indent}Assert.AreEqual(expectedResult, result);");
            }
            lines.Add("}");
            return lines.ToArray();
        }

        private (ConstructorInfo, Type) GetPassthroughConstructor(Type type)
        {
            var interfaces = type.GetInterfaces();
            if (!interfaces.Any()) return (null, null);

            bool isPotentialPassthoughCtor(ConstructorInfo constructor)
            {
                var parameters = constructor.GetParameters();
                if (!parameters.Any()) return false;
                var ifParameters = parameters.Where(p => interfaces.Contains(p.ParameterType)).ToArray();
                if (ifParameters.Length != 1) return false;
                return parameters.Except(ifParameters)
                                 .All(p => m_CSharpWriter.NoCircularDependenciesOrAbstract(p.ParameterType));
            }

            var ctor = type.GetConstructors()
                           .Where(isPotentialPassthoughCtor)
                           .OrderByDescending(c => c.GetParameters().Length)
                           .FirstOrDefault();

            if (ctor == null) return (null, null);

            var ifType = ctor.GetParameters()
                             .Single(p => interfaces.Contains(p.ParameterType))
                             .ParameterType;

            return (ctor, ifType);
        }
    }
}
