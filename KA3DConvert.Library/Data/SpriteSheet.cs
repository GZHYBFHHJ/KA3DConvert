using System;
using System.Collections.Generic;
using System.IO;
using static KA3DConvert.KA3DConstants;

namespace KA3DConvert.Data
{
    public struct Sprite
    {
        public Sprite(short x, short y, short width, short height, short pivotX, short pivotY)
        {
            X      = x;
            Y      = y;
            Width  = width;
            Height = height;
            PivotX = pivotX;
            PivotY = pivotY;
        }

        public short X { get; set; }
        
        public short Y { get; set; }

        public short Width { get; set; }

        public short Height { get; set; }

        public short PivotX { get; set; }

        public short PivotY { get; set; }


        public override string ToString() => $"{X}, {Y}, {Width}, {Height}, {PivotX}, {PivotY}"; // for testing

    }

    public class SpriteSheet : IDatSegment
    {
        public SpriteSheet(string tex)
        {
            TextureFile = tex;
        }
        
        public string TextureFile { get; set; }

        public Dictionary<string, Sprite> Sprites { get; } = new Dictionary<string, Sprite>();



        public static SpriteSheet Read(DatReader reader, bool beginEnd, bool checkBounds = false)
        {
            _ = reader ?? throw new ArgumentNullException(nameof(reader));

            if (reader.Rvio) throw new ArgumentException(nameof(reader), "Invalid DAT Format");

            if (beginEnd) reader.Begin(MAGIC_SPRT, "SpriteSheet");
            try
            {
                var br = new BEBinaryReader(reader.BaseStream);

                short ver = br.ReadInt16();
                if (ver != 1) throw new NotSupportedException($"Invalid version: {ver}");

                var sheet = new SpriteSheet(tex: br.ReadString());

                short count = br.ReadInt16();
                if (count < 0) throw new IOException("Invalid sprite count");
                for (int i = 0; i < count; i++)
                {
                    sheet.Sprites.Add(br.ReadString(), new Sprite(
                        x:      br.ReadInt16(),
                        y:      br.ReadInt16(),
                        width:  br.ReadInt16(),
                        height: br.ReadInt16(),
                        pivotX: br.ReadInt16(),
                        pivotY: br.ReadInt16()
                        ));
                }

                return sheet;
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

            if (beginEnd) writer.Begin(MAGIC_SPRT);
            try
            {
                var bw = new BEBinaryWriter(writer.BaseStream);
                bw.Write((short)1);

                bw.Write(TextureFile);

                bw.Write(checked((short)Sprites.Count));
                foreach (var pair in Sprites)
                {
                    bw.Write(pair.Key);

                    bw.Write(pair.Value.X);
                    bw.Write(pair.Value.Y);
                    bw.Write(pair.Value.Width);
                    bw.Write(pair.Value.Height);
                    bw.Write(pair.Value.PivotX);
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
