using System;
using System.Collections.Generic;

namespace ProgressiveP.Backend
{
   
    public static class ServiceLocatorBackend
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static void Register<T>(T service) where T : class
        {
            _services[typeof(T)] = service;
        }

       
        public static T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
                return (T)service;

            throw new InvalidOperationException(
                $"[ServiceLocatorBackend] '{typeof(T).Name}' is not registered. ");
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out var obj))
            {
                service = (T)obj;
                return true;
            }
            service = null;
            return false;
        }

        
        public static void Clear()
        {
            _services.Clear();
        }
    }
}
