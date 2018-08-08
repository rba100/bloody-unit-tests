﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BloodyUnitTests.CodeGeneration;

namespace BloodyUnitTests
{
    public class ProjectWriter
    {
        private static readonly ITypeHandler s_TypeHandler = TypeHandlerFactory.Create();

        public void WriteAllTests(Assembly assembly, string nameSpacePrefixFilter, string directoryForOutput)
        {
            if(!Directory.Exists(directoryForOutput)) throw new ArgumentException(@"output folder does not exist",
                                                                                  nameof(directoryForOutput));

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
                var directory = Path.GetDirectoryName(outputPaths[fixture.Key]);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                File.WriteAllText(outputPaths[fixture.Key], fixture.Value);
            }
        }

        private static string GetPath(string directoryBase, Type type)
        {
            string fileName = string.Empty;
            if (type.Name.EndsWith("Repository")) fileName += "Repositories\\";
            else if (type.Name.EndsWith("Service")) fileName += "Services\\";
            else if (type.Name.EndsWith("Controller")) fileName += "Controllers\\";
            else if (type.Name.EndsWith("Exception")) fileName += "Exceptions\\";
            else if (type.Name.EndsWith("Manager")) fileName += "Managers\\";
            else fileName += GetSubFolder(type);

            fileName += StringUtils.ToUpperInitial($"{s_TypeHandler.GetNameForIdentifier(type)}Tests.cs");

            return Path.Combine(directoryBase, fileName);
        }

        private static string GetSubFolder(Type type)
        {
            if (type.Name.EndsWith("Repository")) return "Repositories\\";
            if (type.Name.EndsWith("Service"))    return "Services\\";
            if (type.Name.EndsWith("Controller")) return "Controllers\\";
            if (type.Name.EndsWith("Exception"))  return "Exceptions\\";
            if (type.Name.EndsWith("Manager"))    return "Managers\\";

            var name = type.GetInterfaces()
                            .Select(i => i.Name)
                            .Select(i => i.StartsWith("I") ? new string(i.Skip(1).ToArray()) : i)
                            .FirstOrDefault(i => type.Name.EndsWith(i));
            if (name == null) return "Domain\\";
            return name + "s\\";
        }
    }
}
