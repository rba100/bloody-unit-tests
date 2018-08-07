﻿
namespace BloodyUnitTests.CodeGeneration
{
    static class TypeHandlerFactory
    {
        public static ITypeHandler Create()
        {
            var compositeHandler = new CompositeRecursiveTypeHandler(new IRecursiveTypeHandler[]
            {
                // Specific handlers
                new CSharpKeywordTypeNameHandler(),
                new DateTimeTypeHandler(), 
                new NumericTypeHandler(),
                new NullableTypeHandler(),
                new StringAndCharTypeHandler(),
                new ListTypeHandler(), 

                // General handlers
                new ArrayTypeHandler(),
                new GenericTypeNameHandler(),

                // Fallback handlers
                new RhinoMockingTypeHandler(),
                new FallbackRecursiveTypeHandler(),
            });

            var nameRulesHandler = new TypeNameRulesTypeHandler(compositeHandler);

            var cachingHandler = new CachingTypeHandler(nameRulesHandler);

            compositeHandler.SetRoot(cachingHandler);

            return cachingHandler;
        }
    }
}
