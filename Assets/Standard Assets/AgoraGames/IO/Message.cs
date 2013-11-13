using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AgoraGames.Hydra.IO
{
    public abstract class MessageBase
    {
    }

    public abstract class Message<T> : MessageBase
    {
        public abstract T GetMessageType();
    }
}
