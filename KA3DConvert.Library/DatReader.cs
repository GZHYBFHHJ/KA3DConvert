using System;
using System.Collections.Generic;
using System.IO;
using static KA3DConvert.KA3DConstants;

namespace KA3DConvert
{
    public sealed class DatReader : IDisposable
    {
        private struct DatReaderSegment
        {
            public int  magic;
            public int  size;
            public long pos;
        }

        BEBinaryReader _reader;
        bool           _leaveOpen;
        bool           _checkBounds;

        bool _rvio;
        int  _size;
        long _pos;
        Stack<DatReaderSegment> _segments = new Stack<DatReaderSegment>();


        public DatReader(Stream input, bool checkBounds = false, bool leaveOpen = false)
        {
            _ = input ?? throw new ArgumentNullException(nameof(input));

            _reader = new BEBinaryReader(input);
            _leaveOpen = leaveOpen;
            _checkBounds = checkBounds;


            int magic = _reader.ReadInt32();
            if (magic == MAGIC_KA3D)
                _rvio = false;
            else if (magic == MAGIC_RVIO)
                _rvio = true;
            else throw new IOException("Invalid DAT Format");

            _size = _reader.ReadInt32();
            if (_size < 0) throw new IOException("Invalid DAT Format");

            _pos = _reader.BaseStream.Position;

        }


        public Stream BaseStream => _reader.BaseStream;

        public bool Rvio => _rvio;
        
        

        public int Begin()
        {
            DatReaderSegment segment;
            segment.magic = _reader.ReadInt32();
            segment.size  = _reader.ReadInt32();
            segment.pos   = _reader.BaseStream.Position;
            if (segment.size < 0) throw new IOException("Invalid DAT Format");

            _segments.Push(segment);

            return segment.magic;
        }

        public void Begin(int magic, string name)
        {
            while (true)
            {
                try
                {
                    int m = Begin();
                    if (m == magic)
                    {
                        break;
                    }
                    End();
                }
                catch (EndOfStreamException)
                {
                    throw new IOException($"Invalid {name ?? "DAT"} Format");
                }
            }
        }


        public void End(bool checkBounds = false)
        {
            long pos = _reader.BaseStream.Position;

            var segment = _segments.Pop();
            if (pos > segment.pos + segment.size)
            {
                if (checkBounds) throw new IOException("Invalid DAT Format");
            }
            else
                _reader.BaseStream.Seek(segment.pos + segment.size, SeekOrigin.Begin);

        }


        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            try
            {
                long pos = _reader.BaseStream.Position;
                if (pos > _pos + _size)
                {
                    if (_checkBounds) throw new IOException("Invalid DAT Format");
                }
                else
                    _reader.BaseStream.Seek(_pos + _size, SeekOrigin.Begin);
            }
            finally
            {
                if (!_leaveOpen)
                    _reader.Close();
            }
        }

    }
}
