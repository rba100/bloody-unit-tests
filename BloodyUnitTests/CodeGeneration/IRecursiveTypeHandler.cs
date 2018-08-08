using System.Linq;
using System.Security.Permissions;

namespace BloodyUnitTests.CodeGeneration
{
    interface IRecursiveTypeHandler : ITypeHandler
    {
        void SetRoot(ITypeHandler handler);
    }
}
