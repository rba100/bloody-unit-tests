namespace BloodyUnitTests.CodeGeneration
{
    interface IRecursiveTypeHandler : ITypeHandler
    {
        void SetRoot(IRecursiveTypeHandler handler);
    }
}
