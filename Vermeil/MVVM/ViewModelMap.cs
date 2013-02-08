#region

using System;
using System.Collections.Generic;
using Microsoft.Phone.Controls;

#endregion

namespace Vermeil.MVVM
{
    public class ViewModelMap
    {
        private readonly Dictionary<Type, Type> _map = new Dictionary<Type, Type>();

        public void Register<TPage, TViewModel>()
            where TPage : PhoneApplicationPage
            where TViewModel : ViewModel
        {
            var pageType = typeof (TPage);
            if (_map.ContainsKey(pageType))
            {
                throw new Exception("View model already registered");
            }
            _map.Add(pageType, typeof (TViewModel));
        }

        public Type Resolve(Type pageType)
        {
            return _map.ContainsKey(pageType) ? _map[pageType] : null;
        }
    }
}