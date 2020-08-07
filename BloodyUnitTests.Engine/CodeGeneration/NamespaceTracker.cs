using System.Collections.Generic;
using System.Linq;

namespace BloodyUnitTests.Engine.CodeGeneration
{
    class NamespaceTracker : INamespaceTracker
    {
        private readonly HashSet<string> m_Namespaces = new HashSet<string>();

        public void RecordNamespace(string namespc)
        {
            m_Namespaces.Add(namespc);
        }

        public string[] GetNamespaces()
        {
            return m_Namespaces.ToArray();
        }
    }
}