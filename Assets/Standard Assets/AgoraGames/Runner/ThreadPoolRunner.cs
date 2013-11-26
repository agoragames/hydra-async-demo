using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AgoraGames.Hydra
{
    public class ThreadPoolRunner : Runner
    {
        bool trackComplete = false;

        bool newRequets = false;
        Dictionary<Request, ManualResetEvent> handles = new Dictionary<Request, ManualResetEvent>();

        public ThreadPoolRunner(bool trackComplete)
        {
            this.trackComplete = trackComplete;
        }

        public void DoRequest(Client client, Request request)
        {
            trackRequest(request);

            ThreadPool.QueueUserWorkItem(delegate(object state)
            {
                client.Transport.DoRequest(client, request);
                request.NotifyComplete();

                markComplete(request);
            });
        }

        protected void trackRequest(Request request)
        {
            if (trackComplete)
            {
                lock (handles)
                {
                    newRequets = true;
                    handles[request] = new ManualResetEvent(false);
                }
            }
        }

        protected void markComplete(Request request)
        {
            if (trackComplete)
            {
                lock (handles)
                {
                    ManualResetEvent e;

                    handles.TryGetValue(request, out e);

                    if (e != null)
                    {
                        e.Set();
                    }
                }
            }
        }

        protected bool hasNewRequests()
        {
            lock (handles)
            {
                return newRequets;
            }
        }

        public bool WaitForAll(int timeout)
        {
            if (trackComplete)
            {
                bool complete = false;
                while (!complete)
                {
                    ManualResetEvent[] waitHandles;
                    lock (handles)
                    {
                        newRequets = false;
                        waitHandles = handles.Values.ToArray();
                    }

                    WaitHandle.WaitAll(waitHandles);

                    complete = !hasNewRequests();
                }
            }
            return true;
        }
    }
}
