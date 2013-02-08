#region

using System;
using System.Linq.Expressions;

#endregion

namespace Vermeil.State
{
    public interface IStateManager
    {
        void SaveState(Expression<Func<object>> property, object value);
        void SaveState(string key, object state);
        object LoadState(Expression<Func<object>> property, object defaultValue = null);
        object LoadState(string key, object defaultValue = null);
        void Clear(string key);
        void Clear(Expression<Func<object>> property);
        void ClearAll();
    }
}