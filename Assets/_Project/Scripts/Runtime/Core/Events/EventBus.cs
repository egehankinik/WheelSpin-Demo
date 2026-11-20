using System;
using System.Collections.Generic;

namespace WheelOfFortune.Core.Events
{
    public sealed class EventBus : IEventBus
    {
        private readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>();

        public void Subscribe<T>(Action<T> handler) where T : struct, IGameEvent
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _handlers.TryGetValue(typeof(T), out var existing);
            _handlers[typeof(T)] = Delegate.Combine(existing, handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : struct, IGameEvent
        {
            if (handler == null || !_handlers.TryGetValue(typeof(T), out var existing))
            {
                return;
            }

            var remaining = Delegate.Remove(existing, handler);
            if (remaining == null)
            {
                _handlers.Remove(typeof(T));
                return;
            }

            _handlers[typeof(T)] = remaining;
        }

        public void Publish<T>(T gameEvent) where T : struct, IGameEvent
        {
            if (_handlers.TryGetValue(typeof(T), out var handler))
            {
                ((Action<T>)handler).Invoke(gameEvent);
            }
        }
    }
}
