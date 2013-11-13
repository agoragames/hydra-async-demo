using System;
using System.Collections.Generic;
using System.Net;
using System.IO;

namespace AgoraGames.Hydra
{
    public class BlockingRunner : Runner
    {
        // TODO: we may need to take a timeout for each reqeust to take...
        public BlockingRunner()
        {
        }

        public void DoRequest(Client client, Request request)
        {
            client.Transport.DoRequest(client, request);
            
            request.NotifyComplete();
        }

        public bool WaitForAll(int timeout)
        {
            // nothing to do...
            return true;
        }
    }
}
