using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace BloodyUnitTests
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
            
            if(outputPaths.Values.Any(File.Exists)) throw new Exception(@"Some test files already exist");

            var typesToFixtures = testableClasses.ToDictionary(t => t, TestFixtureCreator.CreateTestFixture);

            foreach (var fixture in typesToFixtures)
            {
                if (fixture.Value == null) continue;
                var directory = Path.GetDirectoryName(outputPaths[fixture.Key])
                                ?? throw new Exception($"Could not deduce directory name for {outputPaths[fixture.Key]}");
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                File.WriteAllText(outputPaths[fixture.Key], fixture.Value);
            }
        }

        private static string GetPath(string directoryBase, Type type)
        {
            var fileName = StringUtils.ToUpperInitial($"{FileSafe(type.Name)}Tests.cs");
            return Path.Combine(directoryBase, GetSubFolder(type), fileName);
        }

        private static string FileSafe(string input)
        {
            var illegalChars = input.Where(s_PathInvalidChars.Contains).Distinct().ToArray();
            if (!illegalChars.Any()) return input;
            return new string(input.Except(illegalChars).ToArray());
        }

        private static string GetSubFolder(Type type)
        {
            if (type.Name.EndsWith("Repository")) return "Repositories";
            if (type.Name.EndsWith("Service"))    return "Services";
            if (type.Name.EndsWith("Controller")) return "Controllers";
            if (type.Name.EndsWith("Exception"))  return "Exceptions";
            if (type.Name.EndsWith("Manager"))    return "Managers";

            var name = type.GetInterfaces()
                            .Select(i => i.Name)
                            .Select(i => i.StartsWith("I") ? new string(i.Skip(1).ToArray()) : i)
                            .FirstOrDefault(i => type.Name.EndsWith(i));

            return name == null ? "Domain" : StringUtils.Pluralise(name);
        }
    }
}
