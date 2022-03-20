﻿using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using Nebula.Exceptions;

namespace Nebula.Proxying;

/// <summary>
/// Proxy generator can generate proxy class for classes.
/// </summary>
public partial class ProxyGenerator
{
    /// <summary>
    /// Cache of all generated types.
    /// </summary>
    private readonly ConcurrentDictionary<Type, Type> _generatedTypes = new();

    /// <summary>
    /// Dynamic module for proxy types.
    /// </summary>
    private readonly ModuleBuilder _module;

    public ProxyGenerator(AssemblyName? assemblyName = null, string? moduleName = null)
    {
        assemblyName ??= new AssemblyName(Assembly.GetExecutingAssembly().GetName().Name + ".Proxies");
        moduleName ??= "Proxies";
        _module = AssemblyBuilder
            .DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndCollect)
            .DefineDynamicModule(moduleName);
    }

    /// <summary>
    /// Acquire the proxy class of the given type.
    /// </summary>
    /// <param name="proxiedType">Type to get proxy with.</param>
    /// <returns>Proxy class type.</returns>
    /// <exception cref="UserError">Thrown if the proxiedType is value type.</exception>
    public virtual Type GetProxy(Type proxiedType)
    {
        if (proxiedType.IsValueType)
            ErrorCenter.ReportFatal<UserError>(
                $"Can not create proxy type for value type {proxiedType.Name}.");
        return _generatedTypes.GetOrAdd(proxiedType, GenerateProxyClass);
    }

    /// <summary>
    /// Initialize a proxy entry.
    /// All proxied object will invoke this method for every proxy entry.
    /// </summary>
    /// <param name="proxy">Proxy to initialize.</param>
    public static void InitializeProxy(Proxy proxy)
    {
        ApplyAspectHandlers(proxy);
    }
}