using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using static KA3DConvert.KA3DConstants;

namespace KA3DConvert.Data
{
    internal class LocalizationIDs : ICollection<string>
    {

        HashSet<string> _baseSet;
        Dictionary<string, List<string>> _texts;

        public LocalizationIDs(HashSet<string> baseSet, Dictionary<string, List<string>> texts)
        {
            _baseSet = baseSet;
            _texts = texts;

        }

        public int Count => _baseSet.Count;

        public bool IsReadOnly => false;

        public void Add(string item)
        {
            _ = item ?? throw new ArgumentNullException(nameof(item));

            if (_baseSet.Add(item))
            {
                foreach (var texts in _texts.Values)
                {
                    texts.Add(string.Empty);
                }
            }
        }

        public void Clear()
        {
            _baseSet.Clear();
            foreach (var texts in _texts.Values)
            {
                texts.Clear();
            }
        }

        public bool Contains(string item)
        {
            if (item == null) return false;

            return _baseSet.Contains(item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            _baseSet.CopyTo(array, arrayIndex);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _baseSet.GetEnumerator();
        }

        public bool Remove(string item)
        {
            if (item == null) return false;

            int i = 0;
            foreach (var id in _baseSet)
            {
                if (id == item)
                {
                    foreach (var texts in _texts.Values)
                    {
                        texts.RemoveAt(i);
                    }
                    return _baseSet.Remove(item);
                }
                i++;
            }
            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _baseSet.GetEnumerator();
        }

    }

    internal class LocalizationLanguages : ICollection<string>
    {

        LocalizationIDs _textIDs;
        Dictionary<string, List<string>> _texts;

        public LocalizationLanguages(LocalizationIDs textIDs, Dictionary<string, List<string>> texts)
        {
            _textIDs = textIDs;
            _texts = texts;
        }

        public int Count => _texts.Count;

        public bool IsReadOnly => false;

        public void Add(string item)
        {
            _ = item ?? throw new ArgumentNullException(nameof(item));

            var texts = new List<string>(_textIDs.Count);
            _texts.Add(item, texts);

            for (int i = 0; i < _textIDs.Count; i++) texts.Add(string.Empty);

        }

        public void Clear()
        {
            _texts.Clear();
        }

        public bool Contains(string item)
        {
            if (item == null) return false;

            return _texts.ContainsKey(item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            _texts.Keys.CopyTo(array, arrayIndex);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _texts.Keys.GetEnumerator();
        }

        public bool Remove(string item)
        {
            if (item == null) return false;

            return _texts.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _texts.Keys.GetEnumerator();
        }
    }

    public class Localization : IDatSegment
    {

        LocalizationLanguages _langs;
        LocalizationIDs _textIDs;
        Dictionary<string, List<string>> _texts; // <lang, text[index]>

        public Localization()
        {
            _texts = new Dictionary<string, List<string>>();
            _textIDs = new LocalizationIDs(new HashSet<string>(), _texts);
            _langs = new LocalizationLanguages(_textIDs, _texts);
        }

        public ICollection<string> TextIDs => _textIDs;

        public ICollection<string> Languages => _langs;
        

        public string GetString(string lang, string id)
        {
            _ = lang ?? throw new ArgumentNullException(nameof(lang));
            _ = id ?? throw new ArgumentNullException(nameof(id));

            if (!_texts.TryGetValue(lang, out List<string> texts)) return null;

            int i = 0;
            foreach (var textId in _textIDs)
            {
                if (textId == id) return texts[i];
                i++;
            }
            return null;
        }

        public bool SetString(string lang, string id, string value)
        {
            _ = lang ?? throw new ArgumentNullException(nameof(lang));
            _ = id ?? throw new ArgumentNullException(nameof(id));
            _ = value ?? throw new ArgumentNullException(nameof(value));

            if (!_texts.TryGetValue(lang, out List<string> texts)) return false;

            int i = 0;
            foreach (var textId in _textIDs)
            {
                if (textId == id)
                {
                    texts[i] = value;
                    return true;
                }
                i++;
            }
            return false;
        }


        public static Localization Read(DatReader reader, bool beginEnd, bool checkBounds = false)
        {
            _ = reader ?? throw new ArgumentNullException(nameof(reader));

            if (reader.Rvio) throw new ArgumentException(nameof(reader), "Invalid DAT Format");

            if (beginEnd) reader.Begin(MAGIC_TEXT, "Localization");
            try
            {
                var br = new BEBinaryReader(reader.BaseStream);

                short ver = br.ReadInt16();
                if (ver != 1) throw new NotSupportedException($"Invalid version: {ver}");

                var text = new Localization();

                short langcount;
                short idcount;

                reader.Begin(MAGIC_TEXT_LDAT, "Localization");
                try
                {
                    langcount = br.ReadInt16();
                    if (langcount < 0) throw new IOException("Invalid language count");
                    for (int i = 0; i < langcount; i++)
                    {
                        text._langs.Add(br.ReadString());
                    }
                }
                finally
                {
                    reader.End(checkBounds);
                }

                reader.Begin(MAGIC_TEXT_LIDS, "Localization");
                try
                {
                    idcount = br.ReadInt16();
                    if (idcount < 0) throw new IOException("Invalid ID count");
                    for (int i = 0; i < idcount; i++)
                    {
                        text._textIDs.Add(br.ReadString());
                    }
                }
                finally
                {
                    reader.End(checkBounds);
                }

                foreach (var pair in text._texts)
                {
                    reader.Begin(MAGIC_TEXT_TXGP, "Localization");
                    try
                    {
                        for (int i = 0; i < idcount; i++)
                        {
                            pair.Value[i] = br.ReadString();
                        }
                    }
                    finally
                    {
                        reader.End(checkBounds);
                    }
                }

                return text;
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

            if (beginEnd) writer.Begin(MAGIC_TEXT);
            try
            {
                var bw = new BEBinaryWriter(writer.BaseStream);
                bw.Write((short)1);

                writer.Begin(MAGIC_TEXT_LDAT);
                try
                {
                    bw.Write(checked((short)_texts.Count));
                    foreach (var lang in _texts.Keys)
                    {
                        bw.Write(lang);
                    }
                }
                finally
                {
                    writer.End();
                }

                writer.Begin(MAGIC_TEXT_LIDS);
                try
                {
                    bw.Write(checked((short)_textIDs.Count));
                    foreach (var id in _textIDs)
                    {
                        bw.Write(id);
                    }
                }
                finally
                {
                    writer.End();
                }

                foreach (var texts in _texts.Values)
                {
                    writer.Begin(MAGIC_TEXT_TXGP);
                    try
                    {
                        foreach (var text in texts)
                        {
                            bw.Write(text);
                        }
                    }
                    finally
                    {
                        writer.End();
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
