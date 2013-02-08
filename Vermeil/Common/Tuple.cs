#region

using System;
using System.Collections.Generic;

#endregion

namespace Vermeil.Common
{
    public class Tuple<T1, T2> : IEquatable<Tuple<T1, T2>>
    {
        public Tuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        public T1 Item1 { get; private set; }

        public T2 Item2 { get; private set; }

        public override string ToString()
        {
            return string.Format("({0},{1})", Item1, Item2);
        }

        public override int GetHashCode()
        {
            var comparer = EqualityComparer<object>.Default;
            return Extensions.CombineHashCodes(comparer.GetHashCode(Item1), comparer.GetHashCode(Item2));
        }

        public bool Equals(Tuple<T1, T2> other)
        {
            return Equals((object) other);
        }

        public override bool Equals(object obj)
        {
            var t = obj as Tuple<T1, T2>;
            var comparerT1 = EqualityComparer<T1>.Default;
            var comparerT2 = EqualityComparer<T2>.Default;
            return t != null && comparerT1.Equals(Item1, t.Item1) && comparerT2.Equals(Item2, t.Item2);
        }
    }


    public class Tuple<T1, T2, T3> : IEquatable<Tuple<T1, T2, T3>>
    {
        public Tuple(T1 item1, T2 item2, T3 item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }

        public T1 Item1 { get; private set; }

        public T2 Item2 { get; private set; }

        public T3 Item3 { get; private set; }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", Item1, Item2, Item3);
        }

        public override int GetHashCode()
        {
            var comparer = EqualityComparer<object>.Default;
            return Extensions.CombineHashCodes(Extensions.CombineHashCodes(comparer.GetHashCode(Item1), comparer.GetHashCode(Item2)), comparer.GetHashCode(Item3));
        }

        public override bool Equals(object obj)
        {
            var t = obj as Tuple<T1, T2, T3>;
            var comparerT1 = EqualityComparer<T1>.Default;
            var comparerT2 = EqualityComparer<T2>.Default;
            var comparerT3 = EqualityComparer<T3>.Default;
            return t != null && comparerT1.Equals(Item1, t.Item1)
                   && comparerT2.Equals(Item2, t.Item2)
                   && comparerT3.Equals(Item3, t.Item3);
        }

        public bool Equals(Tuple<T1, T2, T3> other)
        {
            return Equals((object) other);
        }
    }
}