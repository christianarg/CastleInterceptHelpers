using Castle.DynamicProxy;
using CastleInterceptHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
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
            RealServiceExecuted.ResetExecuted();
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
        public void GlobalInterceptorTypesOverloadTest()
        {
            // ARRANGE
            RealServiceExecuted.ResetExecuted();
            MyInterceptor.ResetExecuted();

            unityContainer.RegisterType<IMyFooService, MyFooService>();
            unityContainer = InterceptionHelper.InterceptContainer(unityContainer, new Type[] { typeof(MyInterceptor) });

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
            RealServiceExecuted.ResetExecuted();
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
        public void OrderedAttributeInterceptorTest()
        {
            // ARRANGE
            RealServiceExecuted.ResetExecuted();
            InterceptorsCalled.ResetList();

            unityContainer.RegisterType<IMyFooService, MyFooServiceWithOrderedAttributeInterceptor>();
            unityContainer = InterceptionHelper.InterceptContainer(unityContainer, new IInterceptor[] { });

            var myService = unityContainer.Resolve<IMyFooService>();

            // ACT
            myService.Execute();

            // ASSERT
            Assert.IsTrue(RealServiceExecuted.Executed);
            Assert.IsTrue(InterceptorsCalled.List[0].GetType() == typeof(MyInterceptor));
            Assert.IsTrue(InterceptorsCalled.List[1].GetType() == typeof(MyOtherInterceptor));
        }

        [TestMethod]
        public void GlobalAndAttributeInterceptorTest()
        {
            // ARRANGE
            RealServiceExecuted.ResetExecuted();
            MyInterceptor.ResetExecuted();
            MyOtherInterceptor.ResetExecuted();
            InterceptorsCalled.ResetList();

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
            Assert.IsTrue(InterceptorsCalled.List[0].GetType() == typeof(MyInterceptor));
            Assert.IsTrue(InterceptorsCalled.List[1].GetType() == typeof(MyOtherInterceptor));
        }

        [TestMethod]
        public void GlobalAndAttributeInterceptorConfigureOrderTest()
        {
            // ARRANGE
            RealServiceExecuted.ResetExecuted();
            MyInterceptor.ResetExecuted();
            MyOtherInterceptor.ResetExecuted();
            InterceptorsCalled.ResetList();

            unityContainer.RegisterType<IMyFooService, MyFooServiceWithAttributeInterceptor>();
            unityContainer = InterceptionHelper.InterceptContainer(unityContainer, new IInterceptor[] { new MyOtherInterceptor() }, new InterceptionOptions {  GlobalInterceptorsOrder = GlobalInterceptorsOrder.AfterAttributeInterceptors});

            var myService = unityContainer.Resolve<IMyFooService>();

            // ACT
            myService.Execute();

            // ASSERT
            Assert.IsTrue(RealServiceExecuted.Executed);
            Assert.IsTrue(MyInterceptor.ExecutedBefore);
            Assert.IsTrue(MyInterceptor.ExecutedAfter);
            Assert.IsTrue(MyOtherInterceptor.ExecutedBefore);
            Assert.IsTrue(MyOtherInterceptor.ExecutedAfter);
            Assert.IsTrue(InterceptorsCalled.List[0].GetType() == typeof(MyOtherInterceptor));
            Assert.IsTrue(InterceptorsCalled.List[1].GetType() == typeof(MyInterceptor));
        }

        [TestMethod]
        public void MustNotInterceptAttributeTest()
        {
            RealServiceExecuted.ResetExecuted();
            MyInterceptor.ResetExecuted();

            unityContainer.RegisterType<IMyFooService, MyFooServiceWithDoNotInterceptAttribute>();
            unityContainer = InterceptionHelper.InterceptContainer(unityContainer, new IInterceptor[] { new MyInterceptor() });

            var myService = unityContainer.Resolve<IMyFooService>();

            // ACT
            myService.Execute();

            // ASSERT
            Assert.IsTrue(RealServiceExecuted.Executed);
            Assert.IsFalse(MyInterceptor.ExecutedBefore);
            Assert.IsFalse(MyInterceptor.ExecutedAfter);
        }

        [TestMethod]
        public void RemoveGlobalInterceptorTest()
        {
            RealServiceExecuted.ResetExecuted();
            MyInterceptor.ResetExecuted();
            MyOtherInterceptor.ResetExecuted();

            unityContainer.RegisterType<IMyFooService, MyFooServiceWithRemoveGlobalInterceptor>();
            unityContainer = InterceptionHelper.InterceptContainer(unityContainer, new IInterceptor[] { new MyInterceptor(), new MyOtherInterceptor() });

            var myService = unityContainer.Resolve<IMyFooService>();

            // ACT
            myService.Execute();

            // ASSERT
            Assert.IsTrue(RealServiceExecuted.Executed);
            Assert.IsTrue(MyInterceptor.ExecutedBefore);
            Assert.IsTrue(MyInterceptor.ExecutedAfter);
            Assert.IsFalse(MyOtherInterceptor.ExecutedBefore);
            Assert.IsFalse(MyOtherInterceptor.ExecutedAfter);
        }
    }

    public interface IMyFooService
    {
        void Execute();
    }

    public static class RealServiceExecuted
    {
        public static bool Executed;
        public static bool ResetExecuted() => Executed = false;
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

    [DoNotIntercept]
    public class MyFooServiceWithDoNotInterceptAttribute : IMyFooService
    {
        public void Execute()
        {
            RealServiceExecuted.Executed = true;
        }
    }

    [RemoveGlobalInterceptor(typeof(MyOtherInterceptor))]
    public class MyFooServiceWithRemoveGlobalInterceptor : IMyFooService
    {
        public void Execute()
        {
            RealServiceExecuted.Executed = true;
        }
    }

    [InterceptWith(interceptor: typeof(MyInterceptor), order: 1)]
    [InterceptWith(interceptor: typeof(MyOtherInterceptor), order: 2)]
    public class MyFooServiceWithOrderedAttributeInterceptor : IMyFooService
    {
        public void Execute()
        {
            RealServiceExecuted.Executed = true;
        }
    }

    public static class InterceptorsCalled
    {
        public static List<IInterceptor> List { get; set; } = new List<IInterceptor>();
        public static void ResetList()
        {
            List = new List<IInterceptor>();
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
            InterceptorsCalled.List.Add(this);
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
            InterceptorsCalled.List.Add(this);

        }
    }
}
