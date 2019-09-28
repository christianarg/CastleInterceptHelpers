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
        IUnityContainer unityContainer;

        [TestInitialize]
        public void Init()
        {
            unityContainer = new UnityContainer();
        }

        [TestMethod]
        public void GlobalInterceptorTest()
        {
            // ARRANGE
            RealServiceExecuted.Executed = false;
            MyInterceptor.ResetExecuted();

            unityContainer.RegisterType<IMyFooService, MyFooService>();
            unityContainer = InterceptionHelper.InterceptContainer(unityContainer, new IInterceptor[] { new MyInterceptor() });

            var myService = unityContainer.Resolve<IMyFooService>();
            
            // ACT
            myService.Execute();

            // ASSERT
            Assert.IsTrue(RealServiceExecuted.Executed);
            Assert.IsTrue(MyInterceptor.ExecutedBefore);
            Assert.IsTrue(MyInterceptor.ExecutedAfter);
        }

        [TestMethod]
        public void AttributeInterceptorTest()
        {
            // ARRANGE
            RealServiceExecuted.Executed = false;
            MyInterceptor.ResetExecuted();

            unityContainer.RegisterType<IMyFooService, MyFooServiceWithAttributeInterceptor>();
            unityContainer = InterceptionHelper.InterceptContainer(unityContainer, new IInterceptor[] { });

            var myService = unityContainer.Resolve<IMyFooService>();

            // ACT
            myService.Execute();

            // ASSERT
            Assert.IsTrue(RealServiceExecuted.Executed);
            Assert.IsTrue(MyInterceptor.ExecutedBefore);
            Assert.IsTrue(MyInterceptor.ExecutedAfter);
        }


        [TestMethod]
        public void GlobalAndAttributeInterceptorTest()
        {
            // ARRANGE
            RealServiceExecuted.Executed = false;
            MyInterceptor.ResetExecuted();
            MyOtherInterceptor.ResetExecuted();

            unityContainer.RegisterType<IMyFooService, MyFooServiceWithAttributeInterceptor>();
            unityContainer = InterceptionHelper.InterceptContainer(unityContainer, new IInterceptor[] { new MyOtherInterceptor() });

            var myService = unityContainer.Resolve<IMyFooService>();

            // ACT
            myService.Execute();

            // ASSERT
            Assert.IsTrue(RealServiceExecuted.Executed);
            Assert.IsTrue(MyInterceptor.ExecutedBefore);
            Assert.IsTrue(MyInterceptor.ExecutedAfter);
            Assert.IsTrue(MyOtherInterceptor.ExecutedBefore);
            Assert.IsTrue(MyOtherInterceptor.ExecutedAfter);
        }
    }

    public interface IMyFooService
    {
        void Execute();
    }

    public static class RealServiceExecuted
    {
        public static bool Executed;
    }

    public class MyFooService : IMyFooService
    {
        public void Execute()
        {
            RealServiceExecuted.Executed = true;
        }
    }

    [InterceptWith(typeof(MyInterceptor))]
    public class MyFooServiceWithAttributeInterceptor : IMyFooService
    {
        public void Execute()
        {
            RealServiceExecuted.Executed = true;
        }
    }

    public class MyInterceptor : IInterceptor
    {
        public static bool ExecutedBefore;
        public static bool ExecutedAfter;

        public static void ResetExecuted()
        {
            ExecutedBefore = false;
            ExecutedAfter = false;
        }

        public void Intercept(IInvocation invocation)
        {
            ExecutedBefore = true;
            invocation.Proceed();
            ExecutedAfter = true;
        }
    }

    public class MyOtherInterceptor : IInterceptor
    {
        public static bool ExecutedBefore;
        public static bool ExecutedAfter;

        public static void ResetExecuted()
        {
            ExecutedBefore = false;
            ExecutedAfter = false;
        }

        public void Intercept(IInvocation invocation)
        {
            ExecutedBefore = true;
            invocation.Proceed();
            ExecutedAfter = true;
        }
    }
}
