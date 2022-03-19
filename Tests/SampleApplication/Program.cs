using Nebula.Core;
using Nebula.Resource.Identifiers;

namespace SampleApplication;

public static class SampleApplication
{
    public static void Main(string[] arguments)
    {
        Domain.Current.Launch();
        Domain.Current.Acquire<object>(WildcardIdentifier.Anything);
    }
}