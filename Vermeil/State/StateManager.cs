#region

using System;
using System.Linq.Expressions;
using Microsoft.Phone.Shell;
using Vermeil.Core;

#endregion

namespace Vermeil.State
{
    internal class StateManager : IStateManager
    {
        private readonly PhoneApplicationService _applicationService;

        public StateManager(PhoneApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        public void SaveState(Expression<Func<object>> property, object value)
        {
            SaveState(property.GetPropertyName(), value);
        }

        public void SaveState(string key, object state)
        {
            if (!_applicationService.State.ContainsKey(key))
            {
                _applicationService.State.Add(key, state);
            }
            _applicationService.State[key] = state;
        }

        public object LoadState(Expression<Func<object>> property, object defaultValue = null)
        {
            var key = property.GetPropertyName();
            return LoadState(key, defaultValue);
        }

        public object LoadState(string key, object defaultValue = null)
        {
            if (_applicationService.State.ContainsKey(key))
            {
                return _applicationService.State[key];
            }
            return defaultValue;
        }

        public void Clear(string key)
        {
            if (_applicationService.State.ContainsKey(key))
            {
                _applicationService.State.Remove(key);
            }
        }

        public void Clear(Expression<Func<object>> property)
        {
            var key = property.GetPropertyName();
            Clear(key);
        }

        public void ClearAll()
        {
            _applicationService.State.Clear();
        }
    }
}
