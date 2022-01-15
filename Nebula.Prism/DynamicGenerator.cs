using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using Nebula.Proxying;

namespace Nebula.Prism;

/// <summary>
/// This static tool class can 'emit' assembly contains the proxy class.
/// </summary>
public static partial class DynamicGenerator
{
    // Todo: change redirection-invoker mode to merged proxy mode.
    private static FieldBuilder DefineMethodProxyField(TypeBuilder classBuilder, int proxyId)
    {
        return classBuilder.DefineField(
            $"_prism_MethodProxy_{proxyId}", ExtensibleMethodType, 
            FieldAttributes.Private | FieldAttributes.InitOnly);
    }
    private static (MethodBuilder redirectionMethod, MethodBuilder invokerMethod)
        DefineRedirectionMethod(TypeBuilder classBuilder, int proxyId,
        MethodInfo baseMethod, FieldInfo proxyField)
    {
        var methodParameters = baseMethod.GetParameters();

        var attributeBuilder = new CustomAttributeBuilder(
            GenerationAttributeConstructor, Array.Empty<object>());
        var redirectionBuilder = classBuilder.DefineMethod($"_prism_Method_{proxyId}",
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
            MethodAttributes.SpecialName | MethodAttributes.Virtual,
            CallingConventions.Standard, baseMethod.ReturnType, 
            methodParameters.Select(parameter => parameter.ParameterType).ToArray()
        );
        var redirectionCode = redirectionBuilder.GetILGenerator();
        redirectionCode.Emit(OpCodes.Ldarg_0);
        redirectionCode.Emit(OpCodes.Ldfld, proxyField);
        
        redirectionCode.Emit(OpCodes.Ldc_I4, methodParameters.Length);
        redirectionCode.Emit(OpCodes.Newarr, typeof(object));

        for (var parameterIndex = 0; parameterIndex < methodParameters.Length; ++parameterIndex)
        {
            redirectionCode.Emit(OpCodes.Dup);
            redirectionCode.Emit(OpCodes.Ldc_I4, parameterIndex);
            redirectionCode.Emit(OpCodes.Ldarg, parameterIndex + 1);
            if (methodParameters[parameterIndex].ParameterType.IsValueType)
                redirectionCode.Emit(OpCodes.Box, methodParameters[parameterIndex].ParameterType);
            redirectionCode.Emit(OpCodes.Stelem_Ref);
        }
        redirectionCode.Emit(OpCodes.Callvirt, ExtensibleInvoker);
        if (baseMethod.ReturnType.IsValueType)
            redirectionCode.Emit(OpCodes.Unbox_Any, baseMethod.ReturnType);
        redirectionCode.Emit(OpCodes.Ret);
        classBuilder.DefineMethodOverride(redirectionBuilder, baseMethod);
        
        var invokerBuilder = classBuilder.DefineMethod($"_prism_MethodInvoker_{proxyId}",
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
            MethodAttributes.SpecialName | MethodAttributes.Virtual,
            CallingConventions.Standard, baseMethod.ReturnType, 
            methodParameters.Select(parameter => parameter.ParameterType).ToArray()
        );
        invokerBuilder.SetCustomAttribute(attributeBuilder);
        var invokerCode = invokerBuilder.GetILGenerator();
        for (var parameterIndex = 0; parameterIndex <= methodParameters.Length; ++parameterIndex)
        {
            invokerCode.Emit(OpCodes.Ldarg, parameterIndex);
        }
        invokerCode.Emit(OpCodes.Call, baseMethod);
        invokerCode.Emit(OpCodes.Ret);
        
        invokerBuilder.SetCustomAttribute(attributeBuilder);

        return (redirectionBuilder, invokerBuilder);
    }
    
    private static Type CreateRefraction(string assemblyName, string moduleName, Type baseClass)
    {
        var assemblyBuilder =
            AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleName);
        var classBuilder = moduleBuilder.DefineType(baseClass.Name + "Refraction",
            TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AnsiClass | TypeAttributes.AutoClass |
            TypeAttributes.BeforeFieldInit);
        classBuilder.SetParent(baseClass);
        classBuilder.AddInterfaceImplementation(typeof(IProxiedObject));

        // Define proxies manager.
        var managerField = classBuilder.DefineField("_prism_Proxies", typeof(ProxyManager), FieldAttributes.Private);

        var proxyId = 0;

        var methodProxies = new List<(FieldBuilder proxyField, MethodBuilder invokerMethod, MethodInfo baseMethod)>();
        
