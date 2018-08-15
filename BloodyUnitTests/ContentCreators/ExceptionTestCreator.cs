using System;
using System.Collections.Generic;
using System.Linq;

namespace BloodyUnitTests.ContentCreators
{
    class ExceptionTestCreator : IContentCreator
    {
        private readonly CSharpWriter m_CSharpWriter = new CSharpWriter();

        public ClassContent Create(Type type)
        {
            if (!type.IsSubclassOf(typeof(Exception))) return ClassContent.NoContent;

            var className = m_CSharpWriter.GetNameForCSharp(type);
            var lines = new List<string>();
            lines.AddRange(GetLines(c_DefaultMessage, className));
            lines.Add(string.Empty);
            lines.AddRange(GetLines(c_MessagePassDown, className));
            lines.Add(string.Empty);
            lines.AddRange(GetLines(c_RoundTripTest, className));

            return new ClassContent(lines.ToArray(),
                                    new[] 
                                    {
                                        "System.IO",
                                        "System.Runtime.Serialization.Formatters.Binary"
                                    }.Union(m_CSharpWriter.GetNameSpaces()).ToArray());
        }

        private string[] GetLines(string input, string value)
        {
            return string.Format(input, value).Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        }

        private const string c_MessagePassDown = @"[Test]
public void Exception_message_is_passed_down()
{{
    var exception = new {0}(""exceptionMesssage"");
    Assert.AreEqual(""exceptionMesssage"", exception.Message);
}}";

        private const string c_DefaultMessage = @"[Test]
public void DefaultConstructionDoesNotYieldEmptyMessage()
{{
    Assert.That(new {0}().Message,
                Is.Not.Null.Or.Empty);
}}";

        private const string c_RoundTripTest = @"[Test]
public void ParametersAreCorrectAfterSerialisationRoundTrip()
{{
    var innerException = new Exception(""fakeInnerException"");

    var exception = new {0}
        (""fakeException"", innerException);

    using (var memoryStream = new MemoryStream())
    {{
        var binaryFormatter = new BinaryFormatter();

        binaryFormatter.Serialize(memoryStream, exception);

        memoryStream.Position = 0;

        var newException = ({0})
            binaryFormatter.Deserialize(memoryStream);

        Assert.That(newException.Message, Is.EqualTo(""fakeException""));

        // ReSharper disable once PossibleNullReferenceException
        Assert.That(newException.InnerException.Message,
                    Is.EqualTo(innerException.Message));
    }}
}}";
    }
}
