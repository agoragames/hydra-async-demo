using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AgoraGames.Hydra
{
    public interface Transport
    {
        void DoRequest(Client client, Request request);
    }
}


