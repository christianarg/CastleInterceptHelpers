using System;

namespace CastleInterceptHelpers
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class InterceptWithAttribute : Attribute
    {
        public Type Interceptor { get; }

        public InterceptWithAttribute(Type interceptor)
        {
            Interceptor = interceptor;
        }
    }
}
