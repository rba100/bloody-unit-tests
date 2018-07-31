using System;
using System.Collections.Generic;

namespace BloodyUnitTests
{
    internal static class ExtensionMethods
    {
        public static TOut Into<TOut, TIn>(this IEnumerable<TIn> enumerable, Func<IEnumerable<TIn>, TOut> intoFunction)
        {
            return intoFunction(enumerable);
        }
    }
}
