using System;
using BloodyUnitTests.ContentCreators;

namespace BloodyUnitTests
{
    public interface IContentCreator
    {
        ClassContent Create(Type type);
    }
}