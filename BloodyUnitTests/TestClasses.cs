using System.Collections.Generic;

namespace BloodyUnitTests
{
    class TestClass
    {
        public void TestMethod(InnerObject innerObject, string[] strings)
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
