using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Threading;

namespace AgoraGames.Hydra
{
    public class PollingRunner : ThreadedRunner
    {
        protected Queue<Request> completedRequets = new Queue<Request>();
        protected int counter = 0;

        public PollingRunner()
        {
        }

        public void Poll()
        {
            // we are trying to make this fast, if we lock each frame then we are just doing too much work
            if (Interlocked.CompareExchange(ref counter, 0, 1) == 1)
            {
                lock (completedRequets)
                {
                    while (completedRequets.Count > 0)
                    {
                        Request req = completedRequets.Dequeue();

                        req.NotifyComplete();
                    }
                }
            }
        }

        protected override bool IsComplete()
        {
            if (!base.IsComplete())
            {
                return false;
            }

            lock (completedRequets)
            {
                return completedRequets.Count == 0;
            }
        }

        protected override void NotifyComplete(Request request)
        {
            lock (completedRequets)
            {
                completedRequets.Enqueue(request);
                Interlocked.Exchange(ref counter, 1);
            }
        }

        public override bool WaitForAll(int timeout)
        {
            // lets process all of the requests, this works a bit differently
            //  than the base implemenation because of the completed request queue
            join = true;

            // TODO: implement timeout!
            while (!shutdownEvent.WaitOne(0)) 
            {
                Poll();
            }

            return true;
        }
    }
}
