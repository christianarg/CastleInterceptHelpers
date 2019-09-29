using System;

namespace CastleInterceptHelpers
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class InterceptWithAttribute : Attribute
    {
        public Type Interceptor { get; }
        public int Order { get; }

        public InterceptWithAttribute(Type interceptor, int order = 0)
        {
            Interceptor = interceptor;
            Order = order;
        }
    }
}
