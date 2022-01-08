using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Matchmaking.Client.Messages.Processing
{
    public class MessageProcessor<TMessage, TContext>
    {
        private readonly Dictionary<Type, List<IMessageHandler<TMessage, TContext>>> handlers = new();
        private readonly ConcurrentQueue<Tuple<TMessage, TContext>> pendingMessages = new();

        public void RegisterHandler<T2>(IMessageHandler<TMessage, TContext> handler) where T2 : TMessage
        {
            GetHandlers(typeof(T2))!.Add(handler);
        }

        public void RegisterHandler<T2>(Func<T2, TContext, Task> action) where T2 : TMessage
        {
            var actionHandler = new ActionMessageHandler<TMessage, TContext>((message, context) => action((T2)message, context));
            GetHandlers(typeof(T2))!.Add(actionHandler);
        }

        public void RegisterHandler<T2>(IMessageHandler<T2, TContext> handler) where T2 : TMessage
        {
            var converterHandler = new ConverterMessageHandler<T2, TMessage, TContext>(handler);
            GetHandlers(typeof(T2))!.Add(converterHandler);
        }

        /// <summary>
        /// Pushes an incoming message to the pending queue. To handle the queued messages, call <see cref="HandlePendingMessages"/>.
        /// This method is thread safe.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if message or context is null.</exception>
        public void PushMessage(TMessage message, TContext context)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (context == null) throw new ArgumentNullException(nameof(context));

            pendingMessages.Enqueue(new Tuple<TMessage, TContext>(message, context));
        }

        /// <summary>
        /// Handles all pending messages. This method is thread safe but should ideally only be called from one thread that handles the incoming messages.
        /// </summary>
        public async Task HandlePendingMessages()
        {
            while (pendingMessages.Count > 0)
            {
                if (!pendingMessages.TryDequeue(out var tuple))
                    break;
                
                TMessage message = tuple.Item1!;
                TContext context = tuple.Item2!;

                await HandleMessage(message, context);
            }
        }

        /// <summary>
        /// Handles a message instantly without pushing it to the pending queue.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if message or context is null.</exception>
        public async Task HandleMessage(TMessage message, TContext context)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (context == null) throw new ArgumentNullException(nameof(context));
            
            var messageHandlers = GetHandlers(message.GetType());

            if (messageHandlers == null)
                return;
            
            foreach (var handler in messageHandlers)
            {
                await handler.HandleMessage(message, context);
            }
        }

        private List<IMessageHandler<TMessage, TContext>>? GetHandlers(Type type, bool createIfNecessary = true)
        {
            if (!handlers.TryGetValue(type, out var typeHandlers))
            {
                if (!createIfNecessary)
                    return null;
                
                typeHandlers = new List<IMessageHandler<TMessage, TContext>>();
                handlers[type] = typeHandlers;
            }

            return typeHandlers;
        }
    }
}