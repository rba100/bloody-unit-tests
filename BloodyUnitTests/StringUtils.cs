using System;
using System.Linq;
using System.Text;

namespace BloodyUnitTests
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
    }

    internal class IndentedStringBuilder
    {
        private readonly int m_IndentationIncrement;
        private readonly StringBuilder m_StringBuilder = new StringBuilder();
        private readonly Unindentor m_Unindentor;

        private int m_CurrentIndentation = 0;
        private string m_Header = "";

        public IndentedStringBuilder(int indentationIncrement)
        {
            m_IndentationIncrement = indentationIncrement;
            m_Unindentor = new Unindentor(this);
        }

        public void Append(string str)
        {
            m_StringBuilder.Append($"{str}");
        }

        public void AppendLine(string str = "")
        {
            if (str == "") m_StringBuilder.AppendLine();
            else m_StringBuilder.AppendLine($"{m_Header}{str}");
        }

        public void Indent()
        {
            m_CurrentIndentation += m_IndentationIncrement;
            m_Header = new string(' ', m_CurrentIndentation);
        }

        public IDisposable WithIndent()
        {
            m_CurrentIndentation += m_IndentationIncrement;
            m_Header = new string(' ', m_CurrentIndentation);
            return m_Unindentor;
        }

        public void Unindent()
        {
            m_CurrentIndentation -= m_IndentationIncrement;
            m_Header = new string(' ', m_CurrentIndentation);
        }

        public override string ToString()
        {
            return m_StringBuilder.ToString();
        }

        private class Unindentor : IDisposable
        {
            private readonly IndentedStringBuilder m_StringBuilder;

            public Unindentor(IndentedStringBuilder stringBuilder)
            {
                m_StringBuilder = stringBuilder;
            }

            public void Dispose()
            {
                m_StringBuilder.Unindent();
            }
        }
    }
}
