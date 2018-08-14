using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BloodyUnitTests.ContentCreators;

namespace BloodyUnitTests
{
    public static class TestFixtureCreator
    {
        public static string CreateTestFixture(Type classToTest)
        {
            var sb = new StringBuilder();

            var contentCreators = new IContentCreator[]
            {
                new ExceptionTestCreator(),
                new ConstructorNullArgsTestCreator(),
                new MethodNullArgsTestCreator(),
                new InvalidArgumentTestCreator(),
                new PassThroughTestCreator(),
                new HelperMethodContentCreator(),
                new TestFactoryCreator()
            };

            var contents = contentCreators.Select(c => c.Create(classToTest))
                                          .Where(c => c.LinesOfCode.Any())
                                          .ToArray();

            if (!contents.Any()) return null;

            var namespaces = contents.SelectMany(c => c.NamesSpaces)
                                     .Union(new[] 
                                     {
                                         classToTest.Namespace,
                                         "System",
                                         "System.Collections.Generic",
                                         "NUnit.Framework"
                                     })
                                     .Distinct().ToList();

            var systemNamespaces = namespaces.Where(ns => ns.StartsWith("System")).OrderBy(ns => ns).ToList();
            var staticImports = namespaces.Where(ns => ns.StartsWith("static")).OrderBy(ns => ns).ToList();
            var customNamespaces = namespaces.Except(systemNamespaces.Union(staticImports)).OrderBy(ns => ns).ToList();

            systemNamespaces.ForEach(ns => sb.AppendLine($"using {ns};"));
            if (systemNamespaces.Any()) sb.AppendLine();
            customNamespaces.ForEach(ns => sb.AppendLine($"using {ns};"));
            if (customNamespaces.Any()) sb.AppendLine();
            staticImports.ForEach(ns => sb.AppendLine($"using {ns};"));
            if (staticImports.Any()) sb.AppendLine();

            sb.AppendLine("[TestFixture]");
            sb.AppendLine($"public class {classToTest.Name}Tests");
            sb.AppendLine("{");

            for (var i = 0; i < contents.Length; i++)
            {
                if (i > 0) sb.AppendLine();
                AddContentWithIdentation(sb, contents[i], 4);
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        private static void AddContentWithIdentation(StringBuilder builder, ClassContent content, int indentation)
        {
            foreach (var loc in content.LinesOfCode.Indent(indentation))
            {
                builder.AppendLine(loc);
            }
        }
    }
}
