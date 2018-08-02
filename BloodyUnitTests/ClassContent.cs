using System;

namespace BloodyUnitTests
{
    public sealed class ClassContent
    {
        public ClassContent(string[] linesOfCode,
                            string[] usingNamesSpaces)
        {
            LinesOfCode = linesOfCode
                          ?? throw new ArgumentNullException(nameof(linesOfCode));
            NamesSpaces = usingNamesSpaces
                               ?? throw new ArgumentNullException(nameof(usingNamesSpaces));
        }

        public string[] LinesOfCode { get; }

        public string[] NamesSpaces { get; }
    }
}