﻿using System.Reflection;
using System.Reflection.Emit;
using Nebula.Proxying;

namespace Nebula.Prism;

public partial class DynamicGenerator
{
    private (FieldBuilder, MethodBuilder? setter, MethodBuilder? getter) RefractProperty(
        TypeBuilder classBuilder, PropertyInfo property, int proxyId)
    {
        var proxyField = classBuilder.DefineField(
            $"_prism_{proxyId}_{property.Name}_Proxy", MethodProxyEntryMeta.ClassType, 
            FieldAttributes.Private | FieldAttributes.InitOnly);
        var setter = property.CanWrite ? 
            RefractPropertySetter(classBuilder, proxyField, property, proxyId) : null;
        var getter = property.CanRead ? 
            RefractPropertyGetter(classBuilder, proxyField, property, proxyId) : null;
        return (proxyField, setter, getter);
    }
    
    private static MethodBuilder RefractPropertySetter(
        TypeBuilder classBuilder, FieldInfo proxyField, PropertyInfo property, int proxyId)
    {
        var proxyMethod = classBuilder.DefineMethod($"_prism_{proxyId}_set_{property.Name}",
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
            MethodAttributes.SpecialName | MethodAttributes.Virtual,
            CallingConventions.Standard, null, new []{property.PropertyType});
        var attributeBuilder = new CustomAttributeBuilder(
            GenerationAttributeMeta.Constructors.Default, Array.Empty<object>());
        proxyMethod.SetCustomAttribute(attributeBuilder);
        classBuilder.DefineMethodOverride(proxyMethod, property.GetSetMethod()!);
        
        #region Code
        
        var code = proxyMethod.GetILGenerator();
        var labelPostprocessing = code.DefineLabel();
        var labelEnd = code.DefineLabel();
        
        #region Local variable 0 : arguments array.
        
        code.Emit(OpCodes.Ldarg_1);
        if (property.PropertyType.IsValueType)
            code.Emit(OpCodes.Box, typeof(object));
        // Pop the argument and store it into local variable 0.
        code.DeclareLocal(typeof(object));
        code.Emit(OpCodes.Stloc_0);
        
        #endregion
        
        #region Local variable 1 : access context.
        
        code.Emit(OpCodes.Ldarg_0);     // this
        code.Emit(OpCodes.Ldfld, proxyField);
        code.Emit(OpCodes.Ldloc_0);     // object
        code.Emit(OpCodes.Newobj, AccessContextMeta.Constructors.ObjectArgument);
        code.DeclareLocal(AccessContextMeta.ClassType);
        code.Emit(OpCodes.Stloc_1);
        
        #endregion
        
        #region Trigger setting event.
        
        code.Emit(OpCodes.Ldarg_0);                 // this
        code.Emit(OpCodes.Ldfld, proxyField);       // PropertyProxyEntry
        code.Emit(OpCodes.Ldloc_1);                 // AccessContext
        code.Emit(OpCodes.Call, PropertyProxyEntryMeta.Methods.TriggerSettingEvent);
        
        #endregion
        
        #region Check skipped and interrupted flags.
        
        // Check interrupted.
        code.Emit(OpCodes.Ldloc_1);
        code.Emit(OpCodes.Call, AccessContextMeta.Properties.Interrupted.Get);
        code.Emit(OpCodes.Brtrue, labelEnd);
        // Check skipped.
        code.Emit(OpCodes.Ldloc_1);
        code.Emit(OpCodes.Call, AccessContextMeta.Properties.Skipped.Get);
        code.Emit(OpCodes.Brtrue, labelPostprocessing);
        //
        #endregion
        
        #region Invoke property setter.
        
        code.Emit(OpCodes.Ldarg_1);
        code.Emit(OpCodes.Call, property.GetSetMethod()!);

        #endregion
        
        // #region Trigger after setting event.
        code.MarkLabel(labelPostprocessing);
        code.Emit(OpCodes.Ldarg_0);             // this
        code.Emit(OpCodes.Ldfld, proxyField);   // GeneratedExtensibleProperty
        code.Emit(OpCodes.Ldloc_1);             // AccessContext
        code.Emit(OpCodes.Call, PropertyProxyEntryMeta.Methods.TriggerAfterSettingEvent);
        // #endregion
        
        #region Return.
        code.MarkLabel(labelEnd);
        code.Emit(OpCodes.Ret);
        #endregion
        
        #endregion Code

        return proxyMethod;
    }
    
