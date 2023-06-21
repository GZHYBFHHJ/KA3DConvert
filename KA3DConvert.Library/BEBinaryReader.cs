using System;
using System.IO;
using System.Text;

namespace KA3DConvert
{
    internal class BEBinaryReader : BinaryReader
    {

        Encoding _encoding;
        byte[] _numBuffer = new byte[32];


        public BEBinaryReader(Stream input) : base(input)
        {
            _encoding = Encoding.UTF8;
        }

        public BEBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
        {
            _encoding = encoding;
        }


        public override short ReadInt16()
        {
            ReadToEnd(_numBuffer, 2);
            return (short)((_numBuffer[1] << 0) | (_numBuffer[0] << 8));
        }

        public override int ReadInt32()
        {
            ReadToEnd(_numBuffer, 4);
            return (_numBuffer[3] << 0) | (_numBuffer[2] << 8) | (_numBuffer[1] << 16) | (_numBuffer[0] << 24);
        }

        public unsafe override float ReadSingle()
        {
            int num = ReadInt32();
            return *(float*)&num;
        }

        public override string ReadString()
        {
            short len = ReadInt16();
            if (len < 0) throw new IOException("Invalid string length");

            if (len == 0) return string.Empty;

            if (len > _numBuffer.Length) _numBuffer = new byte[len];
            ReadToEnd(_numBuffer, len);
            return _encoding.GetString(_numBuffer, 0, len);
        }


        private void ReadToEnd(byte[] buffer, int count)
        {
            int total = 0;
            while (total < count)
            {
                int read = Read(buffer, total, count - total);
                if (read == 0) throw new EndOfStreamException();
                total += read;
            }
        }


    }
}
