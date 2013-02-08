#region

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion

namespace Vermeil.State
{
    internal class TombstoneManager
    {
        private readonly IStateManager _stateManager;

        public TombstoneManager(IStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public void SaveState(object viewModel)
        {
            var viewModelType = viewModel.GetType();
            var properties = GetTombstonedProperties(viewModelType);
            properties.ForEach(x =>
                                   {
                                       var value = x.GetValue(viewModel, null);
                                       var key = string.Format("State.{0}.{1}", viewModelType.Name, x.Name);
                                       _stateManager.SaveState(key, value);
                                   });
        }

        public void LoadState(object viewModel)
        {
            var viewModelType = viewModel.GetType();
            var properties = GetTombstonedProperties(viewModelType);
            properties.ForEach(x =>
                                   {
                                       var value = x.GetValue(viewModel, null);
                                       var key = string.Format("State.{0}.{1}", viewModelType.Name, x.Name);
                                       var newValue = _stateManager.LoadState(key, value);
                                       if (value == newValue)
                                       {
                                           return;
                                       }
                                       x.SetValue(viewModel, newValue, null);
                                   });
            Clear();
        }

        private static List<PropertyInfo> GetTombstonedProperties(IReflect viewModelType)
        {
            var propertyInfos = viewModelType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var properties = new List<PropertyInfo>(propertyInfos).Where(x => x.IsDefined(typeof (TombstonedAttribute), true)).ToList();
            return properties;
        }

        public void Clear()
        {
            _stateManager.ClearAll();
        }
    }
}