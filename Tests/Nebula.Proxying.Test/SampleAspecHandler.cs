using System.Collections.Generic;
using Nebula.Extending;

[assembly: PluginAssembly]

namespace Nebula.Proxying.Test;

[AspectHandler(typeof(ProxyAttribute))]
public class SampleGeneratorPlugin : AspectHandler
{
    public static List<ProxyEntry> GeneratedProxies { get; } = new();

    public override void Handle(ProxyEntry proxy)
    {
        GeneratedProxies.Add(proxy);
    }
}