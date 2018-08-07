
namespace BloodyUnitTests.CodeGeneration
{
    static class TypeHandlerFactory
    {
        public static ITypeHandler Create()
        {
            return new CompositeRecursiveTypeHandler(new IRecursiveTypeHandler[]
            {
                new GenericTypeNameHandler(), 
                new NumericTypeHandler(),
                new RhinoMockingTypeHandler(),
                new FallbackRecursiveTypeHandler(), 
            });
        }
    }
}
