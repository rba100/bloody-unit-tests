using System;

namespace BloodyUnitTests.CodeGeneration
{
    class DateTimeTypeHandler : IRecursiveTypeHandler
    {
        private bool CanHandle(Type type)
        {
            return type == typeof(DateTime);
        }

        public bool CanGetInstantiation(Type type)
        {
            return CanHandle(type);
        }

        public string GetInstantiation(Type type, bool interestingValue)
        {
            return interestingValue 
                ? "DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc)"
                : "DateTime.UtcNow";
        }

        public bool IsInstantiationTerse(Type type)
        {
            return true;
        }

        public bool CanGetNameForIdentifier(Type type)
        {
            return CanHandle(type);
        }

        public string GetNameForIdentifier(Type type)
        {
            // We make assumptions about the tests that the user can correct 
            // if we're wrong.
            return "DateTimeUtc";
        }

        public bool CanGetNameForCSharp(Type type)
        {
            return CanHandle(type);
        }

        public string GetNameForCSharp(Type type)
        {
            return "DateTime";
        }

        public void SetRoot(ITypeHandler handler)
        {
            
        }
    }
}