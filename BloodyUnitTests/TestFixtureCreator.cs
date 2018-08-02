using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BloodyUnitTests.ContentCreators;

namespace BloodyUnitTests
{
    public static class TestFixtureCreator
    {
        private static string[] s_DefaultNamesSpaces = {"System"};

        public static string CreateTestFixture(Type classToTest)
        {
            var sb = new StringBuilder();

            var testObjectCreator = new TestObjectCreator();
            var nullTestCreator = new NullTestCreator();

            var testFactoryContent = testObjectCreator.TestFactoryDeclaration(classToTest);
            var helperMethodContent = testObjectCreator.HelperMethodContent(classToTest);
            var ctorNullArgsContent = nullTestCreator.GetNullConstructorArgTestContent(classToTest);
            var methodNullArgsContent = nullTestCreator.GetNullMethodArgTestContent(classToTest);

            // Start with name space declarations

            s_DefaultNamesSpaces.Union(testFactoryContent.UsingNamesSpaces)
                                .Union(ctorNullArgsContent.UsingNamesSpaces)
                                .Union(methodNullArgsContent.UsingNamesSpaces)
                                .Distinct()
                                .ToList()
                                .ForEach(ns => sb.AppendLine($"using {ns};"));

            sb.AppendLine();
            sb.AppendLine("[TestFixture]");
            sb.AppendLine($"public class {classToTest.Name}Tests");
            sb.AppendLine("{");

            AddContentWithIdent(sb, ctorNullArgsContent, 4);
            sb.AppendLine();
            AddContentWithIdent(sb, methodNullArgsContent, 4);
            sb.AppendLine();
            AddContentWithIdent(sb, testFactoryContent, 4);
            sb.AppendLine();
            AddContentWithIdent(sb, helperMethodContent, 4);

            sb.AppendLine("}");

            return sb.ToString();
        }

        private static void AddContentWithIdent(StringBuilder builder, ClassContent content, int indentationAmount)
        {
            foreach (var loc in Indent(content.LinesOfCode, indentationAmount))
            {
                builder.AppendLine(loc);
            }
        }

        private static IEnumerable<string> Indent(IEnumerable<string> strings, int indentationAmount)
        {
            var indent = new string(' ', indentationAmount);
            foreach (var s in strings)
            {
                yield return indent + s;
            }
        }
    }
}
