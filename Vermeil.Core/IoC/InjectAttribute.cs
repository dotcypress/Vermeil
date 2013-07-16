#region

using System;

#endregion

namespace Vermeil.Core.IoC
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class InjectAttribute : Attribute
    {
        public InjectAttribute(string key = null)
        {
            Key = key;
        }

        public string Key { get; private set; }
    }
}
