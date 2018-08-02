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

    class _TestDecorator : ITestDecorator
    {
        private readonly ITestDecorator m_InnerTestDecorator;

        public _TestDecorator(ITestDecorator innerTestDecorator)
        {
            m_InnerTestDecorator = innerTestDecorator;
        }

        public string Process(string argument)
        {
            return m_InnerTestDecorator.Process(argument);
        }

        public int Process(int[] arguments)
        {
            return m_InnerTestDecorator.Process(arguments);
        }
    }

    internal interface ITestDecorator
    {
        string Process(string argument);
        int Process(int[] argument);
    }

    class InnerObject
    {
        public InnerObject(IReadOnlyCollection<int> ints)
        {

        }
    }
}