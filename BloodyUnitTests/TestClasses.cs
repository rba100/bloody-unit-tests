using System;
using System.Collections.Generic;

// ReSharper disable ArrangeTypeModifiers
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace BloodyUnitTests
{
    class _TestClass
    {
        public _TestClass(InnerObject innerObject,
                          IList<string> strings,
                          IDisposable dependency,
                          Func<DateTime> getDate,
                          Action<string> logger)
        {
        }

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
