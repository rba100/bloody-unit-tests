using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BloodyUnitTests.Engine.Util
{
    public class FileLocator
    {
        private static T? ToNullable<T>(T item) where T : struct { return item; }

        public static string GetBinaryForSourceFile(string fileName)
        {
            var ns = GetNameSpaceFromCsFile(fileName);
            var sourceDirectory = Path.GetDirectoryName(fileName);
            var binaries = GetBinariesNearFolder(sourceDirectory, ns);
            return binaries.OrderByDescending(b => b.score)
                           .Select(ToNullable)
                           .FirstOrDefault()?.filePath;
        }

        public static IEnumerable<(int score, string filePath)> GetBinariesNearFolder(string folderPath,
                                                                                      string[] nameSpaceParts)
        {
            string[] binaries = null;
            var folder = folderPath;
            const int maxDepth = 4;
            foreach (var _ in Enumerable.Range(1, maxDepth))
            {
                binaries = GetAllBinaries(folder).ToArray();
                if (binaries.Any()) break;
                folder = Directory.GetParent(folder).FullName;
            }

            if (binaries?.Any() == true)
            {
                foreach (var filePath in binaries)
                {
                    var fileName = Path.GetFileName(filePath);
                    var score = GetScore(fileName, nameSpaceParts);
                    if (score == 0) continue;
                    yield return (score, filePath);
                }
            }
        }

        private static int GetScore(string fileName, string[] nameSpaceParts)
        {
            int score = 0, curIndex = 0;
            foreach (var part in nameSpaceParts)
            {
                curIndex = fileName.Substring(curIndex).IndexOf(part, StringComparison.Ordinal);
                if (curIndex == -1) break;
                score++;
            }
            return score;
        }

        public static IEnumerable<string> GetAllBinaries(string rootDir)
        {
            var dlls = Directory.GetFiles(rootDir).Where(f =>
            {
                var l = f.ToLower();
                return l.EndsWith("dll") || l.EndsWith("exe");
            });
            var subFolders = Directory.GetDirectories(rootDir);
            return dlls.Concat(subFolders.SelectMany(GetAllBinaries));
        }

        public static string GetClassNameFromCsFile(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                var classDeclr = lines.First(l => l.Contains(" class "));
                var parts = classDeclr.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                var index = Array.IndexOf(parts, "class");
                return parts[index + 1];
            }
            catch
            {
                return null;
            }
        }

        public static string[] GetNameSpaceFromCsFile(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath);
                var nsDeclaration = lines.First(l => l.Contains("namespace"));
                return nsDeclaration.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[1]
                                    .Split('.');
            }
            catch
            {
                return null;
            }
        }
    }
}
