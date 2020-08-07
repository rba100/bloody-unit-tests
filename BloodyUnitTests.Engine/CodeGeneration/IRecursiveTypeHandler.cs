namespace BloodyUnitTests.Engine.CodeGeneration
{
    interface IRecursiveTypeHandler : ITypeHandler
    {
        void SetRoot(ITypeHandler handler);
    }
}
