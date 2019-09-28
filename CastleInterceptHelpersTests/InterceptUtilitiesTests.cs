using Castle.DynamicProxy;
using CastleInterceptHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using Unity;

namespace CastleInterceptHelpersTests
{
    [TestClass]
    public class InterceptUtilitiesTests
    {
        [TestInitialize]
        public void Init()
        {
        }

        [TestMethod]
        public void GlobalInterceptorTest()
        {
            // ARRANGE
            MyFooService.Executed = false;
            MyInterceptor.ResetExecuted();

            IUnityContainer unityContainer = new UnityContainer();
            unityContainer.RegisterType<IMyFooService, MyFooService>();
            unityContainer = InterceptionHelper.InterceptContainer(unityContainer, new IInterceptor[] { new MyInterceptor() });

            var myService = unityContainer.Resolve<IMyFooService>();
            // ACT
            myService.Execute();

            // ASSERT
            Assert.IsTrue(MyFooService.Executed);
            Assert.IsTrue(MyInterceptor.ExecutedBefore);
            Assert.IsTrue(MyInterceptor.ExecutedAfter);
        }
    }

    public interface IMyFooService
    {
        void Execute();
    }

    public class MyFooService : IMyFooService
    {
        public static bool Executed;
        public void Execute()
        {
            Executed = true;
        }
    }

    public class MyInterceptor : IInterceptor
    {
        public static void ResetExecuted()
        {
            ExecutedBefore = false;
            ExecutedAfter = false;
        }
        public static bool ExecutedBefore;
        public static bool ExecutedAfter;

        public void Intercept(IInvocation invocation)
        {
            ExecutedBefore = true;
            invocation.Proceed();
            ExecutedAfter = true;
        }
    }
}
