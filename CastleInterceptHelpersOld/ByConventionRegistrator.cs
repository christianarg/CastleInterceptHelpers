using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity;
using Unity.Lifetime;
using Unity.RegistrationByConvention;

namespace CastleInterceptHelpers
{
    public static class ByConventionRegistrator
    {
        /// <summary>
        /// All assemblies that contains AssemblyNameSelector will be used for registration
        /// if both AssembliesForConventionRegistration and AssemblyNameSelector are informed assemblies will be used
        /// </summary>
        public static string AssemblyNameSelector { get; set; }

        /// <summary>
        /// All AssembliesForConventionRegistration will be used for registration.
        /// 
        /// if both AssembliesForConventionRegistration and AssemblyNameSelector are informed assemblies will be used
        /// </summary>
        public static Assembly[] AssembliesForConventionRegistration { get; set; }
        public static Func<Func<Type, bool>> ByConventionRegistrationFilter { get; set; }

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
        public static Assembly[] GetAssembilesWithConventionRegistration()
        {
            if(string.IsNullOrWhiteSpace(AssemblyNameSelector) && AssembliesForConventionRegistration == null)
            {
                throw new Exception("ByConventionRegistration Assemblies not configured.");
            }

            var assemblies = new List<Assembly>();

            if (!string.IsNullOrWhiteSpace(AssemblyNameSelector))
            {
                assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies()
                    .Where(ass => ass.FullName.Contains(AssemblyNameSelector))
                    .ToList());
            }

            if (AssembliesForConventionRegistration != null)
            {
                assemblies.AddRange(AssembliesForConventionRegistration);
            }

            return assemblies.ToArray();
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
            if(ByConventionRegistrationFilter != null)
            {
                return ByConventionRegistrationFilter();
            }
            else
            {
                return t => true;
            }
        }
    }
}
