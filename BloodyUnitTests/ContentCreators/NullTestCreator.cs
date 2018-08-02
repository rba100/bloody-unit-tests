using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BloodyUnitTests.ContentCreators
{
    public class NullTestCreator
    {
        private readonly TypeDescriber m_TypeDescriber = new TypeDescriber();

        public ClassContent GetNullConstructorArgTestContent(Type type)
        {
            return new ClassContent(GetNullConstructorArgsTestInner(type), new string[0]);
        }

        public ClassContent GetNullMethodArgTestContent(Type type)
        {
            return new ClassContent(GetNullMethodArgsTestInner(type), new string[0]);
        }

        private string[] GetNullConstructorArgsTestInner(Type type)
        {
            var typeName = type.Name;
            List<string> lines = new List<string>();
            var testCaseSource = $"{typeName}_constructor_null_argument_testcases";
            lines.Add($"public static IEnumerable<TestCaseData> {testCaseSource}()");
            lines.Add("{");
            foreach (var line in GetConstructorNullTestCaseSource(type))
            {
                lines.Add(new String(' ', 4) + line);
            }

            lines.Add("}");

            lines.Add(string.Empty);

            lines.Add($"[TestCaseSource(nameof({testCaseSource}))]");
            lines.Add($"public void {typeName}_constructor_null_argument_test(TestDelegate testDelegate)");
            lines.Add("{");

            lines.Add("    Assert.Throws<ArgumentNullException>(testDelegate);");
            lines.Add("}");
            return lines.ToArray();
        }

        private string[] GetNullMethodArgsTestInner(Type type)
        {
            var typeName = type.Name;
            List<string> lines = new List<string>();
            var testCaseSource = $"{typeName}_method_null_argument_testcases";
            lines.Add($"public static IEnumerable<TestCaseData> {testCaseSource}()");
            lines.Add("{");
            foreach (var line in GetMethodNullTestCaseSource(type))
            {
                lines.Add(new String(' ', 4) + line);
            }

            lines.Add("}");

            lines.Add(string.Empty);

            lines.Add($"[TestCaseSource(nameof({testCaseSource}))]");
            lines.Add($"public void {typeName}_method_null_argument_test(TestDelegate testDelegate)");
            lines.Add("{");

            lines.Add("    Assert.Throws<ArgumentNullException>(testDelegate);");
            lines.Add("}");
            return lines.ToArray();
        }

        private string[] GetMethodNullTestCaseSource(Type type)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                              .Where(type.IsMethodTestable)
                              .ToArray();

            if (!methods.Any())
                methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
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

            var variableDeclarations = GetVariableDeclarations(variablesNeeded, setToNull: false);
            var outVariableDeclarations = GetVariableDeclarations(outVariablesNeeded, setToNull: true);
            lines.AddRange(variableDeclarations);
            lines.AddRange(outVariableDeclarations);
            if (variableDeclarations.Union(outVariableDeclarations).Any()) lines.Add(string.Empty);

            var instanceName = m_TypeDescriber.GetVariableName(type, Scope.Local);

            lines.AddRange(GetStubbedInstantiation(type));
            lines.Add(string.Empty);

            foreach (var info in infos)
            {
                var methodName = info.Name;
                var parameters = info.GetParameters();
                var arguments = GetMethodArguments(info, true);

                for (var i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].ParameterType.IsValueType) continue;
                    if (m_TypeDescriber.HasParamKeyword(parameters[i])) continue;

                    var copyOfArguments = new List<string>(arguments) { [i] = "null" };

                    lines.Add($"yield return new TestCaseData(new TestDelegate(() => " +
                              $"{instanceName}.{methodName}({string.Join(", ", copyOfArguments)})))" +
                              $".SetName(\"{methodName} null {parameters[i].Name}\");");
                }
            }

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
                                                   .Into(c => GetVariableDeclarations(c, false));

            foreach (var declaration in variableDeclarations)
            {
                lines.Add(declaration);
            }

            if (variableDeclarations.Any()) lines.Add(string.Empty);

            lines.Add("// ReSharper disable ObjectCreationAsStatement");
            foreach (var ctor in constructors)
            {
                var typeName = ctor.DeclaringType?.Name;
                var parameterTypes = ctor.GetParameters();
                var arguments = GetMethodArguments(ctor, true);

                for (int i = 0; i < parameterTypes.Length; i++)
                {
                    if (parameterTypes[i].ParameterType.IsValueType) continue;
                    var name = parameterTypes[i].Name;
                    var copyOfArguments = new List<string>(arguments) { [i] = "null" };

                    lines.Add($"yield return new TestCaseData(new TestDelegate(() => new " +
                              $"{typeName}({string.Join(", ", copyOfArguments)}))).SetName(\"null {name}\");");
                }
            }
            lines.Add("// ReSharper restore ObjectCreationAsStatement");

            return lines.ToArray();
        }

        private string[] GetMethodArguments(MethodBase methodBase, bool assumeDummyVariablesExist)
        {
            var parameters = methodBase.GetParameters();

            var arguments = assumeDummyVariablesExist
                ? parameters.Select(p => p.ParameterType).Select(t => m_TypeDescriber.GetVariableName(t, Scope.Local)).ToArray()
                : parameters.Select(p => p.ParameterType).Select(m_TypeDescriber.GetInstance).ToArray();

            for (var index = 0; index < parameters.Length; index++)
            {
                var pInfo = parameters[index];
                var type = pInfo.ParameterType;
                // Add ref/out keywords where needed
                if (m_TypeDescriber.HasParamKeyword(pInfo))
                {
                    var argument = arguments[index];
                    arguments[index] = $"{m_TypeDescriber.ParamKeyword(pInfo)} {argument}";
                }
                // If not a ref then check if we can use a literal / immediate value instead of variable
                else if (m_TypeDescriber.IsImmediateValueTolerable(type))
                {
                    arguments[index] = m_TypeDescriber.GetInstance(type);
                }
            }

            return arguments;
        }

        private string[] GetVariableDeclarations(IEnumerable<ParameterInfo> parameters, bool setToNull)
        {
            return parameters.Where(p => !p.ParameterType.IsValueType || m_TypeDescriber.HasParamKeyword(p) || p.ParameterType.IsEnum)
                             .Where(p => !m_TypeDescriber.IsImmediateValueTolerable(p.ParameterType) || m_TypeDescriber.HasParamKeyword(p))
                             .Select(p => p.ParameterType)
                             .Distinct()
                             .Select(t => m_TypeDescriber.GetLocalVariableDeclaration(t, setToNull))
                             .ToArray();
        }

        private string[] GetStubbedInstantiation(Type type)
        {
            var sb = new StringBuilder();

            var variableName = m_TypeDescriber.GetVariableName(type, Scope.Local);
            var nameForCSharp = m_TypeDescriber.GetTypeNameForCSharp(type);
            var ctor = type.GetConstructors().First();

            var arguments = GetMethodArguments(ctor, assumeDummyVariablesExist: false);

            var declarationStart = $"var {variableName} = new {nameForCSharp}(";

            var offset = new string(' ', declarationStart.Length);

            sb.Append(declarationStart);
            for (int i = 0; i < arguments.Length; i++)
            {
                sb.Append(arguments[i]);
                if (i < arguments.Length - 1)
                {
                    sb.AppendLine(",");
                    sb.Append(offset);
                }
            }
            sb.AppendLine(");");

            return sb.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}