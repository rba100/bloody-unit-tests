namespace BloodyUnitTests.CodeGeneration
{
    static class TypeHandlerFactory
    {
        public static ITypeHandler Create()
        {
            var compositeHandler = new CompositeRecursiveTypeHandler(new IRecursiveTypeHandler[]
            {
                // Specific handlers first
                new CSharpTypeKeywordNameHandler(),
                new SimpleDelegateTypeHandler(),
                new DateTimeTypeHandler(),
                new BuiltInTypesHandler(),
                new EnumTypeHandler(),
                new NumericTypeHandler(),
                new NullableTypeHandler(),
                new ListTypeHandler(),
                new ValueTupleTypeHandler(),
                new GuidTypeHandler(),

                // General handlers
                new GeneralDictionaryTypeHandler(),
                new ArrayTypeHandler(),
                new GenericTypeNameHandler(),

                // Fallback handlers
                new RhinoMockingTypeHandler(),
                new FallbackRecursiveTypeHandler()
            }, new NamespaceTracker());

            var nameRulesHandler = new InterfaceNameRuleHandler(compositeHandler);
            compositeHandler.SetRoot(nameRulesHandler); // Re-entry point

            // Post-recursion handlers.
            var lowerCamelCase = new CamelCasingIdentifierHandler(nameRulesHandler);
            var keywordAvoianceFilter = new CSharpKeyworkClashAvoidanceTypeHandler(lowerCamelCase);
            var cachingHandler = new CachingTypeHandler(keywordAvoianceFilter);

            return cachingHandler;
        }
    }
}
