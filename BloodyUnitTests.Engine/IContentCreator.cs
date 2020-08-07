using System;
using BloodyUnitTests.Engine.ContentCreators;

namespace BloodyUnitTests.Engine
{
    /// <summary>
    /// Defines a code generation component.
    /// </summary>
    public interface IContentCreator
    {
        /// <summary>
        /// Creates the content
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        ClassContent Create(Type type);
    }
}