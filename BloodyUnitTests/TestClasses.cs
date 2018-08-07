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
        private readonly IReadOnlyDictionary<int, string> m_Mappings;

        public _TestClass(ITestDecorator innerTestDecorator,
                          InnerObject dependency,
                          IList<string> names,
                          IReadOnlyDictionary<int, string> mappings)
        {
            m_InnerTestDecorator = innerTestDecorator;
            m_Mappings = mappings;
        }

        public _TestClass(ITestDecorator innerTestDecorator,
                          Func<DateTime> getDate,
                          Action<string> logger, 
                          IReadOnlyDictionary<int, string> mappings)
        {
            m_InnerTestDecorator = innerTestDecorator;
            m_Mappings = mappings;
        }

        public string Process(string argument)
        {
            return m_InnerTestDecorator.Process(argument);
        }

        public int Aggregate(int[] arguments)
        {
            return m_InnerTestDecorator.Aggregate(arguments);
        }

        public void LogAll(string[] messages, DateTime time)
        {
            
        }
    }

    internal interface ITestDecorator
    {
        string Process(string argument);

        int Aggregate(int[] argument);
    }

    class InnerObject
    {
        public InnerObject(IReadOnlyCollection<int> ints, bool writeable)
        {

        }
    }

    class _TestClass2
    {
        public void Thing(DateTime? val, object o)
        {

        }
    }
}