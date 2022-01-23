using System.Reflection;
using Nebula.Proxying;

namespace Nebula.Prism;

public partial class DynamicGenerator
{
    private static class TypeMeta
    {
        public static readonly Type ClassType = typeof(Type);

        public static class Methods
        {
            public static readonly MethodInfo GetProperty =
                ClassType.GetMethod(nameof(Type.GetProperty),
                    new[] { typeof(string) })!;
            public static readonly MethodInfo GetMethod =
                ClassType.GetMethod(nameof(Type.GetProperty),
                    new[] { typeof(string) })!;
        }
    }
    
    private static class MethodBaseMeta
    {
        public static readonly Type ClassType = typeof(MethodBase);

        public static class Methods
        {
            public static readonly MethodInfo GetMethodFromHandle =
                ClassType.GetMethod(nameof(MethodBase.GetMethodFromHandle),
                    new[] { typeof(RuntimeMethodHandle) })!;
        }
    }

    private static class ObjectMeta
    {
        public static readonly Type ClassType = typeof(object);

        public static class Methods
        {
            public static readonly MethodInfo GetType =
                ClassType.GetMethod(nameof(object.GetType))!;
        }
    }
    
    private static class GenerationAttributeMeta
    {
        public static readonly Type ClassType = typeof(GeneratedByPrismAttribute);

        public static class Constructors
        {
            public static readonly ConstructorInfo Default =
                ClassType.GetConstructor(Array.Empty<Type>())!;

            public static readonly ConstructorInfo Object =
                ClassType.GetConstructor(new[] { typeof(object) })!;
        }
    }

    private static class ProxyManagerMeta
    {
        public static readonly Type ClassType = typeof(ProxyManager);

        public static class Constructors
        {
            public static readonly ConstructorInfo Default =
                ClassType.GetConstructor(Array.Empty<Type>())!;
        }

        public static class Methods
        {
            public static readonly MethodInfo AddMethodProxy =
                ClassType.GetMethod(nameof(ProxyManager.AddMethodProxy))!;

            public static readonly MethodInfo AddPropertyProxy =
                ClassType.GetMethod(nameof(ProxyManager.AddPropertyProxy))!;

            public static readonly MethodInfo GetMethodProxy =
                ClassType.GetMethod(nameof(ProxyManager.GetMethodProxy))!;
            
            public static readonly MethodInfo GetPropertyProxy =
                ClassType.GetMethod(nameof(ProxyManager.GetPropertyProxy))!;
        }
    }

    private static class ObjectArrayMeta
    {
        public static readonly Type ClassType = typeof(object[]);
    }

    private static class ProxiedObjectMeta
    {
        public readonly static Type ClassType = typeof(IProxiedObject);

        public static class Methods
        {
            public static readonly MethodInfo GetMethodProxy =
                ClassType.GetMethod(nameof(IProxiedObject.GetMethodProxy))!;

            public static readonly MethodInfo GetPropertyProxy =
                ClassType.GetMethod(nameof(IProxiedObject.GetPropertyProxy))!;
        }
    }
    
    private static class MethodProxyEntryMeta
    {
        public static readonly Type ClassType =
            typeof(MethodProxyEntry);

        public static class Constructors
        {
            public static readonly ConstructorInfo ObjectMethodInfo =
                ClassType.GetConstructor(new[] { typeof(object), typeof(MethodInfo) })!;
        }

        public static class Methods
        {
            public static readonly MethodInfo TriggerInvoking =
                ClassType.GetMethod(nameof(MethodProxyEntry.TriggerInvokingEvent))!;

            public static readonly MethodInfo TriggerInvoked =
                ClassType.GetMethod(nameof(MethodProxyEntry.TriggerInvokedEvent))!;
        }
    }
    
    private static class PropertyProxyEntryMeta
    {
        public static readonly Type ClassType =
            typeof(PropertyProxyEntry);

        public static class Constructors
        {
            public static readonly ConstructorInfo ObjectPropertyInfo =
                ClassType.GetConstructor(new[] { typeof(object), typeof(PropertyInfo) })!;
        }

        public static class Methods
        {
            public static readonly MethodInfo TriggerGettingEvent =
                ClassType.GetMethod(nameof(PropertyProxyEntry.TriggerGettingEvent))!;

            public static readonly MethodInfo TriggerAfterGettingEvent =
                ClassType.GetMethod(nameof(PropertyProxyEntry.TriggerAfterGettingEvent))!;
            
            public static readonly MethodInfo TriggerSettingEvent =
                ClassType.GetMethod(nameof(PropertyProxyEntry.TriggerSettingEvent))!;

            public static readonly MethodInfo TriggerAfterSettingEvent =
                ClassType.GetMethod(nameof(PropertyProxyEntry.TriggerAfterSettingEvent))!;
        }
    }

    private static class InvocationContextMeta
    {
        public static readonly Type ClassType = typeof(InvocationContext);

        public static class Constructors
        {
            public static readonly ConstructorInfo ObjectArguments =
                ClassType.GetConstructor(new[] { typeof(IExtensibleMethod), typeof(object[]) })!;
        }

        public static class Properties
        {
            public static class Skipped
            {
                public static readonly MethodInfo Get =
                    ClassType.GetProperty(nameof(InvocationContext.Skipped))!.GetMethod!;
            }

            public static class Interrupted
            {
                public static readonly MethodInfo Get =
                    ClassType.GetProperty(nameof(InvocationContext.Interrupted))!.GetMethod!;
            }

            public static class ReturningValue
            {
                public static readonly MethodInfo Get =
                    ClassType.GetProperty(nameof(InvocationContext.ReturningValue))!.GetMethod!;

                public static readonly MethodInfo Set =
                    ClassType.GetProperty(nameof(InvocationContext.ReturningValue))!.SetMethod!;
            }

            public static class Arguments
            {
                public static readonly MethodInfo Get =
                    ClassType.GetProperty(nameof(InvocationContext.Arguments))!.GetMethod!;
            }
        }
    }

    private static class AccessContextMeta
    {
        public static readonly Type ClassType = typeof(AccessContext);

        public static class Constructors
        {
            public static readonly ConstructorInfo ObjectArgument =
                ClassType.GetConstructor(new[] { typeof(IExtensibleProperty), typeof(object) })!;
        }

        public static class Properties
        {
            public static class Skipped
            {
                public static readonly MethodInfo Get =
                    ClassType.GetProperty(nameof(AccessContext.Skipped))!.GetMethod!;
            }

            public static class Interrupted
            {
                public static readonly MethodInfo Get =
                    ClassType.GetProperty(nameof(AccessContext.Interrupted))!.GetMethod!;
            }

            public static class AccessingValue
            {
                public static readonly MethodInfo Get =
                    ClassType.GetProperty(nameof(AccessContext.AccessingValue))!.GetMethod!;

                public static readonly MethodInfo Set =
                    ClassType.GetProperty(nameof(AccessContext.AccessingValue))!.SetMethod!;
            }
        }
    }
}