using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BloodyUnitTests.ContentCreators;

namespace BloodyUnitTests
{
    public static class TestFixtureCreator
    {
        private static readonly string[] s_DefaultNamesSpaces = { "System", "System.Text", "NUnit.Framework", "Rhino.Mocks" };

        public static string CreateTestFixture(Type classToTest)
        {
            var sb = new StringBuilder();

            var contentCreators = new IContentCreator[]
            {
                new ConstructorNullArgsTestCreator(),
                new MethodNullArgsTestCreator(),
                new PassThroughTestCreator(),
                new HelperMethodContentCreator(),
                new TestFactoryCreator()
            };

             var contents = contentCreators.Select(c => c.Create(classToTest)).ToArray();
            var namesSpaces = contents.SelectMany(c => c.NamesSpaces);

            // Start with namespace declarations
            var namesspaces = s_DefaultNamesSpaces.Union(namesSpaces)
                                                  .Distinct().ToList();

            var systemNamespaces = namesspaces.Where(ns => ns.StartsWith("System")).OrderBy(ns=>ns).ToList();
            var customNamespaces = namesspaces.Except(systemNamespaces).OrderBy(ns => ns).ToList();

            systemNamespaces.ForEach(ns=> sb.AppendLine($"using {ns};"));
            sb.AppendLine();
            customNamespaces.ForEach(ns => sb.AppendLine($"using {ns};"));

            sb.AppendLine();
            sb.AppendLine("[TestFixture]");
            sb.AppendLine($"public class {classToTest.Name}Tests");
            sb.AppendLine("{");

            for (var i = 0; i < contents.Length; i++)
            {
                var content = contents[i];
                if (i>0 && content.LinesOfCode.Any()) sb.AppendLine();
                AddContentWithIdentation(sb, content, 4);
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        private static void AddContentWithIdentation(StringBuilder builder, ClassContent content, int identation)
        {
            foreach (var loc in Indent(content.LinesOfCode, identation))
            {
                builder.AppendLine(loc);
            }
        }

        private static IEnumerable<string> Indent(IEnumerable<string> strings, int identation)
        {
            var indent = new string(' ', identation);
            foreach (var s in strings)
            {
                yield return indent + s;
            }
        }
    }
}
