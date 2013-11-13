using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AgoraGames.Hydra.IO
{
    public interface MessageReader<T>
    {
        Message<T> Read(MessageSerializerRegistry<T> r, int type, Stream s);
    }
}
