#region

using System;
using System.Linq.Expressions;

#endregion

namespace Vermeil.Mapping
{
    public class MapConverter<T>
    {
        public MapConverter(Expression<Func<T, object>> selector, Func<object, object> converter)
        {
            Selector = selector;
            Converter = converter;
        }

        public Expression<Func<T, object>> Selector { get; private set; }
        public Func<object, object> Converter { get; private set; }
    }
}
