using System;
using System.Collections.Generic;
using System.IO;
using static KA3DConvert.KA3DConstants;

namespace KA3DConvert.Data
{

    public struct CompoSpriteLayer
    {
        public CompoSpriteLayer(string sprite, short x, short y)
        {
            SpriteName = sprite;
            UnkName = string.Empty;
            X = x;
            Y = y;
            ScaleX = 1f;
            ScaleY = 1f;
            Angle = 0f;
            FlipX = false;
            FlipY = false;
        }

        public CompoSpriteLayer(string sprite, string unk, short x, short y, float scaleX, float scaleY, float angle, bool flipX, bool flipY)
        {
            SpriteName = sprite;
            UnkName = unk;
            X = x;
            Y = y;
            ScaleX = scaleX;
            ScaleY = scaleY;
            Angle = angle;
            FlipX = flipX;
            FlipY = flipY;
        }


        public string SpriteName { get; set; }

        public short X { get; set; }

        public short Y { get; set; }


        // RVIO
        
        public string UnkName { get; set; } // always be empty

        public float ScaleX { get; set; }

        public float ScaleY { get; set; }

        public float Angle { get; set; }

        public bool FlipX { get; set; }

        public bool FlipY { get; set; }

    }

    public class CompoSprites : IDatSegment
    {
        public CompoSprites() { }

        public short Version { get; set; } = 1;

        public Dictionary<string, List<CompoSpriteLayer>> Composites { get; set; } = new Dictionary<string, List<CompoSpriteLayer>>();


        public static CompoSprites Read(DatReader reader, bool beginEnd, bool checkBounds = false)
        {
            _ = reader ?? throw new ArgumentNullException(nameof(reader));

            if (beginEnd) reader.Begin(MAGIC_COMP, "CompoSprites");
            try
            {
                var br = new BEBinaryReader(reader.BaseStream);

                short ver = br.ReadInt16();
                if (reader.Rvio)
                {
                    if (ver != 1) throw new NotSupportedException($"Invalid version: {ver}");
                }
                else
                {
                    if (ver != 1 && ver != 2) throw new NotSupportedException($"Invalid version: {ver}");
                }

                var comp = new CompoSprites
                {
                    Version = ver
                };

                short cscount = br.ReadInt16();
                if (cscount < 0) throw new IOException("Invalid compo sprite count");
                for (int i = 0; i < cscount; i++)
                {
                    string name = br.ReadString();

                    short ecount = br.ReadInt16();
                    if (ecount < 0) throw new IOException("Invalid layer count");
                    List<CompoSpriteLayer> layers = new List<CompoSpriteLayer>(ecount);
                    for (int j = 0; j < ecount; j++)
                    {
                        if (reader.Rvio)
                        {
                            layers.Add(new CompoSpriteLayer(
                                sprite: br.ReadString(),
                                unk   : br.ReadString(),
                                x     : br.ReadInt16(),
                                y     : br.ReadInt16(),
                                scaleX: br.ReadSingle(),
                                scaleY: br.ReadSingle(),
                                angle : br.ReadSingle(),
                                flipX : br.ReadBoolean(),
                                flipY : br.ReadBoolean()
                                ));
                        }
                        else
                        {
                            layers.Add(new CompoSpriteLayer(
                                sprite: br.ReadString(),
                                x     : br.ReadInt16(),
                                y     : br.ReadInt16()
                                ));
                        }
                    }

                    if (!reader.Rvio && ver > 1) br.ReadInt16(); // padding

                    comp.Composites.Add(name, layers);
                }

                return comp;
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

            if (writer.Rvio)
            {
                if (Version != 1) throw new NotSupportedException($"Invalid version: {Version}");
            }
            else
            {
                if (Version != 1 && Version != 2) throw new NotSupportedException($"Invalid version: {Version}");
            }

            if (beginEnd) writer.Begin(MAGIC_COMP);
            try
            {
                var bw = new BEBinaryWriter(writer.BaseStream);
                bw.Write(Version);

                bw.Write(checked((short)Composites.Count));
                foreach (var pair in Composites)
                {
                    bw.Write(pair.Key);

                    bw.Write(checked((short)pair.Value.Count));
                    foreach (var layer in pair.Value)
                    {
                        bw.Write(layer.SpriteName);

                        if (writer.Rvio)
                        {
                            bw.Write(layer.UnkName);

                            bw.Write(layer.X);
                            bw.Write(layer.Y);
                            bw.Write(layer.ScaleX);
                            bw.Write(layer.ScaleY);
                            bw.Write(layer.Angle);
                            bw.Write(layer.FlipX);
                            bw.Write(layer.FlipY);
                        }
                        else
                        {
                            bw.Write(layer.X);
                            bw.Write(layer.Y);
                        }
                    }

                    if (!writer.Rvio && Version > 1) bw.Write((short)0); // padding

                }

            }
            finally
            {
                if (beginEnd) writer.End();
            }
        }

    }
}
