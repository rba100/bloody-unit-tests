using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BloodyUnitTests
{
    public static class AssemblyHelper
    {
        public static Assembly GetAssembly(string assemblyFilePath)
        {
            return Assembly.LoadFrom(assemblyFilePath);
        }

        public static IList<string> Classes(Assembly assembly)
        {
            return GetLoadableTypes(assembly)
                   .Where(t => t.IsClass)
                   .Select(t => t.Name)
                   .Where(n => !n.StartsWith("<"))
                   .OrderBy(n => n)
                   .ToList();
        }

        public static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
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

        public static bool IsTestableMethod(Type type, MethodInfo method)
        {
            return method.Name != "Equals" && !method.IsSpecialName && method.DeclaringType == type;
        }
    }
}
