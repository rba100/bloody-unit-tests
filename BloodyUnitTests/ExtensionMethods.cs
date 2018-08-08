using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BloodyUnitTests
{
    internal static class ExtensionMethods
    {
        public static TOut PipeInto<TOut, TIn>(this IEnumerable<TIn> enumerable, Func<IEnumerable<TIn>, TOut> intoFunction)
        {
            return intoFunction(enumerable);
        }

        public static IEnumerable<string> IndentBy(this IEnumerable<string> strings, int indentationAmount)
        {
            var indent = new string(' ', indentationAmount);
            foreach (var s in strings)
            {
                yield return $"{indent}{s}";
            }
        }

        public static IList<string> GetTestableClasses(this Assembly assembly)
        {
            return assembly.GetLoadableTypes()
                   .Where(t => t.IsClass && !t.IsAbstract)
                   .Select(t => t.Name)
                   .Where(n => !n.StartsWith("<"))
                   .OrderBy(n => n)
                   .ToList();
        }

        public static bool IsMethodTestable(this Type type, MethodInfo method)
        {
            return method.Name != "Equals" && !method.IsSpecialName && method.DeclaringType == type;
        }

        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }
    }
}
