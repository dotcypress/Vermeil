#region

using System;

#endregion

namespace Vermeil.IoC
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class InjectAttribute : Attribute
    {
    }
}