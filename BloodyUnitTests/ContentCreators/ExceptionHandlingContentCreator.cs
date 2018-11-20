using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using BloodyUnitTests.Reflection;
using BloodyUnitTests.Util;

namespace BloodyUnitTests.ContentCreators
{
    public class ExceptionHandlingContentCreator : IContentCreator
    {
        public ClassContent Create(Type type)
        {
            var csharpService = new CSharpService();
            var testCode = GenerateTestCode(type, csharpService);
            if (!testCode.Any()) return ClassContent.NoContent;
            return new ClassContent(testCode, csharpService.GetNameSpaces());
        }

        private string[] GenerateTestCode(Type type, CSharpService csharpService)
        {
            var lines = new List<string>();

            var interfaceDependencies = type.GetConstructors()
                                            .SelectMany(c => c.GetParameters())
                                            .Select(p => p.ParameterType)
                                            .Distinct()
                                            .Where(t => t.IsInterface);

            var methodsToExceptions = new Dictionary<MethodInfo, List<string>>();
            var exceptionalCircumstances = new List<ExceptionalCircumstance>();

            // Get all exceptions that could be thrown by all dependent interfaces
            foreach (var iface in interfaceDependencies)
            {
                var sourceAssemblyVar = iface.Assembly.Location;
                var directory = Path.GetDirectoryName(sourceAssemblyVar);
                var fileRoot = Path.GetFileNameWithoutExtension(sourceAssemblyVar);
                var xmlDocsPath = Path.Combine(directory, fileRoot + ".xml");
                var exists = File.Exists(xmlDocsPath);
                if (!exists) continue;
                var xmlDocument = XDocument.Load(xmlDocsPath);
                var allMemberElements = xmlDocument.Descendants("member").ToArray();
                foreach (var ifMethod in iface.GetMethods())
                {
                    var parameters = ifMethod.GetParameters().Select(p => p.ParameterType.FullName);
                    var paramString = string.Join(",", parameters);
                    var docName = $"M:{iface.FullName}.{ifMethod.Name}({paramString})";
                    var docElement =
                        allMemberElements.FirstOrDefault(m => m.Attribute("name")?.Value == docName);

                    if (docElement == null) continue;

                    var exceptions = docElement.Descendants("exception")
                                               .Select(e => e.Attribute("cref")?.Value)
                                               .Where(v => v != null)
                                               .Select(str => str.Substring(2)); // remove "T:"

                    methodsToExceptions.Add(ifMethod, new List<string>(exceptions));
                }
            }

            // Scan each method to see if it invokes a method than can throw
            foreach (var methodInfo in type.GetMethods())
            {
                var innerCalls = ReflectionUtils.GetCalledMethods(methodInfo)
                                                .Where(m => methodsToExceptions.ContainsKey(m));

                exceptionalCircumstances.AddRange(
                    innerCalls.SelectMany(call => methodsToExceptions[call].Select(
                        exName => new ExceptionalCircumstance(methodInfo, call, exName))));
            }

            var tests = exceptionalCircumstances.Select((c,i) =>
            {
                var methodCode = WriteTestMethod(type, c, csharpService);
                return i > 0 ? new[] { "" }.Concat(methodCode) : methodCode;
            });

            lines.AddRange(tests.SelectMany(line => line));

            return lines.ToArray();
        }

        private IEnumerable<string> WriteTestMethod(Type typeUnderTest,
                                                    ExceptionalCircumstance circumstance,
                                                    CSharpService cSharpService)
        {
            var sanitisedExName = circumstance.ExceptionType.Split('.').Last();
            var testMethodName = circumstance.OuterMethod.Name;
            var innerMethod = circumstance.InnerMethod;
            var innerDependency = circumstance.InnerMethod.DeclaringType;

            var ctor = typeUnderTest.GetConstructors()
                                    .FirstOrDefault(c => c.GetParameters()
                                                          .Any(p => p.ParameterType == innerDependency));
            if (ctor == null) yield break;

            var ctorParams = ctor.GetParameters();
            var paramName = ctorParams.FirstOrDefault(p => p.ParameterType == innerDependency).Name;
            var dummyArgs = string.Join(",", innerMethod.GetParameters().Select(p => DummyValue(p, cSharpService)));

            yield return "[Test]";
            yield return $"public void {testMethodName}_handles_{sanitisedExName}()";
            yield return "{";
            yield return $"    var factory = new {typeUnderTest.Name}Factory();";
            yield return $"    factory.{StringUtils.ToUpperInitial(paramName)}";
            yield return $"           .Stub(x => x.{innerMethod.Name}({dummyArgs}))";
            yield return $"           .IgnoreArguments()";
            yield return $"           .Throw(new {circumstance.ExceptionType}());";
            var identifier = cSharpService.GetIdentifier(typeUnderTest, VarScope.Local);
            yield return $"    var {identifier} = factory.Create();";
            yield return $"    throw new NotImplementedException(\"Test placeholder not implemented.\");";
            yield return "}";
        }

        private string DummyValue(ParameterInfo arg, CSharpService cSharpService)
        {
            return !arg.ParameterType.IsValueType || arg.ParameterType.IsArray
                ? "null"
                : cSharpService.GetInstantiation(arg.ParameterType, nonDefault: false);
        }

        private class ExceptionalCircumstance
        {
            public MethodInfo OuterMethod { get; }
            public MethodInfo InnerMethod { get; }
            public string ExceptionType { get; }

            public ExceptionalCircumstance(MethodInfo outerMethod,
                                       MethodInfo innerMethod,
                                       string exceptionType)
            {
                OuterMethod = outerMethod;
                InnerMethod = innerMethod;
                ExceptionType = exceptionType;
            }
        }
    }
}