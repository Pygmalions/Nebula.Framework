using System.Reflection;

namespace Nebula.Proxying;

/// <summary>
/// Interface for objects which holds and provides proxy for query.
/// </summary>
public interface IProxiedObject
{
    /// <summary>
    /// Acquire the proxy of the corresponding
    /// </summary>
    /// <param name="proxiedMethod"></param>
    /// <returns></returns>
    Proxy? GetProxy(MethodInfo proxiedMethod);
}