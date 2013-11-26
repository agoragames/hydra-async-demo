using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AgoraGames.Hydra
{
    public interface Runner
    {
        void DoRequest(Client client, Request request);
        bool WaitForAll(int timeout);
    }
}
