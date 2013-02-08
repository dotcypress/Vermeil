namespace Vermeil.Messaging
{
    public class Message<T> : BaseMessage
    {
        public Message(T content) : base(null)
        {
            Content = content;
        }

        public Message(object sender, T content)
            : base(sender)
        {
            Sender = sender;
            Content = content;
        }

        public T Content { get; private set; }
    }
}