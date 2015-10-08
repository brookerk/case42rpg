using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Case42.Server
{
    public class Registry
    {
        private readonly Dictionary<Type, object> _components;

        //prevent multiple thread access
        private readonly Dictionary<Type, object> _componentLocks;

        public Registry()
        {
            _components = new Dictionary<Type, object>();
            _componentLocks = new Dictionary<Type, object>();
        }

        public bool Has<TComponent>()
        {
            return _components.ContainsKey(typeof(TComponent));
        }

        public void Remove<TComponent>()
        {
            _components.Remove(typeof(TComponent));
            _componentLocks.Remove(typeof(TComponent));
        }

        public void Set<TComponent>(TComponent component)
        {
            _components[typeof(TComponent)] = component;
            _componentLocks[typeof(TComponent)] = new object();
        }

        public void Get<TComponent>(Action<TComponent> action)
        {
            lock (_componentLocks[typeof(TComponent)])
                action((TComponent)_components[typeof(TComponent)]);
        }

        public TResult Get<TComponent, TResult>(Func<TComponent, TResult> func)
        {
            lock (_componentLocks[typeof(TComponent)])
                return func((TComponent)_components[typeof(TComponent)]);
        }

        public void TryGet<TComponent>(Action<TComponent> action)
        {
            object componentLock;
            if (!_componentLocks.TryGetValue(typeof(TComponent), out componentLock))
                return;

            lock (componentLock)
                action((TComponent)_components[typeof(TComponent)]);
        }

        public TResult TryGet<TComponent, TResult>(Func<TComponent, TResult> func)
        {
            object componentLock;
            if (!_componentLocks.TryGetValue(typeof(TComponent), out componentLock))
                return default(TResult);

            lock (componentLock)
                return func((TComponent)_components[typeof(TComponent)]);
        }

    }
}

