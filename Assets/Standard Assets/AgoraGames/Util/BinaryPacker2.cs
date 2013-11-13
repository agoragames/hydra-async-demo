using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace AgoraGames.Hydra.Util
{
    public class BinaryPacker2
    {
        public static DateTime EPOCH = new DateTime(1970, 1, 1);

        public enum Types
        {
            TYPE_NONE = 0x01,
            TYPE_TRUE = 0x02,
            TYPE_FALSE = 0x03,

            TYPE_INT_8 = 0x10,
            TYPE_UINT_8 = 0x11,
            TYPE_INT_16 = 0x12,
            TYPE_UINT_16 = 0x13,
            TYPE_INT_32 = 0x14,
            TYPE_UINT_32 = 0x15,
            TYPE_INT_64 = 0x16,
            TYPE_UINT_64 = 0x17,

            TYPE_FLOAT = 0x20,
            TYPE_DOUBLE = 0x21,

            TYPE_STRING_8 = 0x30,
            TYPE_STRING_16 = 0x31,
            TYPE_STRING_32 = 0x32,

            TYPE_BINARY_8 = 0x33,
            TYPE_BINARY_16 = 0x34,
            TYPE_BINARY_32 = 0x35,

            TYPE_DATETIME = 0x40,

            TYPE_ARRAY_8 = 0x50,
            TYPE_ARRAY_16 = 0x51,
            TYPE_ARRAY_32 = 0x52,

            TYPE_MAP_8 = 0x60,
            TYPE_MAP_16 = 0x61,
            TYPE_MAP_32 = 0x62,
        };

        protected byte[] buff = new byte[8];

        public static byte[] encode(object obj)
        {
            MemoryStream stream = new MemoryStream();

            write(stream, obj);
            return stream.ToArray();
        }

        public static object decode(byte[] bytes)
        {
            BinaryPacker2 instance = new BinaryPacker2();
            MemoryStream stream = new MemoryStream(bytes);

            return instance.read(stream);
        }

        public static object decode(Stream stream)
        {
            BinaryPacker2 instance = new BinaryPacker2();
            
            return instance.read(stream);
        }

        protected object read(Stream stream)
        {
            int typeByte = stream.ReadByte();

            // EOF, TODO: should we check this before we call read?
            if (typeByte == -1)
            {
                return null;
            }
            else
            {
                Types type = (Types)Enum.ToObject(typeof(Types), typeByte);

                if (Enum.IsDefined(typeof(Types), type))
                {
                    switch (type)
                    {
                        case Types.TYPE_NONE:
                            return null;
                        case Types.TYPE_TRUE:
                            return true;
                        case Types.TYPE_FALSE:
                            return false;

                        // we always want to return longs so that its eaier to code
                        //  against, we don't need tiny values at runtime just on the wire
                        case Types.TYPE_INT_8:
                            return (long)readInt8(stream);
                        case Types.TYPE_UINT_8:
                            return (long)readUInt8(stream);
                        case Types.TYPE_INT_16:
                            return (long)readInt16(stream);
                        case Types.TYPE_UINT_16:
                            return (long)readUInt16(stream);
                        case Types.TYPE_INT_32:
                            return (long)readInt32(stream);
                        case Types.TYPE_UINT_32:
                            return (long)readUInt32(stream);
                        case Types.TYPE_INT_64:
                            return (long)readInt64(stream);
                        case Types.TYPE_UINT_64:
                            return readUInt64(stream);

                        case Types.TYPE_FLOAT:
                            return readFloat(stream);
                        case Types.TYPE_DOUBLE:
                            return readDouble(stream);

                        case Types.TYPE_DATETIME:
                            return readDateTime(stream);

                        case Types.TYPE_STRING_8:
                            return readString8(stream);
                        case Types.TYPE_STRING_16:
                            return readString16(stream);
                        case Types.TYPE_STRING_32:
                            return readString32(stream);

                        case Types.TYPE_BINARY_8:
                            return readBinary8(stream);
                        case Types.TYPE_BINARY_16:
                            return readBinary16(stream);
                        case Types.TYPE_BINARY_32:
                            return readBinary32(stream);

                        case Types.TYPE_ARRAY_8:
                            return readList8(stream);
                        case Types.TYPE_ARRAY_16:
                            return readList16(stream);
                        case Types.TYPE_ARRAY_32:
                            return readList32(stream);

                        case Types.TYPE_MAP_8:
                            return readMap8(stream);
                        case Types.TYPE_MAP_16:
                            return readMap16(stream);
                        case Types.TYPE_MAP_32:
                            return readMap32(stream);
                    }
                }
            }

            return null;
        }

        protected sbyte readInt8(Stream stream)
        {
            return (sbyte)stream.ReadByte();
        }

        protected byte readUInt8(Stream stream)
        {
            return (byte)stream.ReadByte();
        }

        protected short readInt16(Stream stream)
        {
            stream.Read(buff, 0, 2);
            return BitConverter.ToInt16(convert(buff, 2), 0);
        }

        protected ushort readUInt16(Stream stream)
        {
            stream.Read(buff, 0, 2);
            return BitConverter.ToUInt16(convert(buff, 2), 0);
        }

        protected int readInt32(Stream stream)
        {
            stream.Read(buff, 0, 4);
            return BitConverter.ToInt32(convert(buff, 4), 0);
        }

        protected uint readUInt32(Stream stream)
        {
            stream.Read(buff, 0, 4);
            return BitConverter.ToUInt32(convert(buff, 4), 0);
        }

        protected long readInt64(Stream stream)
        {
            stream.Read(buff, 0, 8);
            return BitConverter.ToInt64(convert(buff, 8), 0);
        }

        protected ulong readUInt64(Stream stream)
        {
            stream.Read(buff, 0, 8);
            return BitConverter.ToUInt64(convert(buff, 8), 0);
        }

        public byte[] readBinary8(Stream stream) 
        {
            int len = readUInt8(stream);

            return readBinary(stream, len);
        }

        public byte[] readBinary16(Stream stream) 
        {
            int len = readUInt16(stream);

            return readBinary(stream, len);
        }

        public byte[] readBinary32(Stream stream)
        {
            int len = readInt32(stream);

            return readBinary(stream, len);
        }

        public byte[] readBinary(Stream stream, int len)
        {
            byte[] buff = new byte[len];

            stream.Read(buff, 0, len);
            return buff;
        }

        protected string readString8(Stream stream)
        {
            int len = readUInt8(stream);

            return readString(stream, len);
        }

        protected string readString16(Stream stream)
        {
            int len = readUInt16(stream);

            return readString(stream, len);
        }

        protected string readString32(Stream stream)
        {
            int len = readInt32(stream);

            return readString(stream, len);
        }

        protected string readString(Stream stream, int len)
        {
            byte[] buff = new byte[len];

            stream.Read(buff, 0, len);
            return System.Text.Encoding.UTF8.GetString(buff);
        }

        protected float readFloat(Stream stream)
        {
            stream.Read(buff, 0, 4);
            return BitConverter.ToSingle(convert(buff, 4), 0);
        }

        protected double readDouble(Stream stream)
        {
            stream.Read(buff, 0, 8);
            return BitConverter.ToDouble(convert(buff, 8), 0);
        }

        protected DateTime readDateTime(Stream stream)
        {
            int dateTime = readInt32(stream);

            return EPOCH.AddSeconds(dateTime);
        }

        protected List<object> readList8(Stream stream) 
        {
            int len = readUInt8(stream);

            return readList(stream, len);
        }

        protected List<object> readList16(Stream stream) 
        {
            int len = readUInt16(stream);

            return readList(stream, len);
        }

        protected List<object> readList32(Stream stream)
        {
            int len = readInt32(stream);

            return readList(stream, len);
        }

        protected List<object> readList(Stream stream, int len)
        {
            List<object> list = new List<object>();

            for (int i = 0; i < len; i++)
            {
                object val = read(stream);

                list.Add(val);
            }
            return list;
        }

        protected Dictionary<object, object> readMap8(Stream stream)
        {
            int len = readUInt8(stream);

            return readMap(stream, len);
        }

        protected Dictionary<object, object> readMap16(Stream stream)
        {
            int len = readUInt16(stream);

            return readMap(stream, len);
        }

        protected Dictionary<object, object> readMap32(Stream stream)
        {
            int len = readInt32(stream);

            return readMap(stream, len);
        }
    
        protected Dictionary<object, object> readMap(Stream stream, int len)
        {
            Dictionary<object, object> dict = new Dictionary<object, object>();

            for (int i = 0; i < len; i++)
            {
                object key = read(stream);
                object val = read(stream);

                dict.Add(key, val);
            }
            return dict;
        }

        protected static void write(Stream stream, object obj)
        {
            if (obj == null)
            {
                stream.WriteByte((byte)Types.TYPE_NONE);
            }
            else if (obj is bool)
            {
                writeBoolType(stream, (bool)obj, Types.TYPE_TRUE, Types.TYPE_FALSE);
            }
            else if (obj is byte)
            {
                writeIntegerType(stream, (byte)obj);
            }
            else if (obj is sbyte)
            {
                writeIntegerType(stream, (sbyte)obj);
            }
            else if (obj is short)
            {
                writeIntegerType(stream, (short)obj);
            }
            else if (obj is ushort)
            {
                writeIntegerType(stream, (ushort)obj);
            }
            else if (obj is int)
            {
                writeIntegerType(stream, (int)obj);
            }
            else if (obj is uint)
            {
                writeIntegerType(stream, (uint)obj);
            }
            else if (obj is long)
            {
                writeIntegerType(stream, (long)obj);
            }
            else if (obj is double)
            {
                writeFloatType(stream, (double)obj);
            }
            else if (obj is float)
            {
                writeFloatType(stream, (float)obj);
            }
            else if (obj is ulong)
            {
                stream.WriteByte((byte)Types.TYPE_UINT_64);
                writeUInt64(stream, (ulong)obj);
            }
            else if (obj is string)
            {
                string str = (string)obj;

                writeVariableUnsigned(stream, (uint)str.Length, Types.TYPE_STRING_8, Types.TYPE_STRING_16, Types.TYPE_STRING_32);
                writeString(stream, str);
            }
            else if (obj is byte[])
            {
                byte[] bin = (byte[])obj;

                writeVariableUnsigned(stream, (uint)bin.Length, Types.TYPE_BINARY_8, Types.TYPE_BINARY_16, Types.TYPE_BINARY_32);
                writeBinary(stream, bin);
            }
            else if (obj is DateTime)
            {
                stream.WriteByte((byte)Types.TYPE_DATETIME);
                writeDateTime(stream, (DateTime)obj);
            }
            else if (obj is IDictionary)
            {
                IDictionary dict = (IDictionary)obj;

                writeVariableUnsigned(stream, (uint)dict.Count, Types.TYPE_MAP_8, Types.TYPE_MAP_16, Types.TYPE_MAP_32);
                writeMap(stream, dict);
            }
            else if (obj is IList)
            {
                IList list = (IList)obj;

                writeVariableUnsigned(stream, (uint)list.Count, Types.TYPE_ARRAY_8, Types.TYPE_ARRAY_16, Types.TYPE_ARRAY_32);
                writeList(stream, list);
            }
            else if (obj.GetType().IsArray)
            {
                object[] array = (object[])obj;

                writeVariableUnsigned(stream, (uint)array.Length, Types.TYPE_ARRAY_8, Types.TYPE_ARRAY_16, Types.TYPE_ARRAY_32);
                writeArray(stream, array);
            }
            else
            {
                throw new Exception("unknown encode param");
            }
        }

        protected static void writeVariableUnsigned(Stream stream, uint l, Types type1, Types type2, Types type3)
        {
            if (l >= byte.MinValue && l <= byte.MaxValue)
            {
                stream.WriteByte((byte)type1);
                BinaryPacker2.writeUInt8(stream, (byte)l);
            }
            else if (l >= ushort.MinValue && l <= ushort.MaxValue)
            {
                stream.WriteByte((byte)type2);
                BinaryPacker2.writeUInt16(stream, (ushort)l);
            }
            else if (l >= uint.MinValue && l <= uint.MaxValue)
            {
                stream.WriteByte((byte)type2);
                BinaryPacker2.writeUInt32(stream, (uint)l);
            }
        }

        protected static void writeBoolType(Stream stream, bool b, Types typeTrue, Types typeFalse) 
        {
            if (b)
            {
                stream.WriteByte((byte)typeTrue);
            }
            else
            {
                stream.WriteByte((byte)typeFalse);
            }
        }

        protected static void writeIntegerType(Stream stream, long l)
        {
            if (l >= sbyte.MinValue && l <= sbyte.MaxValue)
            {
                stream.WriteByte((byte)Types.TYPE_INT_8);
                writeInt8(stream, (sbyte)l);
            }
            else if (l >= byte.MinValue && l <= byte.MinValue)
            {
                stream.WriteByte((byte)Types.TYPE_UINT_8);
                writeUInt8(stream, (byte)l);
            }
            else if (l >= short.MinValue && l <= short.MaxValue)
            {
                stream.WriteByte((byte)Types.TYPE_INT_16);
                writeInt16(stream, (short)l);
            }
            else if (l >= ushort.MinValue && l <= ushort.MaxValue)
            {
                stream.WriteByte((byte)Types.TYPE_UINT_16);
                writeUInt16(stream, (ushort)l);
            }
            else if (l >= int.MinValue && l <= int.MaxValue)
            {
                stream.WriteByte((byte)Types.TYPE_INT_32);
                writeInt32(stream, (int)l);
            }
            else if (l >= uint.MinValue && l <= uint.MaxValue)
            {
                stream.WriteByte((byte)Types.TYPE_UINT_32);
                writeUInt32(stream, (uint)l);
            }
            else if (l >= long.MinValue && l <= long.MaxValue)
            {
                stream.WriteByte((byte)Types.TYPE_INT_64);
                writeInt64(stream, (long)l);
            }
        }

        protected static void writeFloatType(Stream stream, double d)
        {
            // TODO: for now we're just always writing a double
            stream.WriteByte((byte)Types.TYPE_DOUBLE);
            writeDouble(stream, d);
        }

        protected static void writeMap(Stream stream, IDictionary map)
        {
            foreach (object key in map.Keys)
            {
                write(stream, key);
                write(stream, map[key]);
            }
        }

        protected static void writeArray(Stream stream, object [] array)
        {
            foreach (object iter in array)
            {
                write(stream, iter);
            }
        }

        protected static void writeList(Stream stream, IList list)
        {
            foreach (object iter in list)
            {
                write(stream, iter);
            }
        }

        protected static void writeString(Stream stream, string d)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(d);

            stream.Write(bytes, 0, bytes.Length);
        }

        protected static void writeFloat(Stream stream, float d)
        {
            byte[] bytes = convert(BitConverter.GetBytes(d), 4);

            stream.Write(bytes, 0, bytes.Length);
        }

        protected static void writeDouble(Stream stream, double d)
        {
            byte[] bytes = convert(BitConverter.GetBytes(d), 8);

            stream.Write(bytes, 0, bytes.Length);
        }

        protected static void writeInt64(Stream stream, long i)
        {
            byte[] bytes = convert(BitConverter.GetBytes(i), 8);

            stream.Write(bytes, 0, bytes.Length);
        }

        protected static void writeUInt64(Stream stream, ulong i)
        {
            byte[] bytes = convert(BitConverter.GetBytes(i), 8);

            stream.Write(bytes, 0, bytes.Length);
        }

        protected static void writeInt32(Stream stream, int i)
        {
            byte[] bytes = convert(BitConverter.GetBytes(i), 4);

            stream.Write(bytes, 0, bytes.Length);
        }

        protected static void writeUInt32(Stream stream, uint i)
        {
            byte[] bytes = convert(BitConverter.GetBytes(i), 4);

            stream.Write(bytes, 0, bytes.Length);
        }

        protected static void writeInt16(Stream stream, short i)
        {
            byte[] bytes = convert(BitConverter.GetBytes(i), 2);

            stream.Write(bytes, 0, bytes.Length);
        }

        protected static void writeUInt16(Stream stream, ushort i)
        {
            byte[] bytes = convert(BitConverter.GetBytes(i), 2);

            stream.Write(bytes, 0, bytes.Length);
        }

        protected static void writeInt8(Stream stream, sbyte i)
        {
            stream.WriteByte((byte)i);
        }

        protected static void writeUInt8(Stream stream, byte i)
        {
            stream.WriteByte(i);
        }

        protected static void write(Stream stream, long i)
        {
            byte[] bytes = convert(BitConverter.GetBytes(i), 8);

            stream.Write(bytes, 0, bytes.Length);
        }

        protected static void writeBinary(Stream stream, byte[] bytes)
        {
            stream.Write(bytes, 0, bytes.Length);
        }

        protected static void writeDateTime(Stream stream, DateTime dateTime)
        {
            TimeSpan span = dateTime - EPOCH;

            writeUInt32(stream, (uint)span.TotalSeconds);
        }

        protected static byte[] convert(byte[] bytes, int len)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes, 0, len);
            }
            return bytes;
        }
    }
}
