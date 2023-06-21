using System;
using System.IO;
using KA3DConvert.Data;
using static KA3DConvert.KA3DConstants;

namespace KA3DConvert
{

    public interface IDatSegment
    {

        void Write(DatWriter writer);

    }

    public static class DatSegment
    {

        public static IDatSegment Read(DatReader reader, bool checkBounds = false)
        {
            _ = reader ?? throw new ArgumentNullException(nameof(reader));

            do
            {
                int magic;
                try
                {
                    magic = reader.Begin();
                }
                catch (EndOfStreamException)
                {
                    throw new IOException($"Invalid DAT Format");
                }

                try
                {
                    switch (magic)
                    {
                        case MAGIC_SPRT when !reader.Rvio: return SpriteSheet .Read(reader, beginEnd: false, checkBounds);
                        case MAGIC_FONT when !reader.Rvio: return Font        .Read(reader, beginEnd: false, checkBounds);
                        case MAGIC_TEXT when !reader.Rvio: return Localization.Read(reader, beginEnd: false, checkBounds);
                        case MAGIC_COMP                  : return CompoSprites.Read(reader, beginEnd: false, checkBounds);
                    }
                }
                finally
                {
                    reader.End(checkBounds);
                }
            }
            while (true);
        }

    }

}