    private static MethodBuilder RefractPropertyGetter(
        TypeBuilder classBuilder, FieldBuilder proxyField, PropertyInfo property, int proxyId)
    {
        if (proxyField == null) throw new ArgumentNullException(nameof(proxyField));
        var proxyMethod = classBuilder.DefineMethod($"_prism_{proxyId}_get_{property.Name}",
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
            MethodAttributes.SpecialName | MethodAttributes.Virtual,
            CallingConventions.Standard, property.PropertyType, null);
        var attributeBuilder = new CustomAttributeBuilder(
            GenerationAttributeMeta.Constructors.Default, Array.Empty<object>());
        proxyMethod.SetCustomAttribute(attributeBuilder);
        classBuilder.DefineMethodOverride(proxyMethod, property.GetGetMethod()!);
        
        #region Code
        
        var code = proxyMethod.GetILGenerator();
        var labelPostprocessing = code.DefineLabel();
        var labelEnd = code.DefineLabel();

        #region Local variable 1 : access context.
        
        code.Emit(OpCodes.Ldarg_0);     // this
        code.Emit(OpCodes.Ldfld, proxyField);
        code.Emit(OpCodes.Ldnull);
        code.Emit(OpCodes.Newobj, AccessContextMeta.Constructors.ObjectArgument);
        code.DeclareLocal(AccessContextMeta.ClassType);
        code.Emit(OpCodes.Stloc_1);
        
        #endregion
        
        #region Trigger getting event.
        
        code.Emit(OpCodes.Ldarg_0);                 // this
        code.Emit(OpCodes.Ldfld, proxyField);       // PropertyProxyEntry
        code.Emit(OpCodes.Ldloc_1);                 // AccessContext
        code.Emit(OpCodes.Call, PropertyProxyEntryMeta.Methods.TriggerGettingEvent);
        
        #endregion
        
        #region Check skipped and interrupted flags.
        
        // Check interrupted.
        code.Emit(OpCodes.Ldloc_1);
        code.Emit(OpCodes.Call, AccessContextMeta.Properties.Interrupted.Get);
        code.Emit(OpCodes.Brtrue, labelEnd);
        // Check skipped.
        code.Emit(OpCodes.Ldloc_1);
        code.Emit(OpCodes.Call, AccessContextMeta.Properties.Skipped.Get);
        code.Emit(OpCodes.Brtrue, labelPostprocessing);
        //
        #endregion
        
        #region Local varaible 2: returning value from invoking proxied method.
        
        code.Emit(OpCodes.Ldarg_1);
        code.Emit(OpCodes.Call, property.GetSetMethod()!);
        code.DeclareLocal(property.PropertyType);
        code.Emit(OpCodes.Stloc_2);
        code.Emit(OpCodes.Ldloc_1);     // access context
        code.Emit(OpCodes.Ldloc_2);     // accessing value
        if (property.PropertyType.IsValueType)
            code.Emit(OpCodes.Box, property.PropertyType);
        code.Emit(OpCodes.Callvirt, AccessContextMeta.Properties.AccessingValue.Set);

        #endregion
        
        // #region Trigger invoked event.
        code.MarkLabel(labelPostprocessing);
        code.Emit(OpCodes.Ldarg_0);             // this
        code.Emit(OpCodes.Ldfld, proxyField);   // GeneratedExtensibleProperty
        code.Emit(OpCodes.Ldloc_1);             // AccessContext
        code.Emit(OpCodes.Call, PropertyProxyEntryMeta.Methods.TriggerAfterGettingEvent);
        // #endregion
        
        #region Return.
        code.MarkLabel(labelEnd);
        code.Emit(OpCodes.Ldloc_1);
        code.Emit(OpCodes.Call, AccessContextMeta.Properties.AccessingValue.Get);
        if (property.PropertyType.IsValueType)
            code.Emit(OpCodes.Unbox_Any, property.PropertyType);
        code.Emit(OpCodes.Ret);
        #endregion
        
        #endregion Code

        return proxyMethod;
    }

