using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BloodyUnitTests
{
    public class NullTestCreator
    {
        private readonly TypeDescriber m_TypeDescriber = new TypeDescriber();

        public string GetNullConstructorArgsTest(Type type)
        {
            var typeName = type.Name;
            StringBuilder sb = new StringBuilder();
            var testCaseSource = $"{typeName}_constructor_null_argument_testcases";
            sb.AppendLine($"public static IEnumerable<TestCaseData> {testCaseSource}()");
            sb.AppendLine("{");
            foreach (var line in GetConstructorNullTestCaseSource(type))
            {
                sb.AppendLine(new String(' ', 4) + line);
            }

            sb.AppendLine("}");

            sb.AppendLine();

            sb.AppendLine($"[TestCaseSource(nameof({testCaseSource}))]");
            sb.AppendLine($"public void {typeName}_constructor_null_argument_test(TestDelegate testDelegate)");
            sb.AppendLine("{");

            sb.AppendLine("    Assert.Throws<ArgumentNullException>(testDelegate);");
            sb.AppendLine("}");
            return sb.ToString();
        }

        public string GetNullMethodArgsTest(Type type)
        {
            var typeName = type.Name;
            StringBuilder sb = new StringBuilder();
            var testCaseSource = $"{typeName}_method_null_argument_testcases";
            sb.AppendLine($"public static IEnumerable<TestCaseData> {testCaseSource}()");
            sb.AppendLine("{");
            foreach (var line in GetMethodNullTestCaseSource(type))
            {
                sb.AppendLine(new String(' ', 4) + line);
            }

            sb.AppendLine("}");

            sb.AppendLine();

            sb.AppendLine($"[TestCaseSource(nameof({testCaseSource}))]");
            sb.AppendLine($"public void {typeName}_method_null_argument_test(TestDelegate testDelegate)");
            sb.AppendLine("{");

            sb.AppendLine("    Assert.Throws<ArgumentNullException>(testDelegate);");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private string[] GetMethodNullTestCaseSource(Type type)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                              .Where(m => AssemblyHelper.IsTestableMethod(type, m))
                              .ToArray();

            if (!methods.Any())
                methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
                              .Where(m => AssemblyHelper.IsTestableMethod(type, m))
                              .ToArray();

            return ToMethodNullArgsTestCaseSourceImp(methods, type);
        }

        private string[] ToMethodNullArgsTestCaseSourceImp(MethodInfo[] infos, Type type)
        {
            var lines = new List<string>();

            if (!infos.Any()) return lines.ToArray();

            var variableDeclarations = GetVariableDeclarations(infos.SelectMany(i => i.GetParameters()));
            lines.AddRange(variableDeclarations);
            if (variableDeclarations.Any()) lines.Add(string.Empty);

            var instanceName = m_TypeDescriber.GetVariableName(type);

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
                    if (m_TypeDescriber.HasParamKeyWord(parameters[i])) continue;

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
                                                   .Into(GetVariableDeclarations);

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

        private string[] GetMethodArguments(MethodBase methodBase, bool useVariables)
        {
            var parameters = methodBase.GetParameters();

            var arguments = useVariables
                ? parameters.Select(p => p.ParameterType).Select(m_TypeDescriber.GetDummyVariableName).ToArray()
                : parameters.Select(p => p.ParameterType).Select(m_TypeDescriber.GetDummyInstantiation).ToArray();

            for (var index = 0; index < parameters.Length; index++)
            {
                var pInfo = parameters[index];
                var type = pInfo.ParameterType;
                if (m_TypeDescriber.HasParamKeyWord(pInfo))
                {
                    var argument = arguments[index];
                    arguments[index] = $"{m_TypeDescriber.ParamKeyWord(pInfo)} {argument}";
                }
                else if (m_TypeDescriber.IsImmediateValueTolerable(type))
                {
                    arguments[index] = m_TypeDescriber.GetDummyInstantiation(type);
                }
            }

            return arguments;
        }

        private string[] GetVariableDeclarations(IEnumerable<ParameterInfo> parameters)
        {
            string asDeclaration(Type type) => $"var {m_TypeDescriber.GetDummyVariableName(type)}"
                                             + $" = {m_TypeDescriber.GetDummyInstantiation(type)};";

            return parameters.Where(p => !p.ParameterType.IsValueType || m_TypeDescriber.HasParamKeyWord(p))
                             .Where(p => !m_TypeDescriber.IsImmediateValueTolerable(p.ParameterType) || m_TypeDescriber.HasParamKeyWord(p))
                             .Select(p => p.ParameterType)
                             .Distinct()
                             .Select(asDeclaration)
                             .ToArray();
        }

        private string[] GetStubbedInstantiation(Type type)
        {
            var sb = new StringBuilder();

            var variableName = m_TypeDescriber.GetVariableName(type);
            var nameForCSharp = m_TypeDescriber.GetTypeNameForCSharp(type);
            var ctor = type.GetConstructors().First();

            var arguments = GetMethodArguments(ctor, false);

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
            sb.AppendLine(")");

            return sb.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}