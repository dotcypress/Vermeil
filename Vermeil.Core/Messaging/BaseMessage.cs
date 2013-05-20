namespace Vermeil.Core.Messaging
{
    public abstract class BaseMessage
    {
        protected BaseMessage(object sender)
        {
            Sender = sender;
        }

        protected object Sender { get; set; }
    }
}
