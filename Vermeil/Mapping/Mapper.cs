#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Vermeil.Core;

#endregion

namespace Vermeil.Mapping
{
    public static class Mapper
    {
        private static readonly List<MapHolder> Holders = new List<MapHolder>();

        public static void CreateMap<T, T1>(params MapConverter<T>[] converters)
        {
            var source = GetMembers<T>();
            var destination = GetMembers<T1>();
            var holder = new MapHolder {From = typeof (T), To = typeof (T1)};
            foreach (var memberInfo in source)
            {
                var destinationInfo = destination.FirstOrDefault(x => x.Name == memberInfo.Name);
                if (destinationInfo == null)
                {
                    throw new Exception(string.Format("Can't find destination for property: {0} on type: {1}", memberInfo.Name, typeof (T1).FullName));
                }
                var converter = new Func<object, object>(x => x);
                var converterHolder = converters.FirstOrDefault(x => memberInfo.Name == x.Selector.GetPropertyName());
                var subMap = FindMap(memberInfo.PropertyType, destinationInfo.PropertyType);
                if (converterHolder != null)
                {
                    converter = converterHolder.Converter;
                }
                else if (memberInfo.PropertyType != destinationInfo.PropertyType && subMap == null)
                {
                    throw new Exception(string.Format("Can't find converter for property: {0}.{1}<{2}> to type: <{3}>", memberInfo.DeclaringType, memberInfo.Name, memberInfo.PropertyType, destinationInfo.PropertyType));
                }
                var memberInfoClosure = memberInfo;
                var destinationInfoClosure = destinationInfo;
                holder.Members.Add((x, y) =>
                    {
                        var rawValue = memberInfoClosure.GetValue(x);
                        if (rawValue != null)
                        {
                            if (subMap != null)
                            {
                                destinationInfoClosure.SetValue(y, MapImpl(rawValue, subMap.To));
                                return;
                            }
                        }
                        var value = converter.Invoke(rawValue);
                        destinationInfoClosure.SetValue(y, value);
                    });
            }
            if (Holders.Contains(holder))
            {
                throw new Exception(string.Format("Mapping: {0} -> {1} already register", holder.From.FullName, holder.To.FullName));
            }
            Holders.Add(holder);
        }

        public static TOut Map<TOut>(this object value)
        {
            if (value == null)
            {
                return default(TOut);
            }
            return (TOut) MapImpl(value, typeof (TOut));
        }

        public static IEnumerable<TOut> Map<TOut>(this IEnumerable<object> value)
        {
            if (value == null)
            {
                return new List<TOut>();
            }
            return value.Select(Map<TOut>);
        }

        private static object MapImpl(object value, Type toType)
        {
            var holder = FindMap(value.GetType(), toType);
            if (holder == null)
            {
                throw new Exception(string.Format("Can't find mapping: {0} -> {1}", value.GetType().FullName, toType.FullName));
            }
            var result = Activator.CreateInstance(toType);
            foreach (var member in holder.Members)
            {
                member.Invoke(value, result);
            }
            return result;
        }

        private static MapHolder FindMap(Type fromType, Type toType)
        {
            return Holders.FirstOrDefault(x => x.From == fromType && x.To == toType);
        }

        private static List<PropertyInfo> GetMembers<T>()
        {
            var memberInfos = new List<PropertyInfo>();
            memberInfos.AddRange(typeof (T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.DeclaredOnly));
            return memberInfos;
        }

        private class MapHolder
        {
            public MapHolder()
            {
                Members = new List<Action<object, object>>();
            }

            public Type From { get; set; }
            public Type To { get; set; }
            public List<Action<object, object>> Members { get; private set; }

            private bool Equals(MapHolder other)
            {
                return From == other.From && To == other.To;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }
                if (ReferenceEquals(this, obj))
                {
                    return true;
                }
                if (obj.GetType() != this.GetType())
                {
                    return false;
                }
                return Equals((MapHolder) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((From != null ? From.GetHashCode() : 0)*397) ^ (To != null ? To.GetHashCode() : 0);
                }
            }
        }
    }
}
