using Unity;
using Castle.DynamicProxy;

namespace CastleInterceptHelpers
{

    public static class UnityContainerFactory
    {
        public static IUnityContainer CreateContainer(IInterceptor[] globalInterceptors)
        {
            IUnityContainer unityContainer = new UnityContainer();
            ByConventionRegistrator.RegisterTypesByConvention(unityContainer);
            return InterceptionHelper.InterceptContainer(unityContainer, globalInterceptors);
        }
    }

    
}
