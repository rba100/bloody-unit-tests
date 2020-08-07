namespace BloodyUnitTests.Engine.CodeGeneration
{
    public interface INamespaceTracker
    {
        void RecordNamespace(string namespc);
        string[] GetNamespaces();
    }
}