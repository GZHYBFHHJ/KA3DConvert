using System;
using System.Collections.Generic;
using System.IO;
using static KA3DConvert.KA3DConstants;

namespace KA3DConvert.Data
{

    public struct AnimationClip
    {
        public AnimationClip(Dictionary<string, AnimationFrame> frames)
        {
            Frames = frames;
        }

        public Dictionary<string, AnimationFrame> Frames { get; set; }

    }

    public struct AnimationFrame
    {
        public AnimationFrame(short unk1, short unk2, short unk3)
        {
            Unk1 = unk1;
            Unk2 = unk2;
            Unk3 = unk3;
        }

        public short Unk1 { get; set; } // always be 0x00

        public short Unk2 { get; set; } // always be 0x00

        public short Unk3 { get; set; } // always be 0x00

    }

    public class Animation : IDatSegment
    {
        public Animation()
        {
        }

        public Dictionary<string, AnimationClip> Clips { get; set; } = new Dictionary<string, AnimationClip>();


        public static Animation Read(DatReader reader, bool beginEnd, bool checkBounds = false)
        {
            _ = reader ?? throw new ArgumentNullException(nameof(reader));

            if (reader.Rvio) throw new ArgumentException(nameof(reader), "Invalid DAT Format");

            if (beginEnd) reader.Begin(MAGIC_ANIM, "Animation");
            try
            {
                var br = new BEBinaryReader(reader.BaseStream);

                short ver = br.ReadInt16();
                if (ver != 1) throw new NotSupportedException($"Invalid version: {ver}");

                var anim = new Animation();

                short clipcount = br.ReadInt16();
                if (clipcount < 0) throw new IOException("Invalid clip count");
                for (int i = 0; i < clipcount; i++)
                {
                    AnimationClip clip = new AnimationClip(new Dictionary<string, AnimationFrame>());
                    string clipname = br.ReadString();

                    short framecount = br.ReadInt16();
                    if (framecount < 0) throw new IOException("Invalid frame count");
                    for (int j = 0; i < framecount; j++)
                    {
                        clip.Frames.Add(br.ReadString(), new AnimationFrame(
                            unk1: br.ReadInt16(),
                            unk2: br.ReadInt16(),
                            unk3: br.ReadInt16()
                            ));
                    }

                    anim.Clips.Add(clipname, clip);
                }

                return anim;
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

            if (beginEnd) writer.Begin(MAGIC_ANIM);
            try
            {
                var bw = new BEBinaryWriter(writer.BaseStream);
                bw.Write((short)1);

                bw.Write(checked((short)Clips.Count));
                foreach (var pair in Clips)
                {
                    bw.Write(pair.Key);

                    bw.Write(checked((short)pair.Value.Frames.Count));
                    foreach (var pair1 in pair.Value.Frames)
                    {
                        bw.Write(pair1.Key);

                        bw.Write(pair1.Value.Unk1);
                        bw.Write(pair1.Value.Unk2);
                        bw.Write(pair1.Value.Unk3);

                    }

                }

            }
            finally
            {
                if (beginEnd) writer.End();
            }
        }


    }
}
