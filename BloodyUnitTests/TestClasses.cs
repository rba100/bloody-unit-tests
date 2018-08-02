using System;
using System.Collections.Generic;

// ReSharper disable ArrangeTypeModifiers
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace BloodyUnitTests
{
    class _TestClass : ITestDecorator
    {
        private readonly ITestDecorator m_InnerTestDecorator;

        public _TestClass(ITestDecorator innerTestDecorator,
                          InnerObject dependency,
                          IList<string> names)
        {
            m_InnerTestDecorator = innerTestDecorator;
        }

        public _TestClass(ITestDecorator innerTestDecorator,
                          Func<DateTime> getDate,
                          Action<string> logger)
        {
            m_InnerTestDecorator = innerTestDecorator;
        }

        public string Process(string argument)
        {
            return m_InnerTestDecorator.Process(argument);
        }

        public int Aggregate(int[] arguments)
        {
            return m_InnerTestDecorator.Aggregate(arguments);
        }
    }

    internal interface ITestDecorator
    {
        string Process(string argument);

        int Aggregate(int[] argument);
    }

    class InnerObject
    {
        public InnerObject(IReadOnlyCollection<int> ints)
        {

        }
    }
}