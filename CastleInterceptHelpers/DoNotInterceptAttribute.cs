using System;
using System.Collections.Generic;
using System.Text;

namespace CastleInterceptHelpers
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public sealed class DoNotInterceptAttribute : Attribute
    {

    }
}
