using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

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
                                               .Select(e=>e.Attribute("cref")?.Value)
                                               .Where(v=>v != null)
                                               .Select(str=>str.Substring(2)); // remove "T:"

                    methodsToExceptions.Add(ifMethod, new List<string>(exceptions));
                }
            }

            return lines.ToArray();
        }
    }
}