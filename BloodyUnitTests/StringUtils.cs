using System.Linq;

namespace BloodyUnitTests
{
    static class StringUtils
    {
        internal static string ToLowerInitial(string str)
        {
            return str[0].ToString().ToLower() + new string(str.Skip(1).ToArray());
        }

        internal static string ToUpperInitial(string str)
        {
            return str[0].ToString().ToUpper() + new string(str.Skip(1).ToArray());
        }
    }
}
