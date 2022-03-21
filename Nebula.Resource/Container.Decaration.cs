namespace Nebula.Resource;

public partial class Container
{
    private class Entry
    {
        public Source Provider;

        public object? Cache;

        public Entry(Source provider, object? cache = null)
        {
            Provider = provider;
        }
    }
    private record Declaration(Type Category, IIdentifier Identifier, Scope Scope);
}