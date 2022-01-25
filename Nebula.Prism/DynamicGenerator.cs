using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using Nebula.Proxying;

namespace Nebula.Prism;

public partial class DynamicGenerator
{
    private readonly ModuleBuilder _module;
    
    private readonly ConcurrentDictionary<Type, Type> _refractions = new();

    /// <summary>
    /// Create a dynamic generator with a dynamic assembly and module.
    /// </summary>
    /// <param name="assemblyName">Name of the new dynamic assembly.</param>
    /// <param name="moduleName">Name of the new dynamic module.</param>
    public DynamicGenerator(string assemblyName, string moduleName)
    {
        _module = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run)
            .DefineDynamicModule(moduleName);
    }

    private Type CreateRefractionClass(Type baseClass)
    {
        var classBuilder = _module.DefineType(baseClass.Name + "_Refraction",
            TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AnsiClass | TypeAttributes.AutoClass |
            TypeAttributes.BeforeFieldInit);
        classBuilder.SetParent(baseClass);
        classBuilder.AddInterfaceImplementation(typeof(IProxiedObject));
        var attributeBuilder = new CustomAttributeBuilder(
            GenerationAttributeMeta.Constructors.Default, Array.Empty<object>());
        classBuilder.SetCustomAttribute(attributeBuilder);
        
        // Define proxies manager.
        var managerField = classBuilder.DefineField("_prism_Proxies", typeof(ProxyManager), FieldAttributes.Private);
        
        // Refract methods.
        var proxyId = 0;
        var methodProxies = new List<(FieldBuilder, MethodBuilder, MethodInfo)>();
        foreach (var method in baseClass.GetMethods())
        {
            if (method.IsAbstract) continue;
            if (!method.IsVirtual) continue;
            var attribute = method.GetCustomAttribute<RefractionAttribute>();
            if (attribute == null) continue;
            var (proxyField, proxyMethod) = RefractMethod(classBuilder, method, proxyId++);
            methodProxies.Add((proxyField, proxyMethod, method));
        }
        
        var propertyProxies = new List<(FieldBuilder, MethodBuilder? setter, MethodBuilder? getter, PropertyInfo)>();
        foreach (var property in baseClass.GetProperties())
        {
            var attribute = property.GetCustomAttribute<RefractionAttribute>();
            if (attribute == null) continue;
            var (proxyField, setter, getter) = 
                RefractProperty(classBuilder, property, proxyId++);
            propertyProxies.Add((proxyField, setter, getter, property));
        }
        
        // Refract constructors.
        foreach (var baseConstructor in baseClass.GetConstructors())
        {
            RefractConstructor(classBuilder, managerField, baseConstructor, methodProxies, propertyProxies);
        }
        
        ImplementProxyInterface(classBuilder, managerField);

        return classBuilder.CreateType()!;
    }

    public Type GetRefraction(Type baseClass)
    {
        if (_refractions.TryGetValue(baseClass, out var refraction))
            return refraction;
        refraction = CreateRefractionClass(baseClass);
        _refractions.TryAdd(baseClass, refraction);
        
        return refraction;
    }

    public Type GetRefraction<TClass>()
        => GetRefraction(typeof(TClass));
}