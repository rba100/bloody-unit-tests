using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace BloodyUnitTests.Engine.Reflection
{
    internal static class OpCodeTranslator
    {
        private static readonly Dictionary<short, OpCode> s_OpCodes = new Dictionary<short, OpCode>();

        static OpCodeTranslator()
        {
            Initialize();
        }

        public static OpCode GetOpCode(short value)
        {
            return s_OpCodes[value];
        }

        private static void Initialize()
        {
            foreach (FieldInfo fieldInfo in typeof(OpCodes).GetFields())
            {
                OpCode opCode = (OpCode)fieldInfo.GetValue(null);

                s_OpCodes.Add(opCode.Value, opCode);
            }
        }
    }
}
