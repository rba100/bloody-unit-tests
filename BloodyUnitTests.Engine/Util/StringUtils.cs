using System;
using System.Linq;

namespace BloodyUnitTests.Engine.Util
{
    static class StringUtils
    {
        private static readonly char[] s_Vowels = { 'a', 'e', 'i', 'o', 'u' };

        private static readonly string[] s_Siblants = { "s", "x", "ch", "sh", "z" };

        internal static string ToLowerInitial(string str)
        {
            return str[0].ToString().ToLower() + new string(str.Skip(1).ToArray());
        }

        internal static string ToUpperInitial(string str)
        {
            return str[0].ToString().ToUpper() + new string(str.Skip(1).ToArray());
        }

        internal static string Pluralise(string input)
        {
            if (input.Length > 2 && input.EndsWith("y") && !s_Vowels.Contains(input[input.Length - 2]))
            {
                return input.Substring(0, input.Length - 1) + "ies";
            }
            if (s_Siblants.Any(input.EndsWith)) return $"{input}es";
            return $"{input}s";
        }
        internal static string GetClassCategory(Type type)
        {
            if (type.Name.EndsWith("Repository")) return "Repositories";
            if (type.Name.EndsWith("Service")) return "Services";
            if (type.Name.EndsWith("Controller")) return "Controllers";
            if (type.Name.EndsWith("Exception")) return "Exceptions";
            if (type.Name.EndsWith("Manager")) return "Managers";

            var name = type.GetInterfaces()
                           .Select(i => i.Name)
                           .Select(i => i.StartsWith("I") ? new string(i.Skip(1).ToArray()) : i)
                           .FirstOrDefault(i => type.Name.EndsWith(i));

            return name == null ? "Domain" : Pluralise(name);
        }
    }
}
