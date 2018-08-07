using System;

namespace BloodyUnitTests.CodeGeneration
{
    interface ITypeHandler
    {
        bool CanGetInstantiation(Type type);
        string GetInstantiation(Type type, bool interestingValue);
        bool IsInstantiationTerse(Type type);

        bool CanGetNameForIdentifier(Type type);
        string GetNameForIdentifier(Type type, VarScope scope);

        bool CanGetNameForCSharp(Type type);
        string GetNameForCSharp(Type type);
    }
}