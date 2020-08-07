using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BloodyUnitTests.Engine.Util;

namespace BloodyUnitTests.Engine
{
    public class ProjectWriter
    {
        private static readonly char[] s_PathInvalidChars = Path.GetInvalidFileNameChars();

        public void WriteAllTests(Assembly assembly, string nameSpacePrefixFilter, string directoryForOutput)
        {
            var testableClasses = assembly.GetTestableClassTypes()
                                          .Where(c=> string.IsNullOrWhiteSpace(nameSpacePrefixFilter) 
                                                     || c.Namespace != null && c.Namespace.StartsWith(nameSpacePrefixFilter))
                                          .ToArray();

            if(!testableClasses.Any()) throw new ArgumentException($@"No classes found with '{nameSpacePrefixFilter}' prefix",
                                                                   nameof(nameSpacePrefixFilter));

            var outputPaths = testableClasses.ToDictionary(t => t, t => GetPath(directoryForOutput, t));

            var typesToFixtures = testableClasses.ToDictionary(t => t, TestFixtureCreator.CreateTestFixture);

            foreach (var fixture in typesToFixtures)
            {
                if (fixture.Value == null) continue;
                if(File.Exists(outputPaths[fixture.Key])) continue;

                var directory = Path.GetDirectoryName(outputPaths[fixture.Key])
                     ?? throw new Exception($"Could not deduce directory name for {outputPaths[fixture.Key]}");
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                File.WriteAllText(outputPaths[fixture.Key], fixture.Value);
            }
        }

        private string GetPath(string directoryBase, Type type)
        {
            var fileName = StringUtils.ToUpperInitial($"{FileSafe(type.Name)}Tests.cs");
            return Path.Combine(directoryBase, StringUtils.GetClassCategory(type), fileName);
        }

        private static string FileSafe(string input)
        {
            var illegalChars = input.Where(s_PathInvalidChars.Contains).Distinct().ToArray();
            if (!illegalChars.Any()) return input;
            return new string(input.Except(illegalChars).ToArray());
        }
    }
}
