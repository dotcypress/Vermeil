#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#endregion

namespace Vermeil.Core.IoC
{
    public class IocContainer
    {
        private const string ConstructorParameterSplitter = ">";
        private const string PropertySplitter = ".";
        private readonly IList<ObjectHolder> _resolverHolders = new List<ObjectHolder>();

        #region Register

        public void Register<T>()
        {
            RegisterCore(typeof (T), typeof (T), null, false);
        }

        public void Register<TFrom, TTo>(string key = null) where TTo : TFrom
        {
            RegisterCore(typeof (TFrom), typeof (TTo), key, false);
        }

        public void Register<T>(Type type, string key = null)
        {
            if (!typeof (T).IsAssignableFrom(type))
            {
                throw new IocException(string.Format("{0} must implement {1}", type.Name, typeof (T).Name));
            }
            RegisterCore(typeof (T), type, key, false);
        }

        public void RegisterInstance<T>()
        {
            RegisterInstance(typeof (T), Instantiate(typeof (T)));
        }

        public void RegisterInstance<T>(T instance, string key = null)
        {
            RegisterInstance(typeof (T), instance, key);
        }

        public void RegisterInstance<TFrom, TTo>(string key = null) where TTo : TFrom
        {
            RegisterInstance(typeof (TFrom), Instantiate(typeof (TTo)));
        }

        public void RegisterInstance(Type type, object instance, string key = null)
        {
            var holder = new ObjectHolder(type, key, () => instance);
            if (_resolverHolders.Contains(holder))
            {
                _resolverHolders.Remove(holder);
            }
            _resolverHolders.Add(holder);
        }

        #endregion

        #region Resolve

        public T Resolve<T>(string key = null)
        {
            return (T) Resolve(typeof (T), key);
        }

        public T TryResolve<T>(string key = null) where T : class
        {
            return (T) TryResolve(typeof (T), key);
        }

        public object Resolve(Type type, string key = null)
        {
            var result = ResolveCore(type, key);
            BuildUp(result);
            return result;
        }

        public object TryResolve(Type type, string key = null)
        {
            try
            {
                return Resolve(type, key);
            }
            catch
            {
                return null;
            }
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            var allObjects = ResolveAll(typeof (T));
            return allObjects.Select(obj => (T) obj);
        }

        public IEnumerable<object> ResolveAll(Type type)
        {
            return _resolverHolders.
                Where(holder => holder.Type == type).
                Select(holder => holder.CreationFunction());
        }

        #endregion

        #region Configuration

        public void DefinePropertyRule<T, TProp>(Expression<Func<T, TProp>> propertyExpression, TProp value)
        {
            var propertyName = typeof (T).FullName + PropertySplitter + propertyExpression.GetPropertyName();
            RegisterInstance(typeof (T), value, propertyName);
        }

        public void DefineContructorParamRule<T>(string name, object value)
        {
            var propertyName = typeof (T).FullName + ConstructorParameterSplitter + name;
            RegisterInstance(typeof (T), value, propertyName);
        }

        #endregion

        #region Building

        public void BuildUp(object instance)
        {
            var propertyInfos = instance.GetType().GetProperties();
            foreach (var propertyInfo in propertyInfos.Where(x => x.GetCustomAttributes(typeof (InjectAttribute), true).Any()))
            {
                var fullPropertyName = instance.GetType().FullName + PropertySplitter + propertyInfo.Name;
                propertyInfo.SetValue(instance,
                    ContainsKey(instance.GetType(), fullPropertyName)
                        ? Resolve(null, fullPropertyName)
                        : Resolve(propertyInfo.PropertyType),
                    null);
            }
        }

        #endregion

        #region Private helper methods

        private void RegisterCore(Type fromType, Type toType, string key, bool isSingleton)
        {
            var holder = new ObjectHolder(fromType, key, () => BuildFromType(toType), isSingleton);
            if (_resolverHolders.Contains(holder))
            {
                _resolverHolders.Remove(holder);
            }
            _resolverHolders.Add(holder);
        }

        private object ResolveCore(Type type, string key)
        {
            if (type == null)
            {
                type = GetTypeFromKey(key);
                if (type == null)
                {
                    throw new IocException("Failed to get type for " + key);
                }
            }
            return ContainsKey(type, key)
                ? _resolverHolders.First(x => x.Type == type && x.Key == key).CreationFunction()
                : BuildFromType(type);
        }

        private bool ContainsKey(Type type, string key)
        {
            return _resolverHolders.Any(x => x.Type == type && x.Key == key);
        }

        private Type GetTypeFromKey(string key)
        {
            var holder = _resolverHolders.FirstOrDefault(x => x.Key == key);
            return holder == null ? null : holder.Type;
        }

        private object BuildFromType(Type type)
        {
            var holder = _resolverHolders.FirstOrDefault(x => x.Key == null && x.Type == type && x.IsSingleton);
            if (holder != null)
            {
                return holder.Instance ?? (holder.Instance = Instantiate(type));
            }
            return Instantiate(type);
        }

        private object Instantiate(Type type)
        {
            var constructor = type.GetConstructors()
                                  .OrderByDescending(c => c.GetParameters().Length)
                                  .FirstOrDefault();
            if (constructor == null)
            {
                throw new IocException("Could not locate a constructor for " + type.FullName);
            }

            var parameterInfos = constructor.GetParameters();
            var constructorParams = new List<object>(parameterInfos.Length);
            constructorParams.AddRange(parameterInfos.
                Select(parameterInfo => new {parameterInfo, fullParamName = type.FullName + ConstructorParameterSplitter + parameterInfo.Name}).
                Select(@t => ContainsKey(type, @t.fullParamName)
                    ? Resolve(null, @t.fullParamName)
                    : Resolve(@t.parameterInfo.ParameterType)));
            try
            {
                return constructor.Invoke(constructorParams.ToArray());
            }
            catch (Exception exception)
            {
                throw new IocException("Failed to resolve " + type.Name, exception);
            }
        }

        #endregion
    }
}
