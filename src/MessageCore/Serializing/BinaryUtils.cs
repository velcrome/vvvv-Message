using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace VVVV.Pack.Game.Core.Serializing
{
    public static class BinaryUtils
    {
        public static bool ReadBool(this Stream input)
        {
            byte[] tmp = new byte[1];
            input.Read(tmp, 0, 1);
            return BitConverter.ToBoolean(tmp, 0);
        }
        public static uint ReadUint(this Stream input)
        {
            byte[] tmp = new byte[4];
            input.Read(tmp, 0, 4);
            return BitConverter.ToUInt32(tmp, 0);
        }
        public static int ReadInt(this Stream input)
        {
            byte[] tmp = new byte[4];
            input.Read(tmp, 0, 4);
            return BitConverter.ToInt32(tmp, 0);
        }
        public static float ReadFloat(this Stream input)
        {
            byte[] tmp = new byte[4];
            input.Read(tmp, 0, 4);
            return BitConverter.ToSingle(tmp, 0);
        }
        public static double ReadDouble(this Stream input)
        {
            byte[] tmp = new byte[8];
            input.Read(tmp, 0, 8);
            return BitConverter.ToDouble(tmp, 0);
        }

        public static void WriteBool(this Stream input, bool data)
        {
            byte[] tmp = BitConverter.GetBytes(data);
            input.Write(tmp, 0, tmp.Length);
        }
        public static void WriteUint(this Stream input, uint data)
        {
            byte[] tmp = BitConverter.GetBytes(data);
            input.Write(tmp, 0, tmp.Length);
        }
        public static void WriteInt(this Stream input, int data)
        {
            byte[] tmp = BitConverter.GetBytes(data);
            input.Write(tmp, 0, tmp.Length);
        }
        public static void WriteFloat(this Stream input, float data)
        {
            byte[] tmp = BitConverter.GetBytes(data);
            input.Write(tmp, 0, tmp.Length);
        }
        public static void WriteDouble(this Stream input, double data)
        {
            byte[] tmp = BitConverter.GetBytes(data);
            input.Write(tmp, 0, tmp.Length);
        }


        public static string ReadASCII(this Stream input, int length)
        {
            byte[] tmp = new byte[length];
            input.Read(tmp, 0, length);
            return System.Text.Encoding.ASCII.GetString(tmp);
        }
        public static string ReadUTF8(this Stream input, int length)
        {
            byte[] tmp = new byte[length];
            input.Read(tmp, 0, length);
            return System.Text.Encoding.UTF8.GetString(tmp);
        }
        public static string ReadUnicode(this Stream input, int length)
        {
            byte[] tmp = new byte[length];
            input.Read(tmp, 0, length);
            return System.Text.Encoding.Unicode.GetString(tmp);
        }

        public static void WriteASCII(this Stream input, string data)
        {
            byte[] tmp = System.Text.Encoding.ASCII.GetBytes(data);
            input.Write(tmp, 0, tmp.Length);
        }
        public static void WriteUTF8(this Stream input, string data)
        {
            byte[] tmp = System.Text.Encoding.UTF8.GetBytes(data);
            input.Write(tmp, 0, tmp.Length);
        }
        public static void WriteUnicode(this Stream input, string data)
        {
            byte[] tmp = System.Text.Encoding.Unicode.GetBytes(data);
            input.Write(tmp, 0, tmp.Length);
        }

        public static uint ASCIILength(this string s)
        {
            byte[] tmp = System.Text.Encoding.ASCII.GetBytes(s);
            return (uint)tmp.Length;
        }
        public static uint UTF8Length(this string s)
        {
            byte[] tmp = System.Text.Encoding.UTF8.GetBytes(s);
            return (uint)tmp.Length;
        }
        public static uint UnicodeLength(this string s)
        {
            byte[] tmp = System.Text.Encoding.Unicode.GetBytes(s);
            return (uint)tmp.Length;
        }
    }
}
