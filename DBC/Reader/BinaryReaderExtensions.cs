// AlphaLegends
// https://github.com/The-Alpha-Project/alpha-legends

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace AlphaCoreExtractor.DBC.Reader
{
    public static class BinaryReaderExtensions
    {
        public static Dictionary<Type, Func<BinaryReader, object>> ReadValue = new Dictionary<Type, Func<BinaryReader, object>>
        {
            {typeof(bool),   br => br.ReadBoolean()},
            {typeof(sbyte),  br => br.ReadSByte()},
            {typeof(byte),   br => br.ReadByte()},
            {typeof(short),  br => br.ReadInt16()},
            {typeof(ushort), br => br.ReadUInt16()},
            {typeof(int),    br => br.ReadInt32()},
            {typeof(uint),   br => br.ReadUInt32()},
            {typeof(float),  br => br.ReadSingle()},
            {typeof(long),   br => br.ReadInt64()},
            {typeof(ulong),  br => br.ReadUInt64()},
            {typeof(double), br => br.ReadDouble()}
        };

        public static T Read<T>(this BinaryReader br)
        {
            return (T)ReadValue[typeof(T)](br);
        }

        public static sbyte[] ReadSByte(this BinaryReader br, int count)
        {
            var arr = new sbyte[count];
            for (int i = 0; i < count; i++)
                arr[i] = br.ReadSByte();

            return arr;
        }

        public static byte[] ReadByte(this BinaryReader br, int count)
        {
            var arr = new byte[count];
            for (int i = 0; i < count; i++)
                arr[i] = br.ReadByte();

            return arr;
        }

        public static int[] ReadInt32(this BinaryReader br, int count)
        {
            var arr = new int[count];
            for (int i = 0; i < count; i++)
                arr[i] = br.ReadInt32();

            return arr;
        }

        public static uint[] ReadUInt32(this BinaryReader br, int count)
        {
            var arr = new uint[count];
            for (int i = 0; i < count; i++)
                arr[i] = br.ReadUInt32();

            return arr;
        }

        public static float[] ReadSingle(this BinaryReader br, int count)
        {
            var arr = new float[count];
            for (int i = 0; i < count; i++)
                arr[i] = br.ReadSingle();

            return arr;
        }

        public static long[] ReadInt64(this BinaryReader br, int count)
        {
            var arr = new long[count];
            for (int i = 0; i < count; i++)
                arr[i] = br.ReadInt64();

            return arr;
        }

        public static ulong[] ReadUInt64(this BinaryReader br, int count)
        {
            var arr = new ulong[count];
            for (int i = 0; i < count; i++)
                arr[i] = br.ReadUInt64();

            return arr;
        }

        public static string ReadCString(this BinaryReader br)
        {
            StringBuilder tmpString = new StringBuilder();
            char tmpChar = br.ReadChar();
            char tmpEndChar = Convert.ToChar(Encoding.UTF8.GetString(new byte[] { 0 }));

            while (tmpChar != tmpEndChar)
            {
                tmpString.Append(tmpChar);
                tmpChar = br.ReadChar();
            }

            return tmpString.ToString();
        }

        public static string ReadString(this BinaryReader br, int count)
        {
            byte[] stringArray = br.ReadBytes(count);
            return Encoding.ASCII.GetString(stringArray);
        }
    }
}
