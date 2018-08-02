using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BloodyUnitTests.ContentCreators
{
    public class PassThroughTestCreator
    {
        private readonly TypeDescriber m_TypeDescriber = new TypeDescriber();

        public ClassContent CreatePassthroughTests(Type type)
        {
            return new ClassContent(CreatePassthroughTestsInner(type), new string[0]);
        }

        private string[] CreatePassthroughTestsInner(Type type)
        {
            var (ctor, ifType) = GetPassthroughConstructor(type);
            if (ctor == null) return new string[0];
            return GenerateTestMethods(type, ctor, ifType);
        }

        private string[] GenerateTestMethods(Type classToTest, ConstructorInfo constructor, Type interfaceType)
        {
            var lines = new List<string>();

            var methods = interfaceType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            for (var i = 0; i < methods.Length; i++)
            {
                if(i>0) lines.Add(string.Empty);
                lines.AddRange(GenerateTestMethod(classToTest, constructor, interfaceType, methods[i]));
            }

            lines.Add("[Test]");

            return lines.ToArray();
        }

        private string[] GenerateTestMethod(Type classToTest, ConstructorInfo constructor, Type interfaceType, MethodInfo method)
        {
            var isVoid = method.ReturnType == typeof(void);

            var mockVarName = $"mock{m_TypeDescriber.GetVariableName(interfaceType, Scope.Member)}";
            var mockVarNameOffset = new string(' ', mockVarName.Length);
            var resultDeclaration = isVoid ? string.Empty : "var result = ";
            var sutVarName = m_TypeDescriber.GetVariableName(classToTest.Name, Scope.Local);
            var rootType = constructor.DeclaringType;

            var ctorArgs = m_TypeDescriber.GetMethodArguments(constructor, useVariables: false, nonDefault: true);
            var methodArgs = m_TypeDescriber.GetMethodArguments(method, useVariables: true, nonDefault: true);
            var ifIndex = constructor.GetParameters()
                                     .Select((p, i) => new { p, i })
                                     .Single(o => o.p.ParameterType == interfaceType).i;

            ctorArgs[ifIndex] = mockVarName;

            var ctorArgumentsFlat = string.Join(", ", ctorArgs);
            var methodArgumentsFlat = string.Join(", ", methodArgs);

            var lines = new List<string>();
            lines.Add("[Test]");
            lines.Add($"public void {method.Name}_passthrough_test()");
            lines.Add("{");
            if (!isVoid)
            {
                lines.Add($"    var expectedResult = {m_TypeDescriber.GetInstance(method.ReturnType, true)};");
            }
            lines.Add($"    {m_TypeDescriber.GetMockVariableDeclaration(interfaceType)}");
            lines.Add($"    {mockVarName}.Expect(m=>m.{method.Name}({methodArgumentsFlat}))");
            lines.Add($"    {mockVarNameOffset}.Repeat.Once(){(isVoid ? ";" : "")}");
            if (!isVoid)
            {
                lines.Add($"    {mockVarNameOffset}.Return(expectedResult);");
            }
            lines.Add($"    var {sutVarName} = new {m_TypeDescriber.GetTypeNameForCSharp(rootType)}({ctorArgumentsFlat});");
            lines.Add($"    {resultDeclaration}{sutVarName}.{method.Name}({methodArgumentsFlat})");
            lines.Add($"    {mockVarName}.VerifyAllExpectations();");
            if (!isVoid)
            {
                lines.Add($"    Assert.AreEqual(expectedResult, result);");
            }
            lines.Add("}");
            return lines.ToArray();
        }

        private (ConstructorInfo,Type) GetPassthroughConstructor(Type type)
        {
            var interfaces = type.GetInterfaces();
            if (!interfaces.Any()) return (null,null);

            //var passthroughConstructor = interfaces.Select(i => type.GetConstructor(new[] {i})).SingleOrDefault();

            bool isPotentialPassthoughCtor(ConstructorInfo constructor)
            {
                var parameters = constructor.GetParameters();
                if (!parameters.Any()) return false;
                var ifParameters = parameters.Where(p => interfaces.Contains(p.ParameterType)).ToArray();
                if (ifParameters.Length != 1) return false;
                return parameters.Except(ifParameters).All(p => m_TypeDescriber.IsPoco(p.ParameterType));
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
