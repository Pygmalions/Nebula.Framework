using System.Reflection;

namespace Nebula.Proxying;

public partial class ProxyGenerator
{
    public static class Meta
    {
        public static class TypeMeta
        {
            public static readonly Type Class = typeof(Type);

            public static class Methods
            {
                public static readonly MethodInfo GetProperty =
                    Class.GetMethod(nameof(Type.GetProperty), new[] { typeof(string) })!;
            }
        }

        public static class PropertyInfoMeta
        {
            public static readonly Type Class = typeof(PropertyInfo);
        }

        public static class ObjectArrayMeta
        {
            public static readonly Type Class = typeof(object[]);
        }

        public static class ProxyGeneratorMeta
        {
            public static readonly Type Class = typeof(ProxyGenerator);

            public static class Methods
            {
                public static readonly MethodInfo ProxyInitialize =
                    Class.GetMethod(nameof(InitializeProxy))!;
            }
        }

        public static class ProxyManagerMeta
        {
            public static readonly Type Class = typeof(ProxyManager);

            public static class Constructors
            {
                public static readonly ConstructorInfo Default = Class.GetConstructor(Array.Empty<Type>())!;
            }

            public static class Methods
            {
                public static readonly MethodInfo GetProxy = Class.GetMethod(nameof(ProxyManager.GetProxy))!;
                public static readonly MethodInfo AddProxy = Class.GetMethod(nameof(ProxyManager.AddProxy))!;
                public static readonly MethodInfo RemoveProxy = Class.GetMethod(nameof(ProxyManager.RemoveProxy))!;
            }
        }

        public static class ProxyMeta
        {
            public static readonly Type Class = typeof(Proxy);

            public static class Constructors
            {
                public static readonly ConstructorInfo Default = Class.GetConstructor(
                    new[] { typeof(object), typeof(MethodInfo), typeof(PropertyInfo) })!;
            }

            public static class Methods
            {
                public static readonly MethodInfo Invoke = Class.GetMethod(nameof(Proxy.Invoke))!;
            }

            public static class Properties
            {
                public static class Invoker
                {
                    public static readonly PropertyInfo Property = Class.GetProperty(nameof(Proxy.Invoker))!;

                    public static readonly MethodInfo Get = Property.GetGetMethod()!;

                    public static readonly MethodInfo Set = Property.GetSetMethod()!;
                }
            }
        }

        public static class MethodBaseMeta
        {
            public static readonly Type Class = typeof(MethodBase);

            public static class Methods
            {
                public static readonly MethodInfo GetMethodFromHandle =
                    Class.GetMethod(nameof(MethodBase.GetMethodFromHandle), new[] { typeof(RuntimeMethodHandle) })!;
            }
        }

        public static class MethodInfoMeta
        {
            public static readonly Type Class = typeof(MethodInfo);
        }

        public static class ObjectMeta
        {
            public static readonly Type Class = typeof(object);

            public static class Methods
            {
                public static readonly MethodInfo GetTypeHandle = Class.GetMethod(nameof(GetType))!;
            }
        }

        public static class ContextMeta
        {
            public static readonly Type Class = typeof(Context);

            public static class Properties
            {
                public static class ProxiedMethod
                {
                    public static readonly PropertyInfo Property = Class.GetProperty(nameof(Context.ProxiedMethod))!;

                    public static readonly MethodInfo Get = Property.GetGetMethod()!;
                }

                public static class ProxiedObject
                {
                    public static readonly PropertyInfo Property = Class.GetProperty(nameof(Context.ProxiedObject))!;

                    public static readonly MethodInfo Get = Property.GetGetMethod()!;
                }

                public static class Arguments
                {
                    public static readonly PropertyInfo Property = Class.GetProperty(nameof(Context.Arguments))!;

                    public static readonly MethodInfo Get = Property.GetGetMethod()!;
                }
            }

            public static class Methods
            {
                public static readonly MethodInfo Return = Class.GetMethod(nameof(Context.Return))!;
            }
        }

        public static class ActionContextMeta
        {
            public static readonly Type Class = typeof(Action<Context>);

            public static class Constructors
            {
                public static readonly ConstructorInfo Default = Class.GetConstructor(
                    new[] { typeof(object), typeof(IntPtr) })!;
            }
        }

        public static class AspectHandlerMeta
        {
            public static readonly Type Class = typeof(AspectHandler);
        }

        public static class AspectHandlerAttributeMeta
        {
            public static readonly Type Class = typeof(AspectHandlerAttribute);
        }

        public static class AspectTriggerMeta
        {
            public static readonly Type Class = typeof(AspectTrigger);
        }

        public static class ProxyAttributeMeta
        {
            public static readonly Type Class = typeof(ProxyAttribute);
        }
    }
}