    private static (FieldBuilder, MethodBuilder) RefractMethod(
        TypeBuilder classBuilder, MethodInfo baseMethod, int proxyId)
    {
         var proxyField = classBuilder.DefineField(
            $"_prism_{proxyId}_{baseMethod.Name}_Proxy", MethodProxyEntryMeta.ClassType, 
            FieldAttributes.Private | FieldAttributes.InitOnly);
        
        var methodParameters = baseMethod.GetParameters();
        
        var proxyMethod = classBuilder.DefineMethod($"_prism_{proxyId}_{baseMethod.Name}",
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
            MethodAttributes.SpecialName | MethodAttributes.Virtual,
            CallingConventions.Standard, baseMethod.ReturnType, 
            methodParameters.Select(parameter => parameter.ParameterType).ToArray()
        );
        var attributeBuilder = new CustomAttributeBuilder(
            GenerationAttributeMeta.Constructors.Default, Array.Empty<object>());
        proxyMethod.SetCustomAttribute(attributeBuilder);
        classBuilder.DefineMethodOverride(proxyMethod, baseMethod);
        
        #region Code
        
        var code = proxyMethod.GetILGenerator();
        var labelPostprocessing = code.DefineLabel();
        var labelEnd = code.DefineLabel();
        
        #region Local variable 0 : arguments array.
        
        code.Emit(OpCodes.Ldc_I4, methodParameters.Length);
        code.Emit(OpCodes.Newarr, typeof(object));
        for (var parameterIndex = 0; parameterIndex < methodParameters.Length; ++parameterIndex)
        {
            code.Emit(OpCodes.Dup);
            code.Emit(OpCodes.Ldc_I4, parameterIndex);
            code.Emit(OpCodes.Ldarg, parameterIndex + 1);
            if (methodParameters[parameterIndex].ParameterType.IsValueType)
                code.Emit(OpCodes.Box, methodParameters[parameterIndex].ParameterType);
            code.Emit(OpCodes.Stelem_Ref);
        }
        // Pop the arguments array and store it into local variable 0.
        code.DeclareLocal(ObjectArrayMeta.ClassType);
        code.Emit(OpCodes.Stloc_0);
        
        #endregion
        
        #region Local variable 1 : invocation context.
        
        code.Emit(OpCodes.Ldarg_0);     // this
        code.Emit(OpCodes.Ldfld, proxyField); 
        code.Emit(OpCodes.Ldloc_0);     // object?[]
        code.Emit(OpCodes.Newobj, InvocationContextMeta.Constructors.ObjectArguments);
        code.DeclareLocal(InvocationContextMeta.ClassType);
        code.Emit(OpCodes.Stloc_1);
        
        #endregion
        
        #region Trigger invoking event.
        
        code.Emit(OpCodes.Ldarg_0);             // this
        code.Emit(OpCodes.Ldfld, proxyField);   // GeneratedExtensibleMethod
        code.Emit(OpCodes.Ldloc_1);             // InvocationContext
        code.Emit(OpCodes.Call, MethodProxyEntryMeta.Methods.TriggerInvoking);
        
        #endregion
        
        #region Check skipped and interrupted flags.
        
        // Check interrupted.
        code.Emit(OpCodes.Ldloc_1);
        code.Emit(OpCodes.Call, InvocationContextMeta.Properties.Interrupted.Get);
        code.Emit(OpCodes.Brtrue, labelEnd);
        // Check skipped.
        code.Emit(OpCodes.Ldloc_1);
        code.Emit(OpCodes.Call, InvocationContextMeta.Properties.Skipped.Get);
        code.Emit(OpCodes.Brtrue, labelPostprocessing);
        //
        #endregion
        
        #region Local varaible 2: returning value from invoking proxied method.
        
        for (var parameterIndex = 0; parameterIndex <= methodParameters.Length; ++parameterIndex)
        {
            code.Emit(OpCodes.Ldarg, parameterIndex);
        }
        code.Emit(OpCodes.Call, baseMethod);
        if (baseMethod.DeclaringType != typeof(void))
        {
            code.DeclareLocal(baseMethod.ReturnType);
            code.Emit(OpCodes.Stloc_2);
            code.Emit(OpCodes.Ldloc_1);     // InvocationContext
            code.Emit(OpCodes.Ldloc_2);     // returning value
            if (baseMethod.ReturnType.IsValueType)
                code.Emit(OpCodes.Box, baseMethod.ReturnType);
            code.Emit(OpCodes.Callvirt, InvocationContextMeta.Properties.ReturningValue.Set);
        }

        #endregion
        
        // #region Trigger invoked event.
        code.MarkLabel(labelPostprocessing);
        code.Emit(OpCodes.Ldarg_0);             // this
        code.Emit(OpCodes.Ldfld, proxyField);   // GeneratedExtensibleMethod
        code.Emit(OpCodes.Ldloc_1);             // InvocationContext
        code.Emit(OpCodes.Call, MethodProxyEntryMeta.Methods.TriggerInvoked);
        // #endregion
        
        #region Return value.
        code.MarkLabel(labelEnd);
        if (baseMethod.ReturnType != typeof(void))
        {
            code.Emit(OpCodes.Ldloc_1);
            code.Emit(OpCodes.Call, InvocationContextMeta.Properties.ReturningValue.Get);
            if (baseMethod.ReturnType.IsValueType)
                code.Emit(OpCodes.Unbox_Any, baseMethod.ReturnType);
        }
        code.Emit(OpCodes.Ret);
        #endregion
        
        #endregion Code

        return (proxyField, proxyMethod);
    }

