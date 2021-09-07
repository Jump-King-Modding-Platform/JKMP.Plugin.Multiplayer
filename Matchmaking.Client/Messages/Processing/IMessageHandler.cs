using System;
using System.Threading.Tasks;

namespace Matchmaking.Client.Messages.Processing
{
    public interface IMessageHandler<in TMessage, in TContext>
    {
        Task HandleMessage(TMessage message, TContext context);
    }

    internal class ActionMessageHandler<TMessage, TContext> : IMessageHandler<TMessage, TContext>
    {
        private readonly Func<TMessage, TContext, Task> action;

        public ActionMessageHandler(Func<TMessage, TContext, Task> action)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
        }

        public Task HandleMessage(TMessage message, TContext context)
        {
            return action(message, context);
        }
    }

    internal class ConverterMessageHandler<TMessage, TBaseMessage, TContext> : IMessageHandler<TBaseMessage, TContext> where TMessage : TBaseMessage
    {
        private readonly IMessageHandler<TMessage, TContext> handler;

        public ConverterMessageHandler(IMessageHandler<TMessage, TContext> handler)
        {
            this.handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public Task HandleMessage(TBaseMessage message, TContext context)
        {
            return handler.HandleMessage((TMessage)message, context);
        }
    }
}