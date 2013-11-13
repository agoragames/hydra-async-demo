using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AgoraGames.Hydra.IO
{
    public class MessageSerializerRegistry<T>
    {
        Dictionary<T, MessageWriter<T>> writers = new Dictionary<T, MessageWriter<T>>();
        Dictionary<T, MessageReader<T>> readers = new Dictionary<T, MessageReader<T>>();

        public MessageSerializerRegistry()
        {
        }

        public void RegisterWriter(T t, MessageWriter<T> s)
        {
            writers[t] = s;
        }

        public void RegisterReader(T t, MessageReader<T> s)
        {
            readers[t] = s;
        }

        public Message<T> Read(Stream s)
        {
            int b = s.ReadByte();
            T t = (T)Enum.ToObject(typeof(T), b);
            MessageReader<T> serializer = readers[t];

            return serializer.Read(this, b, s);
        }

        public Stream Write(Message<T> m)
        {
            MemoryStream stream = new MemoryStream();

            Write(m, stream);

            return stream;
        }

        public Message<T> FromBytes(byte[] bytes)
        {
            MemoryStream s = new MemoryStream(bytes);

            return Read(s);
        }

        public byte[] ToBytes(Message<T> m)
        {
            MemoryStream s = new MemoryStream();

            Write(m, s);
            return s.ToArray();
        }

        public void Write(Message<T> m, Stream s)
        {
            T type = m.GetMessageType();
            MessageWriter<T> serializer = writers[type];
            
            s.WriteByte((byte)Convert.ToInt16(type));
            serializer.Write(this, s, m);
        }
    }
}
