using System;
using System.Collections.Generic;
using System.IO;
using static KA3DConvert.KA3DConstants;

namespace KA3DConvert.Data
{
    public struct Character
    {
        public Character(short x, short y, short width, short height, short pivotY)
        {
            X      = x;
            Y      = y;
            Width  = width;
            Height = height;
            PivotY = pivotY;
        }

        public short X { get; set; }

        public short Y { get; set; }

        public short Width { get; set; }

        public short Height { get; set; }

        public short PivotY { get; set; }


        public override string ToString() => $"{X}, {Y}, {Width}, {Height}, {PivotY}"; // for testing

    }

    public class Font : IDatSegment
    {
        public Font(string tex, short leading, short tracking)
        {
            TextureFile = tex;
            Leading = leading;
            Tracking = tracking;
        }

        public string TextureFile { get; set; }

        public short Leading { get; set; }

        public short Tracking { get; set; }

        public Dictionary<char, Character> Characters { get; } = new Dictionary<char, Character>();


        public static Font Read(DatReader reader, bool beginEnd, bool checkBounds = false)
        {
            _ = reader ?? throw new ArgumentNullException(nameof(reader));

            if (reader.Rvio) throw new ArgumentException(nameof(reader), "Invalid DAT Format");

            if (beginEnd) reader.Begin(MAGIC_FONT, "Font");
            try
            {
                var br = new BEBinaryReader(reader.BaseStream);

                short ver = br.ReadInt16();
                if (ver != 1) throw new NotSupportedException($"Invalid version: {ver}");

                var font = new Font(tex: br.ReadString(), leading: br.ReadInt16(), tracking: br.ReadInt16());

                short count = br.ReadInt16();
                if (count < 0) throw new IOException("Invalid character count");
                for (int i = 0; i < count; i++)
                {
                    font.Characters.Add((char)br.ReadInt16(), new Character(
                        x     : br.ReadInt16(),
                        y     : br.ReadInt16(),
                        width : br.ReadInt16(),
                        height: br.ReadInt16(),
                        pivotY: br.ReadInt16()
                        ));
                }

                return font;
            }
            finally
            {
                if (beginEnd) reader.End(checkBounds);
            }
        }


        void IDatSegment.Write(KA3DConvert.DatWriter writer) => Write(writer, beginEnd: true);

        public void Write(DatWriter writer, bool beginEnd)
        {
            _ = writer ?? throw new ArgumentNullException(nameof(writer));

            if (writer.Rvio) throw new ArgumentException(nameof(writer), "Invalid DAT Format");

            if (beginEnd) writer.Begin(MAGIC_FONT);
            try
            {
                var bw = new BEBinaryWriter(writer.BaseStream);
                bw.Write((short)1);

                bw.Write(TextureFile);
                bw.Write(Leading);
                bw.Write(Tracking);

                bw.Write(checked((short)Characters.Count));
                foreach (var pair in Characters)
                {
                    bw.Write((short)pair.Key);

                    bw.Write(pair.Value.X);
                    bw.Write(pair.Value.Y);
                    bw.Write(pair.Value.Width);
                    bw.Write(pair.Value.Height);
                    bw.Write(pair.Value.PivotY);

                }

            }
            finally
            {
                if (beginEnd) writer.End();
            }
        }


    }
}
