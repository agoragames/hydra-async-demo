using System;
using System.Collections.Generic;
using System.Threading;
using AgoraGames.Hydra.IO;

namespace AgoraGames.Hydra.Util
{
    public class Dispatcher<T>
    {
        public delegate void MessageHandler(Message<T> message);

        protected Client client = null;
        protected List<Message<T>> list = new List<Message<T>>();

        protected FastEvent check = new FastEvent();
        protected MessageHandler messageHandler = null;

        public Dispatcher(Client client, MessageHandler messageHandler)
        {
            this.client = client;
            this.messageHandler = messageHandler;
        }

        public void Add(Message<T> message)
        {
            lock (list)
            {
                list.Add(message);
                check.Set();
            }
        }

        public void Process()
        {
            // we are trying to make this fast, if we lock each frame then we are just doing too much work
            if (check.Check())
            {
                lock (list)
                {
                    // TODO: do we want to allow the ability to only process X number of mesages each
                    //  pass of the gameloop?
                    while (list.Count > 0)
                    {
                        Message<T> message = list[0];

                        list.RemoveAt(0);
                        messageHandler(message);
                    }
                }
            }
        }
    }
}
