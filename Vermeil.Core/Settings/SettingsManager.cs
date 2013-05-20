#region

using System;
using System.IO.IsolatedStorage;
using System.Linq.Expressions;

#endregion

namespace Vermeil.Core.Settings
{
    public class SettingsManager : ISettingsManager
    {
        private readonly IsolatedStorageSettings _isolatedStore;

        public SettingsManager()
        {
            try
            {
                _isolatedStore = IsolatedStorageSettings.ApplicationSettings;
            }
                // ReSharper disable EmptyGeneralCatchClause
            catch
                // ReSharper restore EmptyGeneralCatchClause
            {
            }
        }

        public void Save()
        {
            _isolatedStore.Save();
        }

        public void Clear()
        {
            _isolatedStore.Clear();
        }

        public void SetValue(Expression<Func<object>> property, Object value)
        {
            var key = property.GetPropertyName();
            SetValue(key, value);
        }

        public void SetValue(string key, Object value)
        {
            if (_isolatedStore.Contains(key))
            {
                _isolatedStore[key] = value;
            }
            else
            {
                _isolatedStore.Add(key, value);
            }
        }

        public T GetValue<T>(Expression<Func<object>> property, T defaultValue)
        {
            var key = property.GetPropertyName();
            return GetValue(key, defaultValue);
        }

        public T GetValue<T>(string key, T defaultValue)
        {
            return _isolatedStore.Contains(key) ? (T) _isolatedStore[key] : defaultValue;
        }
    }
}
