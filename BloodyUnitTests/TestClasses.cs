using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BloodyUnitTests
{
    class TestClass
    {
        public TestClass(IList<string> strings,
                         IDisposable dependency)
        {
            Strings = strings;
            Dependency = dependency;
        }

        private IList<string> Strings { get; }
        private IDisposable Dependency { get; }

        public void TestMethod(InnerObject innerObject, List<string> strings)
        {

        }
    }

    class InnerObject
    {
        public InnerObject(IReadOnlyCollection<int> ints)
        {

        }
    }
}
