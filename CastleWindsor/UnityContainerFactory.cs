using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity;
using Unity.Lifetime;
using Unity.Resolution;
using Unity.RegistrationByConvention;
using Castle.DynamicProxy;

namespace CastleWindsorResearch
{
    public static class UnityContainerFactory
    {
        public static IUnityContainer Container { get; set; }

        public static T Resolve<T>()
        {
            return Container.Resolve<T>();
        }

        public static T Resolve<T>(string name)
        {
            if (Container.IsRegistered<T>(name))
            {
                return Container.Resolve<T>(name);
            }
            return default(T);
        }

        public static IEnumerable<T> ResolveAll<T>(params ResolverOverride[] resolverOverrides)
        {
            return Container.ResolveAll<T>(resolverOverrides);
        }

        public static void RegisterSingleton<TInterface, TImplementation>()
            where TImplementation : TInterface
        {
            Container.RegisterType<TInterface, TImplementation>(lifetimeManager: new ContainerControlledLifetimeManager()); // Singleton
        }
        public static void Register<TInterface, TImplementation>(ITypeLifetimeManager lifeTimeManager = null)
            where TImplementation : TInterface
        {
            Container.RegisterType<TInterface, TImplementation>(lifetimeManager: lifeTimeManager ?? new TransientLifetimeManager());
        }

        public static void Register<TInterface, TImplementation>(string name)
            where TImplementation : TInterface
        {
            Container.RegisterType<TInterface, TImplementation>(name);
        }

        public static void Register<TInterface, TImplementation>()
            where TImplementation : TInterface
        {
            Container.RegisterType<TInterface, TImplementation>();
        }

        ///// <summary>
        ///// A partir de la interfaz T registramos por Ioc automáticamente
        ///// todas las clases que la implementan de la lista de assemblies donde se buscan registros por convención
        ///// (assemblies de Softeng.Pb y PbWebSite* )
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        //public static void RegisterAutoNamed<T>()
        //{
        //    foreach (var type in ByConventionRegistrator.GetAutoNamedTypesFor<T>())
        //    {
        //        Container.RegisterType(typeof(T), type, type.Name);
        //    }
        //}


        //internal static void Register<TTo, TFrom>()
        //	 where TTo : TFrom
        //{
        //	Container.RegisterType<TTo, TFrom>();
        //}
        public static IUnityContainer CreateContainer(IInterceptor[] globalInterceptors)
        {
            IUnityContainer unityContainer = new UnityContainer();
            ByConventionRegistrator.RegisterTypesByConvention(unityContainer);
            return InterceptContainer(unityContainer, globalInterceptors);
        }

        private static IUnityContainer InterceptContainer(IUnityContainer unityContainer, IInterceptor[] globalInterceptors)
        {
            var childContainer = unityContainer.CreateChildContainer();
            foreach (var registration in unityContainer.Registrations)
            {
                if (registration.MappedToType.GetInterfaces().Any(x => x == typeof(IInterceptor)))
                    continue;   // no registrar los propios interceptores 

                if (!registration.RegisteredType.IsInterface)
                    continue;

                var allInterceptors = globalInterceptors.Concat(ReflectionHelper.GetAttributeInterceptors(registration.MappedToType)).ToArray();
                //ReflectionHelper.
                var proxied = ProxyManager.Generator.CreateInterfaceProxyWithTarget(registration.RegisteredType, unityContainer.Resolve(registration.RegisteredType, registration.Name), allInterceptors);

                childContainer.RegisterFactory(registration.RegisteredType, registration.Name, (c) => proxied);
            }
            return childContainer;
        }
    }

    public static class ByConventionRegistrator
    {
        public static Func<List<Assembly>> AdditionalAssembliesForConventionRegistration { get; set; }

        public static void RegisterTypesByConvention(IUnityContainer container)
        {
            container.RegisterTypes(
                types: AllClasses.FromAssemblies(GetAssembilesWithConventionRegistration()).Where(FilterTypesToRegisterByConvention()),
                getFromTypes: WithMappings.FromMatchingInterface,
                getName: WithName.Default,
                getLifetimeManager: WithLifetime.Custom<TransientLifetimeManager>);
        }



        /// <summary>
        /// Obtenemos la lista de assembiles donde buscaremos las convenciones
        /// 
        /// Es muy importante filtrar y no utilizar "AllLoadedAssembiles" ya que la carga inicial con este modo tarda 6 segundos en nuestras máquinas (i7 4970)
        /// Cuando filtramos tarda menos de 0,5seg solo un poco más que con el registro "manual"
        /// </summary>
        /// <returns></returns>
        public static List<Assembly> GetAssembilesWithConventionRegistration()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(ass => ass.FullName.Contains("CastleWindsorResearch"))
                .ToList();

            if (AdditionalAssembliesForConventionRegistration != null)
            {
                assemblies.AddRange(AdditionalAssembliesForConventionRegistration());
            }

            return assemblies;
        }



        /// <summary>
        /// A partir de la interfaz T obtenemos todas las clases que la implementan
        /// de la lista de assemblies donde se buscan registros por convención
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<Type> GetAutoNamedTypesFor<T>()
        {
            return ByConventionRegistrator.GetAssembilesWithConventionRegistration()
                                             .SelectMany(s =>
                                             {
                                                 Type[] types;
                                                 try
                                                 {
                                                     types = s.GetTypes();
                                                 }
                                                 catch (Exception ex)
                                                 {
                                                     if (ex is System.Reflection.ReflectionTypeLoadException)
                                                     {
                                                         var typeLoadException = ex as ReflectionTypeLoadException;
                                                         var loaderExceptions = typeLoadException.LoaderExceptions;
                                                         var errorMsg = typeLoadException.ToString() + Environment.NewLine + "loaderException: " + string.Join("loaderException: ", loaderExceptions.Select(e => e.ToString()).ToList());
                                                         throw new ApplicationException(errorMsg);
                                                     }
                                                     throw;
                                                 }
                                                 return types;
                                             }).Where(IsRegistrable<T>).ToList();
        }

        public static bool IsRegistrable<T>(Type t)
        {
            return typeof(T).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract;
        }

        /// <summary>
        /// Filtramos algunos tipos que no interesa que se registren por convención.
        /// 
        /// Ejemplo, si registramos los controladores, como por defecto se registran con PerExecutionContextLifetimeManager, se reaprovecha 
        /// la instancia del controlador, cosa que MVC no permite y da excepción cuando en la misma página se utiliza 2 veces el mismo controlador
        /// Ejemplos son 
        /// - ChildActions  Html.Action("childaction")
        /// - Algunos bloques con comportamiento, si tiramos 2 veces el mismo petan
        /// </summary>
        /// <returns></returns>
        private static Func<Type, bool> FilterTypesToRegisterByConvention()
        {
            return t => true;
            //return t => !t.IsSubclassOf(typeof(Controller));
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    sealed class InterceptWithAttribute : Attribute
    {
        public Type Interceptor { get; }

        public InterceptWithAttribute(Type interceptor)
        {
            Interceptor = interceptor;
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

    public class ProxyManager
    {
        public static readonly ProxyGenerator Generator = new ProxyGenerator();
    }
}
