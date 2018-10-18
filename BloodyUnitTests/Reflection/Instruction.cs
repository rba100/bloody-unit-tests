using System.Reflection;
using System.Reflection.Emit;

namespace BloodyUnitTests.Reflection
{
    public sealed class Instruction
    {
        public int Offset { get; set; }

        public OpCode OpCode { get; set; }

        public object Data { get; set; }

        public override string ToString()
        {
            return $"{Offset:X4} : {OpCode} {FormatData()}";
        }

        private string FormatData()
        {
            if (Data == null) return string.Empty;

            var methodInfo = Data as MethodInfo;
            if (methodInfo != null)
            {
                return methodInfo.ToPrettyString();
            }

            var constructorInfo = Data as ConstructorInfo;
            if (constructorInfo != null)
            {
                return constructorInfo.ToPrettyString();
            }

            return Data.ToString();
        }
    }
}
