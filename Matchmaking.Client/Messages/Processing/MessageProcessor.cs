using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Matchmaking.Client.Messages.Processing
{
    public class MessageProcessor<TMessage, TContext>
    {
        private readonly Dictionary<Type, List<IMessageHandler<TMessage, TContext>>> handlers = new();

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