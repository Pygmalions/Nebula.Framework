using System.Reflection;
using System.Reflection.Emit;
using Nebula.Reporting;

namespace Nebula.Proxying;

public partial class ProxyGenerator
{
    private static readonly CustomAttributeBuilder GeneratedAttributeBuilder = new(
        typeof(GeneratedAttribute).GetConstructor(Array.Empty<Type>())!,
        Array.Empty<object>());

    /// <summary>
    /// Generate a proxy class for the given base class.
    /// </summary>
    /// <param name="baseClass">Class to generate proxy class.</param>
    /// <returns>Proxy class type.</returns>
    /// <exception cref="ReportExceptionWrapper">
    /// Throw if failed to generate proxy for the given class.
    /// </exception>
    private Type GenerateProxyClass(Type baseClass)
    {
        var classBuilder = _module.DefineType(baseClass.Name + "Proxy",
            TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.AnsiClass | TypeAttributes.AutoClass |
            TypeAttributes.BeforeFieldInit);
        classBuilder.SetParent(baseClass);
        classBuilder.SetCustomAttribute(GeneratedAttributeBuilder);

        var proxyManager = classBuilder.DefineField("_Nebula_Proxies",
            Meta.ProxyManagerMeta.Class, FieldAttributes.Private);

        ImplementInterfaces(classBuilder, proxyManager);

        var proxies = new List<(MethodInfo, (MethodBuilder, FieldBuilder), PropertyInfo?)>();

        ulong proxyId = 0;

        foreach (var baseMethod in baseClass.GetMethods(
                     BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (baseMethod.IsAbstract || !baseMethod.IsVirtual) continue;
            var proxyAttribute = baseMethod.GetCustomAttribute<ProxyAttribute>();
            if (proxyAttribute == null) continue;
            proxies.Add((baseMethod, RefractMethod(classBuilder, baseMethod, proxyId), null));
            proxyId++;
        }

        foreach (var baseProperty in baseClass.GetProperties(
                     BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            var proxyAttribute = baseProperty.GetCustomAttribute<ProxyAttribute>();
            if (proxyAttribute == null) continue;
            var baseGetter = baseProperty.GetGetMethod();
            if (baseProperty.CanRead && baseGetter != null)
            {
                proxies.Add((baseGetter, RefractMethod(classBuilder, baseGetter, proxyId), baseProperty));
                proxyId++;
            }

            var baseSetter = baseProperty.GetSetMethod();
            if (baseProperty.CanWrite && baseSetter != null)
            {
                proxies.Add((baseSetter, RefractMethod(classBuilder, baseSetter, proxyId), baseProperty));
                proxyId++;
            }
        }

        proxyId = 0;

        var initializer = GenerateInitializerMethod(classBuilder, proxyManager, proxies);

        foreach (var baseConstructor in baseClass.GetConstructors(
                     BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            RefractConstructor(classBuilder, baseConstructor, initializer);
            proxyId++;
        }

        Type? proxyClass = null;
        try
        {
            proxyClass = classBuilder.CreateType();
        }
        catch (Exception innerException)
        {
            throw Report.Error("Failed to Create Proxy Class", innerException.Message,
                    this)
                .AttachDetails("ProxiedType", baseClass)
                .AsException(innerException);
        }
        if (proxyClass == null)
            throw Report.Error("Failed to Create Proxy Class", "Failed to generate proxy class.",
                    this)
                .AttachDetails("ProxiedType", baseClass)
                .AsException();
        return proxyClass;
    }

    /// <summary>
    /// Generate a initializer method which will
    /// </summary>
    /// <param name="classBuilder"></param>
    /// <param name="proxyManager"></param>
    /// <param name="proxies"></param>
    /// <returns></returns>
    private static MethodBuilder GenerateInitializerMethod(TypeBuilder classBuilder, FieldInfo proxyManager,
        List<(MethodInfo, (MethodBuilder, FieldBuilder), PropertyInfo?)> proxies)
    {
        var method = classBuilder.DefineMethod("_Nebula_Initialize",
            MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
            MethodAttributes.SpecialName,
            CallingConventions.Standard,
            null, Type.EmptyTypes);
        method.SetCustomAttribute(GeneratedAttributeBuilder);
        var code = method.GetILGenerator();

        // Initialize proxy manager.
        code.Emit(OpCodes.Ldarg_0);
        code.Emit(OpCodes.Newobj, Meta.ProxyManagerMeta.Constructors.Default);
        code.Emit(OpCodes.Stfld, proxyManager);

        var variableMethodHandle = code.DeclareLocal(Meta.MethodBaseMeta.Class);
        var variableInvoker = code.DeclareLocal(Meta.ActionContextMeta.Class);
        var variableProxy = code.DeclareLocal(Meta.ProxyMeta.Class);

        var variableHolderType = code.DeclareLocal(Meta.TypeMeta.Class);
        var variablePropertyInfo = code.DeclareLocal(Meta.PropertyInfoMeta.Class);

        code.Emit(OpCodes.Ldarg_0);
        code.Emit(OpCodes.Call,
            Meta.ObjectMeta.Methods.GetTypeHandle);
        code.Emit(OpCodes.Stloc, variableHolderType);

        foreach (var (baseMethod, (invokerMethod, proxyField), propertyInfo) in proxies)
        {
            // Acquire method reflection handle.
            code.Emit(OpCodes.Ldtoken, baseMethod);
            code.Emit(OpCodes.Call, Meta.MethodBaseMeta.Methods.GetMethodFromHandle);
            code.Emit(OpCodes.Stloc, variableMethodHandle);

            code.Emit(OpCodes.Ldarg_0);
            code.Emit(OpCodes.Ldloc, variableMethodHandle);

            // If the proxied method is a property accessor, then get the property reflection information.
            if (propertyInfo != null)
            {
                code.Emit(OpCodes.Ldloc, variableHolderType);
                code.Emit(OpCodes.Ldstr, propertyInfo.Name);
                code.Emit(OpCodes.Callvirt,
                    Meta.TypeMeta.Methods.GetProperty);
            }
            else
            {
                code.Emit(OpCodes.Ldnull);
            }

            code.Emit(OpCodes.Newobj,
                Meta.ProxyMeta.Constructors.Default);
            code.Emit(OpCodes.Stloc, variableProxy);

            // Construct proxy field.
            code.Emit(OpCodes.Ldarg_0);
            code.Emit(OpCodes.Ldloc, variableProxy);
            code.Emit(OpCodes.Stfld, proxyField);

            // Create invoker delegate from the invoker method.
            code.Emit(OpCodes.Ldarg_0);
            code.Emit(OpCodes.Ldftn, invokerMethod);
            code.Emit(OpCodes.Newobj, Meta.ActionContextMeta.Constructors.Default);
            code.Emit(OpCodes.Stloc, variableInvoker);

            // Replace the invoker of the proxy.
            code.Emit(OpCodes.Ldloc, variableProxy);
            code.Emit(OpCodes.Ldloc, variableInvoker);
            code.Emit(OpCodes.Callvirt, Meta.ProxyMeta.Properties.Invoker.Set);

            // Add proxy to the proxy manager.
            code.Emit(OpCodes.Ldarg_0);
            code.Emit(OpCodes.Ldfld, proxyManager);
            code.Emit(OpCodes.Ldloc, variableMethodHandle);
            code.Emit(OpCodes.Ldarg_0);
            code.Emit(OpCodes.Ldfld, proxyField);
            code.Emit(OpCodes.Callvirt, Meta.ProxyManagerMeta.Methods.AddProxy);

            // Initialize proxy with the generator InitializeProxy(...) method.
            code.Emit(OpCodes.Ldarg_0);
            code.Emit(OpCodes.Ldfld, proxyField);
            code.Emit(OpCodes.Call, Meta.ProxyGeneratorMeta.Methods.ProxyInitialize);
        }

        code.Emit(OpCodes.Ret);

        return method;
    }

    private static void RefractConstructor(
        TypeBuilder classBuilder, ConstructorInfo baseConstructor, MethodInfo initializer)
    {
        var parameters = baseConstructor.GetParameters();
        var constructorFlags = MethodAttributes.HideBySig |
                               MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
        if (baseConstructor.IsPublic)
            constructorFlags |= MethodAttributes.Public;
        else constructorFlags |= MethodAttributes.Private;

        var code = classBuilder.DefineConstructor(
                constructorFlags, CallingConventions.Standard,
                parameters.Select(info => info.ParameterType).ToArray())
            .GetILGenerator();
        for (var argumentIndex = 0; argumentIndex <= parameters.Length; ++argumentIndex)
            // Load constructor arguments to the stack.
            code.Emit(OpCodes.Ldarg, argumentIndex);
        // Call base constructor.
        code.Emit(OpCodes.Call, baseConstructor);

        // Invoke the initializer method.
        code.Emit(OpCodes.Ldarg_0);
        code.Emit(OpCodes.Call, initializer);

        code.Emit(OpCodes.Ret);
    }

    private static (MethodBuilder invoker, FieldBuilder proxy) RefractMethod(
        TypeBuilder classBuilder, MethodInfo baseMethod, ulong proxyId)
    {
        var proxyField = classBuilder.DefineField(
            $"_Nebula_Proxy_{proxyId}_{baseMethod.Name}", Meta.ProxyMeta.Class,
            FieldAttributes.Private);
        proxyField.SetCustomAttribute(GeneratedAttributeBuilder);

        var methodParameters = baseMethod.GetParameters();

        var proxyMethod = classBuilder.DefineMethod($"_Nebula_{proxyId}_{baseMethod.Name}",
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
            MethodAttributes.SpecialName | MethodAttributes.Virtual,
            CallingConventions.Standard, baseMethod.ReturnType,
            methodParameters.Select(parameter => parameter.ParameterType).ToArray()
        );
        proxyMethod.SetCustomAttribute(GeneratedAttributeBuilder);
        classBuilder.DefineMethodOverride(proxyMethod, baseMethod);

        var proxyCode = proxyMethod.GetILGenerator();

        // Generate arguments array.
        proxyCode.Emit(OpCodes.Ldc_I4, methodParameters.Length);
        proxyCode.Emit(OpCodes.Newarr, typeof(object));
        for (var parameterIndex = 0; parameterIndex < methodParameters.Length; ++parameterIndex)
        {
            proxyCode.Emit(OpCodes.Dup);
            proxyCode.Emit(OpCodes.Ldc_I4, parameterIndex);
            // Exclude the parameter 0, which is the pointer "this".
            proxyCode.Emit(OpCodes.Ldarg, parameterIndex + 1);
            if (methodParameters[parameterIndex].ParameterType.IsValueType)
                proxyCode.Emit(OpCodes.Box, methodParameters[parameterIndex].ParameterType);
            proxyCode.Emit(OpCodes.Stelem_Ref);
        }

        var arguments = proxyCode.DeclareLocal(Meta.ObjectArrayMeta.Class);
        proxyCode.Emit(OpCodes.Stloc, arguments);

        // Call the Invoke(...) method of the corresponding proxy.
        proxyCode.Emit(OpCodes.Ldarg_0);
        proxyCode.Emit(OpCodes.Ldfld, proxyField);
        proxyCode.Emit(OpCodes.Ldloc, arguments);
        proxyCode.Emit(OpCodes.Callvirt, Meta.ProxyMeta.Methods.Invoke);

        if (baseMethod.ReturnType != typeof(void))
        {
            if (baseMethod.ReturnType.IsValueType)
                proxyCode.Emit(OpCodes.Unbox_Any, baseMethod.ReturnType);
        }
        else
        {
            proxyCode.Emit(OpCodes.Pop);
        }

        proxyCode.Emit(OpCodes.Ret);

        var invokerMethod = classBuilder.DefineMethod($"_Nebula_Invoker_{proxyId}_{baseMethod.Name}",
            MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
            MethodAttributes.SpecialName,
            CallingConventions.Standard, null,
            new[] { Meta.ContextMeta.Class }
        );
        var invokerCode = invokerMethod.GetILGenerator();

        var variableProxiedObject = invokerCode.DeclareLocal(typeof(object));
        var variableProxiedMethod = invokerCode.DeclareLocal(typeof(MethodInfo));
        var variableArguments = invokerCode.DeclareLocal(Meta.ObjectArrayMeta.Class);

        invokerCode.Emit(OpCodes.Ldarg_1);
        invokerCode.Emit(OpCodes.Call, Meta.ContextMeta.Properties.ProxiedObject.Get);
        invokerCode.Emit(OpCodes.Stloc, variableProxiedObject);
        invokerCode.Emit(OpCodes.Ldarg_1);
        invokerCode.Emit(OpCodes.Call, Meta.ContextMeta.Properties.ProxiedMethod.Get);
        invokerCode.Emit(OpCodes.Stloc, variableProxiedMethod);
        invokerCode.Emit(OpCodes.Ldarg_1);
        invokerCode.Emit(OpCodes.Call, Meta.ContextMeta.Properties.Arguments.Get);
        invokerCode.Emit(OpCodes.Stloc, variableArguments);

        // Load arguments from arguments array to the caller stack.
        invokerCode.Emit(OpCodes.Ldarg_0);

        for (var parameterIndex = 0; parameterIndex < methodParameters.Length; ++parameterIndex)
        {
            invokerCode.Emit(OpCodes.Ldloc, variableArguments);
            invokerCode.Emit(OpCodes.Ldc_I4, parameterIndex);
            invokerCode.Emit(OpCodes.Ldelem_Ref);
            if (methodParameters[parameterIndex].ParameterType.IsValueType)
                invokerCode.Emit(OpCodes.Unbox_Any, methodParameters[parameterIndex].ParameterType);
        }

        // Invoke the base method without looking up in the virtual function table.
        invokerCode.Emit(OpCodes.Call, baseMethod);

        if (baseMethod.ReturnType != typeof(void))
        {
            if (baseMethod.ReturnType.IsValueType)
                invokerCode.Emit(OpCodes.Box, baseMethod.ReturnType);
            var variableReturning = invokerCode.DeclareLocal(Meta.ObjectMeta.Class);
            invokerCode.Emit(OpCodes.Stloc, variableReturning);
            // Set the returning value of the context.
            invokerCode.Emit(OpCodes.Ldarg_1);
            invokerCode.Emit(OpCodes.Ldloc, variableReturning);
            invokerCode.Emit(OpCodes.Callvirt, Meta.ContextMeta.Methods.Return);
        }

        invokerCode.Emit(OpCodes.Ret);

        return (invokerMethod, proxyField);
    }

    private static void ImplementInterfaces(TypeBuilder classBuilder, FieldInfo proxyManager)
    {
        classBuilder.AddInterfaceImplementation(typeof(IProxyObject));

        var method = classBuilder.DefineMethod("_Nebula_GetProxy",
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
            MethodAttributes.SpecialName | MethodAttributes.Virtual,
            CallingConventions.Standard,
            Meta.ProxyMeta.Class, new[] { typeof(MethodInfo) });
        classBuilder.DefineMethodOverride(method,
            typeof(IProxyObject).GetMethod(nameof(IProxyObject.GetProxy))!);
        var code = method.GetILGenerator();

        code.Emit(OpCodes.Ldarg_0);
        code.Emit(OpCodes.Ldfld, proxyManager);
        code.Emit(OpCodes.Ldarg_1);
        code.Emit(OpCodes.Call,
            Meta.ProxyManagerMeta.Methods.GetProxy);

        code.Emit(OpCodes.Ret);
    }
}