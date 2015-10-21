using System;

namespace mspec_light
{
    [AttributeUsage(AttributeTargets.Delegate, Inherited = false, AllowMultiple = false)]
    public sealed class CleanupDelegateAttribute : Attribute
    {
    }
}