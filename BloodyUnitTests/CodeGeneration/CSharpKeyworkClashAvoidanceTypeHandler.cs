using System;
using System.Linq;

namespace BloodyUnitTests.CodeGeneration
{
    class CSharpKeyworkClashAvoidanceTypeHandler : ITypeHandler
    {
        private readonly ITypeHandler m_InnerHandler;

        public CSharpKeyworkClashAvoidanceTypeHandler(ITypeHandler innerHandler)
        {
            m_InnerHandler = innerHandler;
        }

        public string GetNameForIdentifier(Type type)
        {
            var rawName = m_InnerHandler.GetNameForIdentifier(type);

            // don't allow identifiers to match c# keywords.
            if (s_Keywords.Contains(rawName))
            {
                var prefix = type.IsInterface ? "stub" : "dummy";
                return $"{prefix}{rawName}";
            }

            // don't allow identifiers to exactly match type names
            if (rawName != type.Name) return rawName;

            if (type == typeof(string)) return "str";
            if (type == typeof(int)) return "number";

            return rawName + "Value";
        }

        public bool CanGetInstantiation(Type type)
        {
            return m_InnerHandler.CanGetInstantiation(type);
        }

        public string GetInstantiation(Type type, bool interestingValue)
        {
            return m_InnerHandler.GetInstantiation(type, interestingValue);
        }

        public bool IsInstantiationTerse(Type type)
        {
            return m_InnerHandler.IsInstantiationTerse(type);
        }

        public bool CanGetNameForIdentifier(Type type)
        {
            return m_InnerHandler.CanGetNameForIdentifier(type);
        }

        public bool CanGetNameForCSharp(Type type)
        {
            return m_InnerHandler.CanGetNameForCSharp(type);
        }

        public string GetNameForCSharp(Type type)
        {
            return m_InnerHandler.GetNameForCSharp(type);
        }

        public INamespaceTracker GetNamespaceTracker()
        {
            return m_InnerHandler.GetNamespaceTracker();
        }

        private static readonly string[] s_Keywords = {
            "abstract", "add", "as", "ascending",
            "async", "await", "base", "bool",
            "break", "by", "byte", "case",
            "catch", "char", "checked", "class",
            "const", "continue", "decimal", "default",
            "delegate", "descending", "do", "double",
            "dynamic", "else", "enum", "equals",
            "explicit", "extern", "false", "finally",
            "fixed", "float", "for", "foreach",
            "from", "get", "global", "goto",
            "group", "if", "implicit", "in",
            "int", "interface", "internal", "into",
            "is", "join", "let", "lock",
            "long", "namespace", "new", "null",
            "object", "on", "operator", "orderby",
            "out", "override", "params", "partial",
            "private", "protected", "public", "readonly",
            "ref", "remove", "return", "sbyte",
            "sealed", "select", "set", "short",
            "sizeof", "stackalloc", "static", "string",
            "struct", "switch", "this", "throw",
            "true", "try", "typeof", "uint",
            "ulong", "unchecked", "unsafe", "ushort",
            "using", "value", "var", "virtual",
            "void", "volatile", "where", "while",
            "yield"
        };
    }
}