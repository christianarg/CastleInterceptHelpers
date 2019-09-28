using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Text;

namespace CastleWindsorResearch
{
    public interface IMyService
    {
        void ExecuteCoso();
    }

    public class MyService : IMyService
    {
        public void ExecuteCoso()
        {
            Console.WriteLine("Coso");
        }
    }

    public interface IMyServicePeta
    {
        void ExecuteCoso();
    }


    public class MyServicePeta : IMyServicePeta
    {
        public void ExecuteCoso()
        {
            throw new Exception("y petó");
        }
    }

    public class MyInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            Console.WriteLine("Before invocation");
            try
            {
                invocation.Proceed();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ha petao {ex.ToString()}");
                throw;
            }
            Console.WriteLine("After invocation");
        }
    }

    public class ProxyManager
    {
        public static readonly ProxyGenerator Generator = new ProxyGenerator();
    }
}
