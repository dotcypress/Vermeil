namespace Vermeil.Messaging
{
    public interface IMessagePublisher
    {
        bool IsRegistered<T>(IMessageReceiver<T> receiver) where T : BaseMessage;

        void RegisterWeak<T>(IMessageReceiver<T> receiver) where T : BaseMessage;

        void Register<T>(IMessageReceiver<T> receiver) where T : BaseMessage;

        void Unregister<T>(IMessageReceiver<T> receiver) where T : BaseMessage;

        void Publish<T>(T messageContent) where T : BaseMessage;
    }
}
