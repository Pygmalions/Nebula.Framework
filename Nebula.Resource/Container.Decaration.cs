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
            Cache = cache;
        }
    }
    private record Declaration(Type Category, IIdentifier Identifier, Scope Scope);
}