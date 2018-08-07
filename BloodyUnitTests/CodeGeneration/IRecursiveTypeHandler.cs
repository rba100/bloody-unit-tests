namespace BloodyUnitTests.CodeGeneration
{
    interface IRecursiveTypeHandler : ITypeHandler
    {
        void SetRoot(ITypeHandler handler);
    }
}
