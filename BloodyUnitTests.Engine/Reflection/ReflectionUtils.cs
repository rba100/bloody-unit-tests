using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace BloodyUnitTests.Engine.Reflection
{
    public static class ReflectionUtils
    {
        public static MethodInfo[] GetCalledMethods(MethodInfo methodInfo)
        {
            return InstructionLoader.GetInstructions(methodInfo)
                                    .Where(i => i.OpCode == OpCodes.Callvirt)
                                    .Select(i=>i.Data)
                                    .OfType<MethodInfo>()
                                    .ToArray();
        }

        public static (ConstructorInfo ctor, MethodDelegationReport[] delegations)[]
            GetMethodsThatPassthrough(Type classType)
        {
            var ctors = classType.GetConstructors();
            return ctors
                   .Select(ctor=> (ctor, GetMethodsThatPassthrough(classType, ctor)))
                   .ToArray();
        }

        public static MethodDelegationReport[] GetMethodsThatPassthrough(Type classType, ConstructorInfo ctor)
        {
            return ctor.GetParameters()
                       .Select(p => p.ParameterType)
                       .Where(type => type.IsInterface)
                       .Distinct()
                       .SelectMany(type => GetMethodsThatDelegate(classType, type))
                       .ToArray();
        }


        private static MethodDelegationReport[] GetMethodsThatDelegate(Type classType, 
                                                                       Type delegateInterfaceType)
        {
            var returnList = new List<MethodDelegationReport>();
            var delegateMethods = delegateInterfaceType.GetMethods();
            var classPublicMethods = classType.GetMethods();
            foreach (var methodInfo in classPublicMethods)
            {
                var delegatedMethod = GetSingleDelegateOrNull(methodInfo, delegateMethods);
                if (delegatedMethod != null)
                {
                    returnList.Add(new MethodDelegationReport(methodInfo,
                                                              delegatedMethod, 
                                                              delegateInterfaceType));
                }
            }

            return returnList.ToArray();
        }

        private static MethodInfo GetSingleDelegateOrNull(MethodInfo info, MethodInfo[] delegateMethods)
        {
            var delegated = delegateMethods.Where(m => MethodDelegatesTo(info, m)).ToArray();
            if (delegated.Length == 1) return delegated.Single();
            return null;
        }

        private static bool MethodDelegatesTo(MethodInfo caller, MethodInfo callee)
        {
            var ins = InstructionLoader.GetInstructions(caller).ToArray();

            foreach (var instruction in ins)
            {
                var methodCall = instruction.Data as MethodBase;
                if (instruction.OpCode == OpCodes.Callvirt
                    && methodCall != null)
                {
                    var methodCalled = methodCall == callee;
                    if (methodCalled) return true;
                }
            }

            return false;
        }

        private static bool MethodDelegatesToExactly(MethodInfo caller, MethodInfo callee)
        {
            var ins = InstructionLoader.GetInstructions(caller).ToArray();
            int loadArgCount = 0;

            var loadArgTypes = new[] { OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3, OpCodes.Ldarg };
            foreach (var instruction in ins)
            {
                var methodCall = instruction.Data as MethodBase;
                if (instruction.OpCode == OpCodes.Callvirt
                    && methodCall != null)
                {
                    var methodCalled = methodCall == callee;
                    var loadedWithArg = methodCall.GetParameters().Length == loadArgCount;
                    if (methodCalled && loadedWithArg) return true;
                }
                else
                {
                    if (loadArgTypes.Contains(instruction.OpCode)) loadArgCount++;
                    else loadArgCount = 0;
                }
            }

            return false;
        }
    }

    public class MethodDelegationReport
    {
        public MethodInfo Caller { get; }
        public MethodInfo InnerMethod { get; }

        public Type InnerMethodInterfaceType { get; }

        public MethodDelegationReport(MethodInfo caller,
                                      MethodInfo innerMethod,
                                      Type innerMethodInterfaceType)
        {
            Caller = caller ?? throw new ArgumentNullException(nameof(caller));
            InnerMethod = innerMethod ?? throw new ArgumentNullException(nameof(innerMethod));
            InnerMethodInterfaceType = innerMethodInterfaceType ?? throw new ArgumentNullException(nameof(innerMethodInterfaceType));
        }
    }
}