        #region Generate method proxy fields.
        foreach (var method in baseClass.GetMethods())
        {
            if (method.IsAbstract) continue;
            if (!method.IsVirtual) continue;
            var attribute = method.GetCustomAttribute<RefractionAttribute>();
            if (attribute == null) continue;

            var proxyField = DefineMethodProxyField(classBuilder, proxyId);
            var (redirection, invoker) = 
                DefineRedirectionMethod(classBuilder, proxyId, method, proxyField);
            methodProxies.Add((proxyField, invoker, method));
            ++proxyId;
        }
        #endregion
        
        // #region Generate property proxy fields.
        // foreach (var property in baseClass.GetProperties())
        // {
        //     var attribute = property.GetCustomAttribute<RefractionAttribute>();
        //     if (attribute == null) continue;
        //     var propertyProxyField = classBuilder.DefineField(
        //         $"_prism_PropertyProxy_{proxyId}", ExtensiblePropertyType, 
        //         FieldAttributes.Private | FieldAttributes.InitOnly);
        //
        //     #region Generate redirection property.
        //     var propertyBuilder = classBuilder.DefineProperty(
        //         $"_prism_PropertyProxy_{proxyId}", PropertyAttributes.None,
        //         property.PropertyType, null);
        //     #region Generate getter
        //     if (property.CanRead)
        //     {
        //         var getterBuilder = classBuilder.DefineMethod($"_prism_Getter_{proxyId}",
        //             MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
        //             MethodAttributes.SpecialName | MethodAttributes.Virtual,
        //             CallingConventions.Standard, property.PropertyType, null
        //         );
        //         
        //         var attributeBuilder = new CustomAttributeBuilder(
        //             GenerationAttributeDataConstruct,
        //             new[] { (object)proxyId });
        //         getterBuilder.SetCustomAttribute(attributeBuilder);
        //         var getterCode = getterBuilder.GetILGenerator();
        //         getterCode.Emit(OpCodes.Ldarg_0);
        //         getterCode.Emit(OpCodes.Ldfld, propertyProxyField);
        //         getterCode.Emit(OpCodes.Callvirt, ExtensibleGet);
        //         getterCode.Emit(OpCodes.Ret);
        //         propertyBuilder.SetGetMethod(getterBuilder);
        //         classBuilder.DefineMethodOverride(getterBuilder, 
        //             property.GetGetMethod(true)!);
        //     }
        //     #endregion
        //     
        //     #region Generate setter
        //     if (property.CanWrite)
        //     {
        //         var setterBuilder = classBuilder.DefineMethod($"_prism_Setter_{proxyId}",
        //             MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
        //             MethodAttributes.SpecialName | MethodAttributes.Virtual,
        //             CallingConventions.Standard, 
        //             null, new []{property.PropertyType}
        //         );
        //         var attributeBuilder = new CustomAttributeBuilder(
        //             GenerationAttributeDataConstruct,
        //             new[] { (object)proxyId });
        //         setterBuilder.SetCustomAttribute(attributeBuilder);
        //         var setterCode = setterBuilder.GetILGenerator();
        //         setterCode.Emit(OpCodes.Ldarg_0);
        //         setterCode.Emit(OpCodes.Ldfld, propertyProxyField);
        //         setterCode.Emit(OpCodes.Ldarg_1);
        //         setterCode.Emit(OpCodes.Callvirt, ExtensibleSet);
        //         setterCode.Emit(OpCodes.Ret);
        //         propertyBuilder.SetSetMethod(setterBuilder);
        //         classBuilder.DefineMethodOverride(setterBuilder, 
        //             property.GetSetMethod(true)!);
        //     }
        //     #endregion
        //     
        //     #endregion
        //     
        //     ++proxyId;
        // }
        // #endregion