    private static void RefractConstructor(
        TypeBuilder classBuilder, FieldInfo managerField, ConstructorInfo baseConstructor,
        List<(FieldBuilder, MethodBuilder, MethodInfo)> methodProxies,
        List<(FieldBuilder, MethodBuilder? setter, MethodBuilder? getter, PropertyInfo)> propertyProxies)
    {
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

        #region Initialize proxies manager.
        constructorCode.Emit(OpCodes.Ldarg_0);
        constructorCode.Emit(OpCodes.Newobj, ProxyManagerMeta.Constructors.Default);
        constructorCode.Emit(OpCodes.Stfld, managerField);
        #endregion

        var generatedProxies = methodProxies.ToList();
        foreach (var (proxyField, setter, getter, property) in propertyProxies)
        {
            if (setter != null && property.CanWrite)
            {
                generatedProxies.Add((proxyField, setter, property.GetSetMethod()!));
            }

            if (getter != null && property.CanRead)
            {
                generatedProxies.Add((proxyField, getter, property.GetGetMethod()!));
            }
        }


        #region Initialize proxies.
        foreach (var (proxyField, proxyMethod, baseMethod) in generatedProxies)
        {
            constructorCode.Emit(OpCodes.Ldarg_0);
            constructorCode.Emit(OpCodes.Ldarg_0);
            constructorCode.Emit(OpCodes.Ldtoken, baseMethod);
            constructorCode.Emit(OpCodes.Call, MethodBaseMeta.Methods.GetMethodFromHandle);
            constructorCode.Emit(OpCodes.Newobj, 
                MethodProxyEntryMeta.Constructors.ObjectMethodInfo);
            constructorCode.Emit(OpCodes.Stfld, proxyField);
            
            constructorCode.Emit(OpCodes.Ldarg_0);
            constructorCode.Emit(OpCodes.Ldfld, managerField);
            constructorCode.Emit(OpCodes.Ldtoken, baseMethod);
            constructorCode.Emit(OpCodes.Call, 
                MethodBaseMeta.Methods.GetMethodFromHandle);
            constructorCode.Emit(OpCodes.Ldarg_0);
            constructorCode.Emit(OpCodes.Ldfld, proxyField);
            constructorCode.Emit(OpCodes.Callvirt, ProxyManagerMeta.Methods.AddMethodProxy);
        }
        #endregion
        
        // Return.
        constructorCode.Emit(OpCodes.Ret);
    }

    private static void ImplementProxyInterface(TypeBuilder classBuilder, FieldInfo managerField)
    {
        var gettingMethod = classBuilder.DefineMethod("_prism_GetMethodProxy",
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
            MethodAttributes.SpecialName | MethodAttributes.Virtual,
            CallingConventions.Standard,
            typeof(IMethodProxy), new[] { typeof(MethodInfo) });
        classBuilder.DefineMethodOverride(gettingMethod, 
            ProxiedObjectMeta.Methods.GetMethodProxy);
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
        
        var gettingProperty = classBuilder.DefineMethod("_prism_GetPropertyProxy",
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot |
            MethodAttributes.SpecialName | MethodAttributes.Virtual,
            CallingConventions.Standard,
            typeof(IPropertyProxy), new[] { typeof(PropertyInfo) });
        classBuilder.DefineMethodOverride(gettingProperty, 
            ProxiedObjectMeta.Methods.GetPropertyProxy);
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
    }
}