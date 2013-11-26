using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AgoraGames.Hydra.IO
{
    public interface MessageWriter<T>
    {
        void Write(MessageSerializerRegistry<T> r, Stream s, Message<T> m);
    }
}
