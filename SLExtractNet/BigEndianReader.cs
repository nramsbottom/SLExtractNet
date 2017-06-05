
/*
 * BigEndianReader.cs
 * 
 * Wrapper around the BCL BinaryReader to allow reading of big endian numbers from streams.
 * 
 * Based on code from https://stackoverflow.com/questions/123918/how-can-one-simplify-network-byte-order-conversion-from-a-binaryreader
 *
 */

using System;
using System.IO;
using System.Text;

namespace SLExtractNet
{
    public class BigEndianReader : IDisposable
    {
        public BigEndianReader(BinaryReader baseReader)
        {
            mBaseReader = baseReader;
        }

        public short ReadInt16()
        {
            return BitConverter.ToInt16(ReadBigEndianBytes(2), 0);
        }

        public byte ReadByte()
        {
            return mBaseReader.ReadByte();
        }

        public ushort ReadUInt16()
        {
            return BitConverter.ToUInt16(ReadBigEndianBytes(2), 0);
        }

        public uint ReadUInt32()
        {
            return BitConverter.ToUInt32(ReadBigEndianBytes(4), 0);
        }

        public byte[] ReadBigEndianBytes(int count)
        {
            byte[] bytes = new byte[count];
            for (int i = count - 1; i >= 0; i--)
                bytes[i] = mBaseReader.ReadByte();

            return bytes;
        }

        public string ReadAsciiString(byte count)
        {
            byte[] bytes = mBaseReader.ReadBytes(count);
            return Encoding.ASCII.GetString(bytes, 0, count);
        }

        /// <summary>
        /// Reads a zero terminated ASCII string.
        /// </summary>
        /// <returns></returns>
        public string ReadAsciiStringZ()
        {
            using (var ms = new MemoryStream())
            {
                for (;;)
                {
                    byte b = mBaseReader.ReadByte();
                    if (b == 0)
                        break;
                    ms.WriteByte(b);
                }
                return Encoding.ASCII.GetString(ms.GetBuffer(), 0, (int)ms.Length); 
            }
        }

        public byte[] ReadBytes(int count)
        {
            return mBaseReader.ReadBytes(count);
        }

        public void Close()
        {
            mBaseReader.Close();
        }

        public void Dispose()
        {
            mBaseReader.Dispose();
        }

        public Stream BaseStream
        {
            get { return mBaseReader.BaseStream; }
        }

        private BinaryReader mBaseReader;
    }
}
