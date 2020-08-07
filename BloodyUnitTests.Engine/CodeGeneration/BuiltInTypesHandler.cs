using System;
using System.Reflection;

namespace BloodyUnitTests.Engine.CodeGeneration
{
    class BuiltInTypesHandler : IRecursiveTypeHandler
    {
        private ITypeHandler m_RootHandler;

        public bool CanGetInstantiation(Type type)
        {
            return type == typeof(string) 
                || type == typeof(char)
                || type == typeof(bool)
                || type == typeof(IntPtr)
                || type == typeof(Assembly)
                || type == typeof(MethodBase)
                || type == typeof(Uri)
                || type == typeof(Type);
        }

        public string GetInstantiation(Type type, bool interestingValue)
        {
            if (type == typeof(bool)) return interestingValue.ToString().ToLower();
            if (type == typeof(IntPtr)) return interestingValue ? "new IntPtr(1)" : "IntPtr.Zero";
            if (type == typeof(string)) return interestingValue ? "\"value\"" : "\"\"";
            if (type == typeof(char)) return interestingValue ? "'X'" : "' '";
            if (type == typeof(Type)) return "typeof(object)";
            if (type == typeof(Uri)) return "new Uri(\"https://example.com\")";
            if (type == typeof(Assembly)) return "Assembly.GetCallingAssembly()";
            if (type == typeof(MethodBase)) return "MethodBase.GetCurrentMethod()";

            throw new NotSupportedException();
        }

        public bool IsInstantiationTerse(Type type)
        {
            return type == typeof(string)
                || type == typeof(char) 
                || type == typeof(bool) 
                || type == typeof(IntPtr)
                || type == typeof(Type);
        }

        public bool CanGetNameForIdentifier(Type type)
        {
            return false;
        }

        public string GetNameForIdentifier(Type type)
        {
            throw new NotSupportedException();
        }

        public bool CanGetNameForCSharp(Type type)
        {
            return false;
        }

        public string GetNameForCSharp(Type type)
        {
            throw new NotSupportedException();
        }

        public INamespaceTracker GetNamespaceTracker()
        {
            return m_RootHandler.GetNamespaceTracker();
        }

        public void SetRoot(ITypeHandler handler)
        {
            m_RootHandler = handler;
        }
    }
}