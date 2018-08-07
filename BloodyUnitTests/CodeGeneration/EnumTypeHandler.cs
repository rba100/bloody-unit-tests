using System;
using System.Linq;

namespace BloodyUnitTests.CodeGeneration
{
    class EnumTypeHandler : IRecursiveTypeHandler
    {
        public bool CanGetInstantiation(Type type)
        {
            return type.IsEnum;
        }

        public string GetInstantiation(Type type, bool interestingValue)
        {
            var names = Enum.GetNames(type);
            if(!names.Any()) return $"({type.Name}) 0";
            return interestingValue && names.Length > 1
                ? $"{type.Name}.{names[1]}"
                : $"{type.Name}.{names[0]}";
        }

        public bool IsInstantiationTerse(Type type)
        {
            return true;
        }

        public bool CanGetNameForIdentifier(Type type)
        {
            return false; // fallback is fine.
        }

        public string GetNameForIdentifier(Type type)
        {
            throw new NotImplementedException();
        }

        public bool CanGetNameForCSharp(Type type)
        {
            return false; // fallback is fine.
        }

        public string GetNameForCSharp(Type type)
        {
            throw new NotImplementedException();
        }

        public void SetRoot(ITypeHandler handler)
        {

        }
    }
}