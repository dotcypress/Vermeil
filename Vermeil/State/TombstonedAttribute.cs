#region

using System;

#endregion

namespace Vermeil.State
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class TombstonedAttribute : Attribute
    {
    }
}