using System.Reflection;

namespace BloodyUnitTests.Reflection
{
    public static class ParameterInfoExtensions
    {
        public static string ToPrettyString(this ParameterInfo parameterInfo)
        {
            return ToPrettyString(parameterInfo, true);
        }

        public static string ToPrettyString(this ParameterInfo parameterInfo, bool useShortNotation)
        {
            return parameterInfo.ParameterType.ToPrettyString(useShortNotation);
        }
    }
}
