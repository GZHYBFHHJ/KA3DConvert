using System;
using System.Collections.Generic;
using System.IO;
using static KA3DConvert.KA3DConstants;

namespace KA3DConvert
{
    public sealed class DatWriter : IDisposable
    {
        private struct DatReaderSegment
        {
            public long pos;
        }

        BEBinaryWriter _writer;
        bool           _leaveOpen;

        bool _rvio;
        long _pos;
        Stack<DatReaderSegment> _segments = new Stack<DatReaderSegment>();


        public DatWriter(Stream output, bool rvio = false, bool leaveOpen = false)
        {
            _ = output ?? throw new ArgumentNullException(nameof(output));

            _writer = new BEBinaryWriter(output);
            _leaveOpen = leaveOpen;

            _rvio = rvio;


            if (rvio) _writer.Write(MAGIC_RVIO);
            else      _writer.Write(MAGIC_KA3D);
            _writer.Write((int)0);
            _pos = _writer.BaseStream.Position;

        }


        public Stream BaseStream => _writer.BaseStream;

        public bool Rvio => _rvio;


        public void Begin(int magic)
        {
            DatReaderSegment segment;
            _writer.Write(magic);
            _writer.Write((int)0);
            segment.pos = _writer.BaseStream.Position;

            _segments.Push(segment);
            
        }

        public void End()
        {
            long pos = _writer.BaseStream.Position;

            DatReaderSegment segment = _segments.Pop();
            _writer.BaseStream.Seek(segment.pos - 4, SeekOrigin.Begin);
            try
            {
                _writer.Write(checked((int)(pos - segment.pos)));
            }
            finally
            {
                _writer.BaseStream.Seek(pos, SeekOrigin.Begin);
            }

        }


        public void Dispose()
        {
            Close();
        }

        public void Close()
        {
            try
            {
                long pos = _writer.BaseStream.Position;
                _writer.BaseStream.Seek(_pos - 4, SeekOrigin.Begin);
                _writer.Write(checked((int)(pos - _pos)));

                _writer.BaseStream.Seek(pos, SeekOrigin.Begin);
            }
            finally
            {
                if (!_leaveOpen)
                    _writer.Close();
            }
        }

    }
}
