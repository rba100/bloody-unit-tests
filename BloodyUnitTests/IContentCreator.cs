using System;

namespace BloodyUnitTests
{
    public interface IContentCreator
    {
        ClassContent Create(Type type);
    }
}