using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Threading;

namespace AgoraGames.Hydra
{
    public class ThreadedRunner : Runner
    {
        protected Thread thread;

        protected EventWaitHandle waitEvent = new AutoResetEvent(false);
        protected ManualResetEvent shutdownEvent = new ManualResetEvent(false);

        protected Queue<Request> requests = new Queue<Request>();

        protected bool join = false;

        public ThreadedRunner()
        {
            this.thread = new Thread(new ThreadStart(Run));
            this.thread.Start();
        }

        public int GetRequestCount()
        {
            lock (requests)
            {
                return requests.Count;
            }
        }

        public void Run()
        {
            bool shutdown = false;

            while (!shutdown)
            {
                Request currentRequest = null;

                lock (requests)
                {
                    if (requests.Count > 0)
                    {
                        currentRequest = requests.Dequeue();
                    }
                }

                if (currentRequest != null)
                {
                    Client client = currentRequest.Client;

                    client.Transport.DoRequest(client, currentRequest);

                    NotifyComplete(currentRequest);
                }
                else
                {
                    try
                    {
                        waitEvent.WaitOne();
                    }
                    catch (ThreadInterruptedException)
                    {
                        shutdown = true;
                    }
                }

                // if we're requested to join, we want to give the NotifyComplete callback a chance
                //  to run and see if it wants to process another request before we say we're all done
                if (join && IsComplete())
                {
                    shutdown = true;
                }
            }
            
            shutdownEvent.Set();
        }

        protected virtual void NotifyComplete(Request request) 
        {
            // by default we're going to notify in the woker thread... subclass for different implemenation
            request.NotifyComplete();
        }

        protected virtual bool IsComplete()
        {
            lock (requests)
            {
                return requests.Count == 0;
            }
        }

        public void DoRequest(Client client, Request request)
        {
            // TODO: add to queue
            lock (requests)
            {
                requests.Enqueue(request);
            }
            waitEvent.Set();
        }

        public virtual bool WaitForAll(int timeout)
        {
            join = true;
            waitEvent.Set();

            // block until we have processed everything
            shutdownEvent.WaitOne(timeout);

            thread.Interrupt();
            return IsComplete();
        }
    }
}
