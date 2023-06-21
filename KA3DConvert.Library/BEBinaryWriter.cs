using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KA3DConvert
{
    internal class BEBinaryWriter : BinaryWriter
    {

        Encoding _encoding;
        byte[] _numBuffer = new byte[32];

        public BEBinaryWriter(Stream output) : base(output)
        {
            _encoding = Encoding.Default;
        }

        public BEBinaryWriter(Stream output, Encoding encoding) : base(output, encoding)
        {
            _encoding = encoding;
        }

        protected BEBinaryWriter()
        {
        }


        public override void Write(short value)
        {
            _numBuffer[1] = unchecked((byte)(value >> 0));
            _numBuffer[0] = unchecked((byte)(value >> 8));
            Write(_numBuffer, 0, 2);
        }

        public override void Write(int value)
        {
            _numBuffer[3] = unchecked((byte)(value >> 0));
            _numBuffer[2] = unchecked((byte)(value >> 8));
            _numBuffer[1] = unchecked((byte)(value >> 16));
            _numBuffer[0] = unchecked((byte)(value >> 24));
            Write(_numBuffer, 0, 4);
        }

        public override unsafe void Write(float value)
        {
            Write(*(int*)&value);
        }

        public override void Write(string value)
        {
            _ = value ?? throw new ArgumentNullException(nameof(value));

            int len = _encoding.GetByteCount(value);
            if (len > short.MaxValue) throw new ArgumentOutOfRangeException("String too large", nameof(value));
            Write((short)len);

            if (_numBuffer.Length < len)
                _numBuffer = new byte[len];
            _encoding.GetBytes(value, 0, value.Length, _numBuffer, 0);
            Write(_numBuffer, 0, len);
        }


    }
}
