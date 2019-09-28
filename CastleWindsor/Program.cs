using System;
using Unity;

namespace CastleWindsorResearch
{
    class Program
    {

        static void Main(string[] args)
        {
            //CreateProxy();

            var container = UnityContainerFactory.CreateContainer();

            var myService = container.Resolve<IMyService>();
            myService.ExecuteCoso();

            var myServicePeta = container.Resolve<IMyServicePeta>();

            try
            {
                myServicePeta.ExecuteCoso();
            }
            catch (Exception)
            {
            }

            Console.ReadLine();
        }

        private static void InterceptWithUnityAManopla()
        {
            IUnityContainer container = new UnityContainer();
            container.RegisterType<IMyService, MyService>();
            var myServiceAPelo = container.Resolve<IMyService>();
            myServiceAPelo.ExecuteCoso();

            var childContainer = container.CreateChildContainer();
            foreach (var registration in container.Registrations)
            {

                var proxied = ProxyManager.Generator.CreateInterfaceProxyWithTarget(registration.RegisteredType, container.Resolve(registration.RegisteredType, registration.Name), new MyInterceptor());

                childContainer.RegisterFactory(registration.RegisteredType, registration.Name, (c) => proxied);

                //if (string.IsNullOrEmpty(registration.Name))
                //{
                //    var proxied = ProxyManager.Generator.CreateInterfaceProxyWithTarget(registration.RegisteredType, container.Resolve(registration.RegisteredType), new MyInterceptor());

                //    childContainer.RegisterFactory(registration.RegisteredType, (c) => proxied);
                //}
                //else
                //{
                //    var proxied = ProxyManager.Generator.CreateInterfaceProxyWithTarget(registration.RegisteredType, container.Resolve(registration.RegisteredType, registration.Name), new MyInterceptor());

                //    childContainer.RegisterFactory(registration.RegisteredType, registration.Name, (c) => proxied);
                //}
            }

            var serviceFromChild = childContainer.Resolve<IMyService>();
            serviceFromChild.ExecuteCoso();
        }

        private static void CreateProxy()
        {
            var myService = ProxyManager.Generator.CreateInterfaceProxyWithTarget<IMyService>(new MyService(), new MyInterceptor());
            myService.ExecuteCoso();
        }
    }
}
