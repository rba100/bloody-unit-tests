using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable NotAccessedField.Local
// ReSharper disable ArrangeTypeModifiers
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming

namespace BloodyUnitTests.Engine.TestClasses
{
    public class _TestClass : ITestDecorator
    {
        private readonly ITestDecorator m_InnerTestDecorator;
        private readonly Action<string> m_Logger;
        private readonly IReadOnlyDictionary<int, string> m_Mappings;

        public _TestClass(ITestDecorator innerTestDecorator,
                          InnerObject dependency,
                          IList<string> names,
                          IReadOnlyDictionary<int, string> mappings)
        {
            if (dependency == null) throw new ArgumentNullException(nameof(dependency));
            if (names == null) throw new ArgumentNullException(nameof(names));
            m_InnerTestDecorator = innerTestDecorator ?? throw new ArgumentNullException(nameof(innerTestDecorator));
            m_Mappings = mappings ?? throw new ArgumentNullException(nameof(mappings));
        }

        public _TestClass(ITestDecorator innerTestDecorator,
                          Func<DateTime> getDate,
                          Action<string> logger, 
                          IReadOnlyDictionary<int, string> mappings)
        {
            if (getDate == null) throw new ArgumentNullException(nameof(getDate));
            m_InnerTestDecorator = innerTestDecorator ?? throw new ArgumentNullException(nameof(innerTestDecorator));
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_Mappings = mappings ?? throw new ArgumentNullException(nameof(mappings));
        }

        public string Process(Guid argument)
        {
            return m_InnerTestDecorator.Process(argument);
        }

        public int Aggregate(int[] arguments)
        {
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));
            return m_InnerTestDecorator.Aggregate(arguments);
        }

        public void LogAll(string[] messages, DateTime time)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));
            if (messages.Contains(null)) throw new ArgumentException(nameof(messages));
            if (time.Kind != DateTimeKind.Utc) throw new ArgumentException();
        }
    }

    public interface ITestDecorator
    {
        string Process(Guid argument);

        /// <summary>
        /// Do stuff.
        /// </summary>
        /// <exception cref="NotSupportedException"></exception>
        int Aggregate(int[] argument);
    }

    public class InnerObject
    {
        public InnerObject(IReadOnlyCollection<int> ints, bool writeable)
        {
            if (ints == null) throw new ArgumentNullException(nameof(ints));
        }
    }

    public class _TestClass2
    {
        public void Thing(Guid? val, object o)
        {
            if (o == null) throw new ArgumentNullException(nameof(o));
        }

        public void TakesTuple((int, string) valuePair, object o)
        {
            if (o == null) throw new ArgumentNullException(nameof(o));
        }
    }

    public class _TestClass3
    {
        private readonly ITestDelegate m_TestDelegate;

        public _TestClass3(ITestDelegate testDelegate)
        {
            m_TestDelegate = testDelegate;
        }

        public int Passthrough(bool input, string otherInput)
        {
            return m_TestDelegate.TestMethod(otherInput, input);
        }
    }

    public interface ITestDelegate
    {
        int TestMethod(string input, bool otherInput);
    }
}