# CastleInterceptHelpers
Helpers for creating interceptors with castle core and Unity Container

Usage

Global interceptors

``` c#
// Create container
IUnityContainer unityContainer = new UnityContainer();
unityContainer.RegisterType<IMyFooService, MyFooService>(); 
// Call InterceptionHelper.InterceptContainer with your interceptors as arguments. This will return a new child container whose all registration will be intercepted
unityContainer = InterceptionHelper.InterceptContainer(unityContainer, new IInterceptor[] { new MyInterceptor() });
```

Attribute Interceptors

``` c#
[InterceptWith(typeof(MyInterceptor))]  // decorate your class like this
public class MyFooServiceWithAttributeInterceptor : IMyFooService
{
  public void Execute(){ }
}

// Then initialize the interception:

// Create container
IUnityContainer unityContainer = new UnityContainer();
unityContainer.RegisterType<IMyFooService, MyFooService>(); 
// Call InterceptionHelper.InterceptContainer This will return a new child container whose all registration will be intercepted
unityContainer = InterceptionHelper.InterceptContainer(unityContainer, new IInterceptor[] { });  // In this example I'm not passing global interceptors but you can use both global and attribute interceptors

```
