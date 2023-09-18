using System;

namespace DJM.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class InjectAttribute : Attribute { }
}