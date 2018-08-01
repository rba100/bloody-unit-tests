using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
