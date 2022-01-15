using System.Reflection;
using Nebula.Proxying;

namespace Nebula.Prism;

public static partial class DynamicGenerator
{
    private static readonly Type MethodBaseType = typeof(MethodBase);
    private static readonly MethodInfo MethodInfoGetter =
        MethodBaseType.GetMethod(nameof(MethodBase.GetMethodFromHandle), new[] { typeof(RuntimeMethodHandle) })!;
    private static readonly Type PropertyInfoType = typeof(PropertyInfo);
    
    private static readonly Type ExtensibleMethodType = typeof(DecoratedMethod);
    private static readonly ConstructorInfo ExtensibleMethodConstructor =
        ExtensibleMethodType.GetConstructor(new[] { typeof(object), typeof(MethodInfo) })!;
    private static readonly MethodInfo ExtensibleInvoker =
        ExtensibleMethodType.GetMethod(nameof(DecoratedMethod.Invoke))!;
    
    private static readonly Type ExtensiblePropertyType = typeof(DecoratedProperty);
    private static readonly ConstructorInfo ExtensiblePropertyConstructor =
        ExtensiblePropertyType.GetConstructor(new[] { typeof(object), typeof(PropertyInfo) })!;
    private static readonly MethodInfo ExtensibleGetter =
        ExtensiblePropertyType.GetMethod(nameof(DecoratedProperty.Get))!;
    private static readonly MethodInfo ExtensibleSetter =
        ExtensiblePropertyType.GetMethod(nameof(DecoratedProperty.Set))!;

    private static readonly Type GenerationAttributeType = typeof(GeneratedByPrismAttribute);
    private static readonly ConstructorInfo GenerationAttributeConstructor =
        GenerationAttributeType.GetConstructor(Array.Empty<Type>())!;
    private static readonly ConstructorInfo GenerationAttributeDataConstructor =
        GenerationAttributeType.GetConstructor(new[] { typeof(object) })!;
    
    private static readonly Type ProxyManagerType = typeof(ProxyManager);
    private static readonly ConstructorInfo ProxyManagerConstructor =
        ProxyManagerType.GetConstructor(Array.Empty<Type>())!;
    private static readonly MethodInfo ProxyManagerMethodProxyAdder =
        ProxyManagerType.GetMethod(nameof(ProxyManager.AddMethodProxy))!;
    private static readonly MethodInfo ProxyManagerPropertyProxyAdder =
        ProxyManagerType.GetMethod(nameof(ProxyManager.AddPropertyProxy))!;
    

    

    

    
}