namespace Vermeil.Messaging
{
// ReSharper disable TypeParameterCanBeVariant
    public interface IMessageReceiver<T> where T : BaseMessage
// ReSharper restore TypeParameterCanBeVariant
    {
        void OnReceive(T message);
    }
}