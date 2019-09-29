using System;
using System.Linq;
using System.Reflection;
using Unity;
using Unity.Resolution;
using Castle.DynamicProxy;
using System.Collections.Generic;

namespace CastleInterceptHelpers
{
    public static class InterceptionHelper
    {
        public static IUnityContainer InterceptContainer(IUnityContainer unityContainer, Type[] globalInterceptors, InterceptionOptions options = null)
        {
            var interceptorInstances = globalInterceptors.Select(x => unityContainer.Resolve(x) as IInterceptor).ToArray();
            return InterceptContainer(unityContainer, interceptorInstances);
        }
        /// <summary>
        /// Creates a child container with all the registrations from the parent container intercepted by globalInterceptors
        /// and attribute interceptors
        /// </summary>
        /// <param name="unityContainer"></param>
        /// <param name="globalInterceptors"></param>
        /// <returns></returns>
        public static IUnityContainer InterceptContainer(IUnityContainer unityContainer, IInterceptor[] globalInterceptors, InterceptionOptions options = null)
        {
            options = options ?? new InterceptionOptions();

            var childContainer = unityContainer.CreateChildContainer();
            foreach (var registration in unityContainer.Registrations)
            {
                if (registration.MappedToType.GetInterfaces().Any(x => x == typeof(IInterceptor)))
                    continue;   // no registrar los propios interceptores 

                if (!registration.RegisteredType.IsInterface)
                    continue;

                if (ReflectionHelper.MustNotIntercept(registration.MappedToType))
                    continue;

                var allInterceptors = GetInterceptors(unityContainer, globalInterceptors, registration, options);

                var proxied = ProxyManager.Generator.CreateInterfaceProxyWithTarget(registration.RegisteredType, unityContainer.Resolve(registration.RegisteredType, registration.Name), allInterceptors);

                childContainer.RegisterFactory(registration.RegisteredType, registration.Name, (c) => proxied);
            }
            return childContainer;
        }

        private static IInterceptor[] GetInterceptors(IUnityContainer unityContainer, IInterceptor[] globalInterceptors, IContainerRegistration registration, InterceptionOptions options)
        {
            var attributeInterceptors = ReflectionHelper.GetAttributeInterceptors(registration.MappedToType, unityContainer);

            var interceptors = new List<IInterceptor>();
            if (options.GlobalInterceptorsOrder == GlobalInterceptorsOrder.AfterAttributeInterceptors)
            {
                interceptors.AddRange(attributeInterceptors);
                interceptors.AddRange(globalInterceptors);
            }
            else
            {
                interceptors.AddRange(globalInterceptors);
                interceptors.AddRange(attributeInterceptors);
            }

            return ReflectionHelper.RemoveExcludedGlobalInterceptors(registration.MappedToType, interceptors.ToArray());
        }
    }

    public class InterceptionOptions
    {
        public GlobalInterceptorsOrder GlobalInterceptorsOrder { get; set; }
    }


    public enum GlobalInterceptorsOrder
    {
        BeforeAttributeInterceptors = 0,
        AfterAttributeInterceptors = 1
    }

    public class ReflectionHelper
    {
        /// <summary>
        /// Returns a list of interceptor instances
        /// Instances are created with parent container so that if they have dependencies they are alse resolved
        /// For example LoggingInterceptor may use a LoggingService as dependency
        /// </summary>
        /// <param name="type"></param>
        /// <param name="unityContainer"></param>
        /// <returns></returns>
        public static IInterceptor[] GetAttributeInterceptors(Type type, IUnityContainer unityContainer)
        {
            var interceptorTypes = type.GetCustomAttributes<InterceptWithAttribute>().OrderByDescending(x => x.Order).Select(x => x.Interceptor).ToArray();
            return interceptorTypes.Select(x => unityContainer.Resolve(x) as IInterceptor).ToArray();
        }

        public static bool MustNotIntercept(Type type)
        {
            return type.GetCustomAttribute<DoNotInterceptAttribute>() != null;
        }

        public static IInterceptor[] RemoveExcludedGlobalInterceptors(Type type, IInterceptor[] interceptors)
        {
            var globalInterceptorsToRemove = type.GetCustomAttributes<RemoveGlobalInterceptorAttribute>().Select(x => x.GlobalInterceptorToRemove).ToArray();
            if (globalInterceptorsToRemove.Length == 0)
            {
                return interceptors;
            }

            var result = new List<IInterceptor>();
            foreach (var interceptor in interceptors)
            {
                if (globalInterceptorsToRemove.Any(x => x != interceptor.GetType()))
                {
                    result.Add(interceptor);
                }
            }
            return result.ToArray();
        }
    }
}
