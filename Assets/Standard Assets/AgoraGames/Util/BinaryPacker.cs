using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace AgoraGames.Hydra.Util
{
    public class BinaryPacker
    {
        public static DateTime EPOCH = new DateTime(1970, 1, 1);

        protected static byte NONE_TYPE = 1;
        protected static byte INT32_TYPE = 2;
        protected static byte BINARY_TYPE = 3;
        protected static byte BOOL_TYPE = 4;
        protected static byte BYTE8_TYPE = 5;
        protected static byte FLOAT64_TYPE = 6;
        protected static byte DATETIME_TYPE = 7;
        protected static byte ARRAY_TYPE = 8;
        protected static byte STRUCT_TYPE = 9;
        protected static byte BITSTRUCT_TYPE = 10;
        protected static byte INT64_TYPE = 11;
        protected static byte MAP_TYPE = 12;
        protected static byte UINT64_TYPE = 13;
        protected static byte UTF8_TYPE = 14;

        public static byte[] encode(object obj)
        {
            MemoryStream stream = new MemoryStream();

            write(stream, obj);
            return stream.ToArray();
        }

        public static object decode(byte[] bytes)
        {
            MemoryStream stream = new MemoryStream(bytes);

            return read(stream);
        }

        public static object decode(Stream stream)
        {
            return read(stream);
        }

        protected static object read(Stream stream)
        {
            int type = stream.ReadByte();

            // EOF
            if (type == -1)
            {
                return null;
            }
            else if (type == NONE_TYPE)
            {
                return null;
            }
            else if (type == INT32_TYPE)
            {
                return readInt32(stream);
            }
            else if (type == BINARY_TYPE)
            {
                return readBinary(stream);
            }
            else if (type == BOOL_TYPE)
            {
                return readByte(stream) == 0 ? false : true;
            }
            else if (type == FLOAT64_TYPE)
            {
                return readDouble(stream);
            }
            else if (type == DATETIME_TYPE)
            {
                return readDateTime(stream);
            }
            else if (type == ARRAY_TYPE)
            {
                List<object> list = new List<object>();
                int count = readInt32(stream);

                for (int i = 0; i < count; i++)
                {
                    object val = read(stream);

                    list.Add(val);
                }
                return list;
            }
            else if (type == MAP_TYPE || type == STRUCT_TYPE)
            {
                Dictionary<object, object> dict = new Dictionary<object, object>();
                int count = readInt32(stream);

                for (int i = 0; i < count; i++)
                {
                    object key = read(stream);
                    object val = read(stream);

                    dict.Add(key, val);
                }
                return dict;
            }
            else if (type == INT64_TYPE)
            {
                return readInt64(stream);
            }
            else if (type == BITSTRUCT_TYPE)
            {
                return readBitArray(stream);
            }
            else if (type == UINT64_TYPE)
            {
                return (ulong)readUInt64(stream);
            }
            else if (type == UTF8_TYPE)
            {
                return readString(stream);
            }
            return null;
        }

        protected static int readInt32(Stream stream)
        {
            byte[] buff = new byte[4];

            stream.Read(buff, 0, 4);
            return BitConverter.ToInt32(convert(buff), 0);
        }

        protected static long readInt64(Stream stream)
        {
            byte[] buff = new byte[8];

            stream.Read(buff, 0, 8);
            return BitConverter.ToInt64(convert(buff), 0);
        }

        protected static ulong readUInt64(Stream stream)
        {
            byte[] buff = new byte[8];

            stream.Read(buff, 0, 8);
            return BitConverter.ToUInt64(convert(buff), 0);
        }

        public static byte[] readBinary(Stream stream)
        {
            int len = readInt32(stream);
            byte[] buff = new byte[len];

            stream.Read(buff, 0, len);
            return buff;
        }

        protected static byte readByte(Stream stream)
        {
            return (byte)stream.ReadByte();
        }

        protected static string readString(Stream stream)
        {
            int len = readInt32(stream);
            byte[] buff = new byte[len];

            stream.Read(buff, 0, len);
            return System.Text.Encoding.UTF8.GetString(buff);
        }

        protected static double readDouble(Stream stream)
        {
            byte[] buff = new byte[8];

            stream.Read(buff, 0, 8);
            return BitConverter.ToDouble(convert(buff), 0);
        }

        protected static DateTime readDateTime(Stream stream)
        {
            int dateTime = readInt32(stream);

            return EPOCH.AddSeconds(dateTime);
        }

        protected static BitArray readBitArray(Stream stream)
        {
            int bits = readInt32(stream);
            BitArray ret = new BitArray(new[] { bits });

            return ret;
        }

        protected static void write(Stream stream, object obj)
        {
            if (obj == null)
            {
                stream.WriteByte(NONE_TYPE);
            }
            else if (obj is string)
            {
                stream.WriteByte(UTF8_TYPE);
                write(stream, (string)obj);
            }
            else if (obj is int || obj is short || obj is ushort || obj is uint)
            {
                stream.WriteByte(INT32_TYPE);
                write(stream, (int)obj);
            }
            else if (obj is long)
            {
                stream.WriteByte(INT64_TYPE);
                write(stream, (long)obj);
            }
            else if (obj is ulong)
            {
                stream.WriteByte(UINT64_TYPE);
                write(stream, (ulong)obj);
            }
            else if (obj is float || obj is double)
            {
                stream.WriteByte(FLOAT64_TYPE);
                write(stream, (double)obj);
            }
            else if (obj is byte[])
            {
                stream.WriteByte(BINARY_TYPE);
                write(stream, (byte[])obj);
            }
            else if (obj is byte || obj is sbyte)
            {
                stream.WriteByte(BYTE8_TYPE);
                write(stream, (byte)obj);
            }
            else if (obj is DateTime)
            {
                stream.WriteByte(DATETIME_TYPE);
                write(stream, (DateTime)obj);
            }
            else if (obj is BitArray)
            {
                stream.WriteByte(BITSTRUCT_TYPE);
                write(stream, (BitArray)obj);
            }
            else if (obj is IDictionary)
            {
                IDictionary dict = (IDictionary)obj;

                stream.WriteByte(MAP_TYPE);
                write(stream, dict.Count);

                foreach (object key in dict.Keys)
                {
                    write(stream, key);
                    write(stream, dict[key]);
                }
            }
            else if (obj is IList)
            {
                IList list = (IList)obj;

                stream.WriteByte(ARRAY_TYPE);
                write(stream, list.Count);
                foreach (object iter in list)
                {
                    write(stream, iter);
                }
            }
            else
            {
                throw new Exception("unknown encode param");
            }
        }

        protected static void write(Stream stream, string d)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(d);

            write(stream, bytes.Length);
            stream.Write(bytes, 0, bytes.Length);
        }

        protected static void write(Stream stream, double d)
        {
            byte[] bytes = convert(BitConverter.GetBytes(d));

            stream.Write(bytes, 0, bytes.Length);
        }

        protected static void write(Stream stream, int i)
        {
            byte[] bytes = convert(BitConverter.GetBytes(i));

            stream.Write(bytes, 0, bytes.Length);
        }

        protected static void write(Stream stream, long i)
        {
            byte[] bytes = convert(BitConverter.GetBytes(i));

            stream.Write(bytes, 0, bytes.Length);
        }

        protected static void write(Stream stream, byte[] bytes)
        {
            write(stream, bytes.Length);
            stream.Write(bytes, 0, bytes.Length);
        }

        protected static void write(Stream stream, DateTime dateTime)
        {
            TimeSpan span = dateTime - EPOCH;

            write(stream, (int)span.TotalSeconds);
        }

        protected static byte[] convert(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }
    }
}
