using System;

namespace BloodyUnitTests.CodeGeneration
{
    interface ITypeHandler
    {
        bool CanGetInstantiation(Type type);

        /// <summary>
        /// Only valid when CanGetInstantiation
        /// </summary>
        string GetInstantiation(Type type, bool interestingValue);
        /// <summary>
        /// Only valid when CanGetInstantiation
        /// </summary>
        bool IsInstantiationTerse(Type type);

        bool CanGetNameForIdentifier(Type type);

        /// <summary>
        /// Only valid when CanGetNameForIdentifier
        /// </summary>
        string GetNameForIdentifier(Type type);

        bool CanGetNameForCSharp(Type type);

        /// <summary>
        /// Only valid when CanGetNameForCSharp
        /// </summary>
        string GetNameForCSharp(Type type);
    }
}