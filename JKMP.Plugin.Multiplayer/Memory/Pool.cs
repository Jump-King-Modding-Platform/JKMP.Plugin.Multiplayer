using System;
using System.Collections.Generic;

namespace JKMP.Plugin.Multiplayer.Memory
{
    public interface IPoolable
    {
        void Reset();
        void OnSpawned();
    }
    
    public static class Pool
    {
        private static readonly Dictionary<Type, Stack<object>> Instances = new();

        public static T Get<T>() where T : IPoolable, new() => Get<T>(typeof(T));

        public static TBaseType Get<TBaseType>(Type type) where TBaseType : IPoolable
        {
            if (!typeof(TBaseType).IsAssignableFrom(type))
                throw new ArgumentException($"{type} is not assignable to {typeof(TBaseType)}");
            
            var stack = GetOrAddStack(type);
            
            if (stack.Count == 0)
            {
                var instance = (TBaseType)Activator.CreateInstance(type);
                return instance;
            }
            else
            {
                var instance = (TBaseType)stack.Pop();
                return instance;
            }
        }

        public static void Release<T>(T instance) where T : IPoolable
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            var stack = GetOrAddStack(instance.GetType());
            instance.Reset();
            stack.Push(instance);
        }

        public static void Release(object instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            if (instance is not IPoolable poolable)
                throw new ArgumentException($"{instance.GetType()} is not assignable to {typeof(IPoolable)}");
            
            var stack = GetOrAddStack(poolable.GetType());
            poolable.Reset();
            stack.Push(poolable);
        }

        private static Stack<object> GetOrAddStack(Type type)
        {
            if (Instances.TryGetValue(type, out var stack))
            {
                return stack;
            }

            stack = new();
            Instances.Add(type, stack);
            return stack;
        }
    }
}