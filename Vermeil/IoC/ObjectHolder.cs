#region

using System;
using System.Collections.Generic;

#endregion

namespace Vermeil.IoC
{
    internal class ObjectHolder : IEquatable<ObjectHolder>
    {
        public ObjectHolder(Type type, string key) : this(type, key, null)
        {
        }

        public ObjectHolder(Type type, string key, Func<object> creationFunction)
            : this(type, key, creationFunction, false)
        {
        }

        public ObjectHolder(Type type, string key, Func<object> creationFunction, bool isSingleton)
        {
            Type = type;
            Key = key;
            CreationFunction = creationFunction;
            IsSingleton = isSingleton;
        }

        public Type Type { get; private set; }

        public string Key { get; private set; }

        public Func<object> CreationFunction { get; private set; }

        public bool IsSingleton { get; set; }

        public object Instance { get; set; }

        public override string ToString()
        {
            return string.Format("({0}:{1})", Type, Key);
        }

        public override int GetHashCode()
        {
            var comparer = EqualityComparer<object>.Default;
            return Extensions.CombineHashCodes(comparer.GetHashCode(Type), comparer.GetHashCode(Key));
        }

        public bool Equals(ObjectHolder other)
        {
            return Equals((object) other);
        }

        public override bool Equals(object obj)
        {
            var t = obj as ObjectHolder;
            var typeComparer = EqualityComparer<Type>.Default;
            var keyComparer = EqualityComparer<string>.Default;
            return t != null && typeComparer.Equals(Type, t.Type) && keyComparer.Equals(Key, t.Key);
        }
    }
}