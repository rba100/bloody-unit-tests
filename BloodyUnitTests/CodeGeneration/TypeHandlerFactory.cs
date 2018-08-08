
namespace BloodyUnitTests.CodeGeneration
{
    static class TypeHandlerFactory
    {
        public static ITypeHandler Create()
        {
            var compositeHandler = new CompositeRecursiveTypeHandler(new IRecursiveTypeHandler[]
            {
                // Specific handlers
                new CSharpTypeKeywordNameHandler(),
                new SimpleDelegateTypeHandler(),
                new DateTimeTypeHandler(),
                new BuiltInTypesHandler(),
                new EnumTypeHandler(),
                new NumericTypeHandler(),
                new NullableTypeHandler(),
                new StringAndCharTypeHandler(),
                new ListTypeHandler(),
                new ValueTupleTypeHandler(),

                // General handlers
                new GeneralDictionaryTypeHandler(),
                new ArrayTypeHandler(),
                new GenericTypeNameHandler(),

                // Fallback handlers
                new RhinoMockingTypeHandler(),
                new FallbackRecursiveTypeHandler(),
            });

            var nameRulesHandler = new InterfaceNameRuleHandler(compositeHandler);
            var cachingHandler = new CachingTypeHandler(nameRulesHandler);
            compositeHandler.SetRoot(cachingHandler); // Re-entry point

            // Post-recursion handlers.
            var lowerCamelCase = new CamelCasingIdentifierHandler(cachingHandler);
            var keywordAvoianceFilter = new CSharpKeyworkClashAvoidanceTypeHandler(lowerCamelCase);

            return keywordAvoianceFilter;
        }
    }
}
