// TODO: have a better way to define this....
#if UNITY_IPHONE
#define MANUALEVENT
#else
#define INTERLOCKED
#endif

using System;
using System.Collections.Generic;
using System.Threading;

namespace AgoraGames.Hydra.Util
{
    public class FastEvent
    {
#if INTERLOCKED
        protected int counter = 0;
#elif MANUALEVENT
        protected AutoResetEvent manualResetEvent = new AutoResetEvent(false);
#endif

        public void Set() 
        {
#if INTERLOCKED
            Interlocked.Exchange(ref counter, 1);
#elif MANUALEVENT
            manualResetEvent.Set();
#endif
        }

        public bool Check() 
        {
#if INTERLOCKED
            // we are trying to make this fast, if we lock each frame then we are just doing too much work
            if (Interlocked.CompareExchange(ref counter, 0, 1) == 1)
            {
                return true;
            }
#elif MANUALEVENT
            if (manualResetEvent.WaitOne(0, false))
            {
                return true;
            }
#endif
            return false;
        }

    }
}