        #region Define constructors.
        // Define constructors.
        foreach (var baseConstructor in baseClass.GetConstructors())
        {
            #region Call base constructor.
            var parameters = baseConstructor.GetParameters();
            var constructorCode = classBuilder.DefineConstructor(
                    MethodAttributes.Public | MethodAttributes.HideBySig | 
                    MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, 
                    CallingConventions.Standard, 
                    parameters.Select(info => info.ParameterType).ToArray())
                .GetILGenerator();
            for (var argumentIndex = 0; argumentIndex <= parameters.Length; ++argumentIndex)
            {
                // Load constructor arguments to the stack.
                constructorCode.Emit(OpCodes.Ldarg, argumentIndex);
            }
            // Call base constructor.
            constructorCode.Emit(OpCodes.Call, baseConstructor);
            #endregion
            
            #region Initialize proxies manager.
            constructorCode.Emit(OpCodes.Ldarg_0);
            constructorCode.Emit(OpCodes.Newobj, ProxyManagerConstructor);
            constructorCode.Emit(OpCodes.Stfld, managerField);
            #endregion
            
            #region Initialize proxies.
            foreach (var (proxyField, invokerMethod, baseMethod) in methodProxies)
            {
                constructorCode.Emit(OpCodes.Ldarg_0);
                constructorCode.Emit(OpCodes.Ldarg_0);
                constructorCode.Emit(OpCodes.Ldtoken, invokerMethod);
                constructorCode.Emit(OpCodes.Call, MethodInfoGetter);
                constructorCode.Emit(OpCodes.Newobj, ExtensibleMethodConstructor);
                constructorCode.Emit(OpCodes.Stfld, proxyField);
                
                constructorCode.Emit(OpCodes.Ldarg_0);
                constructorCode.Emit(OpCodes.Ldfld, managerField);
                constructorCode.Emit(OpCodes.Ldtoken, baseMethod);
                constructorCode.Emit(OpCodes.Call, MethodInfoGetter);
                constructorCode.Emit(OpCodes.Ldarg_0);
                constructorCode.Emit(OpCodes.Ldfld, proxyField);
                constructorCode.Emit(OpCodes.Callvirt, ProxyManagerMethodProxyAdder);
            }
            #endregion
            
            // Return.
            constructorCode.Emit(OpCodes.Ret);
        }
        #endregion
        
        
        #region Implement IProxiedObject::GetMethodProxy.
        var gettingMethod = classBuilder.DefineMethod("_prism_GetMethodProxy",
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
            MethodAttributes.SpecialName | MethodAttributes.Virtual,
            CallingConventions.Standard,
            typeof(IMethodProxy), new[] { typeof(MethodInfo) });
        var gettingMethodCode = gettingMethod.GetILGenerator();
        // Load this.
        gettingMethodCode.Emit(OpCodes.Ldarg_0);
        // Load manager.
        gettingMethodCode.Emit(OpCodes.Ldfld, managerField);
        // Load argument of MethodInfo.
        gettingMethodCode.Emit(OpCodes.Ldarg_1);
        gettingMethodCode.Emit(OpCodes.Callvirt, 
            typeof(ProxyManager).GetMethod(nameof(ProxyManager.GetMethodProxy))!);
        gettingMethodCode.Emit(OpCodes.Ret);
        classBuilder.DefineMethodOverride(gettingMethod, 
            typeof(IProxiedObject).GetMethod(nameof(IProxiedObject.GetMethodProxy))!);
        #endregion
        
        #region Implement IProxiedObject::GetPropertyProxy.
        var gettingProperty = classBuilder.DefineMethod("_prism_GetPropertyProxy",
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
            MethodAttributes.SpecialName | MethodAttributes.Virtual,
            CallingConventions.Standard,
            typeof(IPropertyProxy), new[] { typeof(PropertyInfo) });
        var gettingPropertyCode = gettingProperty.GetILGenerator();
        // Load this.
        gettingPropertyCode.Emit(OpCodes.Ldarg_0);
        // Load manager.
        gettingPropertyCode.Emit(OpCodes.Ldfld, managerField);
        // Load argument of MethodInfo.
        gettingPropertyCode.Emit(OpCodes.Ldarg_1);
        gettingPropertyCode.Emit(OpCodes.Callvirt, 
            typeof(ProxyManager).GetMethod(nameof(ProxyManager.GetPropertyProxy))!);
        gettingPropertyCode.Emit(OpCodes.Ret);
        classBuilder.DefineMethodOverride(gettingProperty, 
            typeof(IProxiedObject).GetMethod(nameof(IProxiedObject.GetPropertyProxy))!);
        #endregion

        return classBuilder.CreateType()!;
    }

    private static readonly ConcurrentDictionary<Type, Type> GeneratedRefractions = new();

    public static Type GetRefraction<TClass>() where TClass : class
    {
        return GetRefraction(typeof(TClass));
    }
    
    public static Type GetRefraction(Type classType)
    {
        if (GeneratedRefractions.TryGetValue(classType, out var refraction))
            return refraction;
        var name = Assembly.GetCallingAssembly().GetName().Name;
        refraction = CreateRefraction(name, name, classType)!;
        GeneratedRefractions[classType] = refraction;
        return refraction;
    }
}