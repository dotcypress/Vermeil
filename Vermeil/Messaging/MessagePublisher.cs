#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Vermeil.Messaging
{
    internal class MessagePublisher : IMessagePublisher
    {
        private readonly Dictionary<Type, List<object>> _receivers = new Dictionary<Type, List<object>>();
        private readonly Dictionary<Type, List<WeakReference>> _weakReceivers = new Dictionary<Type, List<WeakReference>>();

        private readonly object _syncLock = new object();

        public bool IsRegistered<T>(IMessageReceiver<T> receiver) where T : BaseMessage
        {
            lock (_syncLock)
            {
                var recieverMessageType = typeof (T);
                if (!_receivers.ContainsKey(recieverMessageType))
                {
                    _receivers.Add(recieverMessageType, new List<object>());
                }
                var messageReceivers = _receivers[recieverMessageType];
                return messageReceivers.Contains(receiver);
            }
        }

        public void RegisterWeak<T>(IMessageReceiver<T> newReceiver) where T : BaseMessage
        {
            ClearWeakReceivers();
            var receiver = new WeakReference(newReceiver);
            var recieverMessageType = typeof (T);
            lock (_syncLock)
            {
                if (!_weakReceivers.ContainsKey(recieverMessageType))
                {
                    _weakReceivers.Add(recieverMessageType, new List<WeakReference>());
                }

                var messageReceivers = _weakReceivers[recieverMessageType];
                if (messageReceivers.Any(x => x.Target == newReceiver))
                {
                    return;
                }
                messageReceivers.Add(receiver);
            }
        }

        public void Register<T>(IMessageReceiver<T> receiver) where T : BaseMessage
        {
            lock (_syncLock)
            {
                var recieverMessageType = typeof (T);
                if (!_receivers.ContainsKey(recieverMessageType))
                {
                    _receivers.Add(recieverMessageType, new List<object>());
                }
                var messageReceivers = _receivers[recieverMessageType];
                if (messageReceivers.Contains(receiver))
                {
                    throw new Exception("Receiver already registered");
                }
                messageReceivers.Add(receiver);
            }
        }

        public void Unregister<T>(IMessageReceiver<T> receiver) where T : BaseMessage
        {
            lock (_syncLock)
            {
                var recieverMessageType = typeof (T);
                if (!_receivers.ContainsKey(recieverMessageType))
                {
                    return;
                }
                var messageReceivers = _receivers[recieverMessageType];
                if (messageReceivers.Contains(receiver))
                {
                    messageReceivers.Remove(receiver);
                }
                else
                {
                    throw new Exception("Receiver not registered");
                }
            }
        }

        public void Publish<T>(T message) where T : BaseMessage
        {
            lock (_syncLock)
            {
                var recieverMessageType = message.GetType();
                if (_receivers.ContainsKey(recieverMessageType))
                {
                    _receivers[recieverMessageType].ForEach(x =>
                                                                {
                                                                    var reciever = (IMessageReceiver<T>) x;
                                                                    reciever.OnReceive(message);
                                                                });
                }

                if (_weakReceivers.ContainsKey(recieverMessageType))
                {
                    _weakReceivers[recieverMessageType].ForEach(x =>
                                                                    {
                                                                        if (!x.IsAlive)
                                                                        {
                                                                            return;
                                                                        }
                                                                        var receiver = ((IMessageReceiver<T>) x.Target);
                                                                        receiver.OnReceive(message);
                                                                    });
                }
            }
        }

        private void ClearWeakReceivers()
        {
            lock (_syncLock)
            {
                foreach (var recievers in _weakReceivers.Values)
                {
                    for (var pos = recievers.Count - 1; pos >= 0; pos--)
                    {
                        var receiver = recievers[pos];
                        if (!receiver.IsAlive)
                        {
                            recievers.RemoveAt(pos);
                        }
                    }
                }
            }
        }
    }
}