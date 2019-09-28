using System;
using System.Linq;
using System.Reflection;
using Unity;
using Unity.Resolution;
using Castle.DynamicProxy;

namespace CastleInterceptHelpers
{
    public static class InterceptionHelper
    {
        /// <summary>
        /// Creates a child container with all the registrations from the parent container intercepted by globalInterceptors
        /// and attribute interceptors
        /// </summary>
        /// <param name="unityContainer"></param>
        /// <param name="globalInterceptors"></param>
        /// <returns></returns>
        public static IUnityContainer InterceptContainer(IUnityContainer unityContainer, IInterceptor[] globalInterceptors)
        {
            var childContainer = unityContainer.CreateChildContainer();
            foreach (var registration in unityContainer.Registrations)
            {
                if (registration.MappedToType.GetInterfaces().Any(x => x == typeof(IInterceptor)))
                    continue;   // no registrar los propios interceptores 

                if (!registration.RegisteredType.IsInterface)
                    continue;

                var allInterceptors = globalInterceptors.Concat(ReflectionHelper.GetAttributeInterceptors(registration.MappedToType)).ToArray();

                var proxied = ProxyManager.Generator.CreateInterfaceProxyWithTarget(registration.RegisteredType, unityContainer.Resolve(registration.RegisteredType, registration.Name), allInterceptors);

                childContainer.RegisterFactory(registration.RegisteredType, registration.Name, (c) => proxied);
            }
            return childContainer;
        }
    }

    public class ReflectionHelper
    {
        public static IInterceptor[] GetAttributeInterceptors(Type type)
        {
            var interceptorTypes = type.GetCustomAttributes<InterceptWithAttribute>().Select(x => x.Interceptor).ToArray();
            return interceptorTypes.Select(x => Activator.CreateInstance(x) as IInterceptor).ToArray();

        }
    }
}
