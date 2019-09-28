using System;
using System.Collections.Generic;
using System.Text;

namespace CastleInterceptHelpers
{
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class RemoveGlobalInterceptorAttribute : Attribute
    {
        public Type GlobalInterceptorToRemove { get; }
        public RemoveGlobalInterceptorAttribute(Type globalInterceptorToRemove)
        {
            GlobalInterceptorToRemove = globalInterceptorToRemove;
        }
    }
}
