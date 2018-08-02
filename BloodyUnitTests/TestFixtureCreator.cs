﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BloodyUnitTests.ContentCreators;

namespace BloodyUnitTests
{
    public static class TestFixtureCreator
    {
        private static readonly string[] s_DefaultNamesSpaces = { "System", "NUnit.Framework", "Rhino.Mocks" };

        public static string CreateTestFixture(Type classToTest)
        {
            var sb = new StringBuilder();

            var testObjectCreator = new TestObjectCreator();
            var nullTestCreator = new NullTestCreator();

            var testFactoryContent = testObjectCreator.TestFactoryDeclaration(classToTest);
            var helperMethodContent = testObjectCreator.HelperMethodContent(classToTest);
            var ctorNullArgsContent = nullTestCreator.GetNullConstructorArgTestContent(classToTest);
            var methodNullArgsContent = nullTestCreator.GetNullMethodArgTestContent(classToTest);

            // Start with namespace declarations
            var namesspaces = s_DefaultNamesSpaces.Union(testFactoryContent.NamesSpaces)
                                                  .Union(ctorNullArgsContent.NamesSpaces)
                                                  .Union(methodNullArgsContent.NamesSpaces)
                                                  .Union(helperMethodContent.NamesSpaces)
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

            AddContentWithIdentation(sb, ctorNullArgsContent, 4);

            if (methodNullArgsContent.LinesOfCode.Any()) sb.AppendLine();
            AddContentWithIdentation(sb, methodNullArgsContent, 4);

            if (testFactoryContent.LinesOfCode.Any()) sb.AppendLine();
            AddContentWithIdentation(sb, testFactoryContent, 4);

            if(helperMethodContent.LinesOfCode.Any()) sb.AppendLine();
            AddContentWithIdentation(sb, helperMethodContent, 4);

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
