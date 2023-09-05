using KA3DConvert.Data;
using System.Xml;

namespace KA3DConvert.CLI
{
    public static class XmlSegment
    {

        public static XmlDocument Read(Stream input, bool checkBounds = false)
        {
            IDatSegment segment;
            bool rvio;
            using (var reader = new DatReader(input, checkBounds, leaveOpen: true))
            {
                rvio = reader.Rvio;
                segment = DatSegment.Read(reader, checkBounds);
            }


            var doc = new XmlDocument();

            XmlDeclaration decl = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.AppendChild(decl);

            if (segment is SpriteSheet sprt)
            {
                var element = doc.CreateElement("spritesheet");

                element.SetAttribute("texture", sprt.TextureFile);
                foreach (var sprite in sprt.Sprites)
                {
                    var child = doc.CreateElement("sprite");
                    child.SetAttribute("name", sprite.Key);

                    child.SetAttribute("x"     , sprite.Value.X     .ToString());
                    child.SetAttribute("y"     , sprite.Value.Y     .ToString());
                    child.SetAttribute("width" , sprite.Value.Width .ToString());
                    child.SetAttribute("height", sprite.Value.Height.ToString());
                    child.SetAttribute("pivotX", sprite.Value.PivotX.ToString());
                    child.SetAttribute("pivotY", sprite.Value.PivotY.ToString());

                    element.AppendChild(child);
                }

                doc.AppendChild(element);
            }
            else if (segment is Font font)
            {
                var element = doc.CreateElement("font");

                element.SetAttribute("texture" , font.TextureFile);

                element.SetAttribute("leading" , font.Leading .ToString());
                element.SetAttribute("tracking", font.Tracking.ToString());

                foreach (var character in font.Characters)
                {
                    var child = doc.CreateElement("character");
                    child.SetAttribute("char", ((short)character.Key).ToString("x4"));

                    child.SetAttribute("x"     , character.Value.X     .ToString());
                    child.SetAttribute("y"     , character.Value.Y     .ToString());
                    child.SetAttribute("width" , character.Value.Width .ToString());
                    child.SetAttribute("height", character.Value.Height.ToString());
                    child.SetAttribute("pivotY", character.Value.PivotY.ToString());

                    element.AppendChild(child);
                }

                doc.AppendChild(element);
            }
            else if (segment is Localization text)
            {
                var element = doc.CreateElement("localization");

                var langs = doc.CreateElement("langs");
                foreach (var lang in text.Languages)
                {
                    langs.AppendChild(doc.CreateElement(lang));
                }
                element.AppendChild(langs);

                var texts = doc.CreateElement("texts");
                foreach (var id in text.TextIDs)
                {
                    var child = doc.CreateElement("text");
                    child.SetAttribute("id", id);

                    foreach (var lang in text.Languages)
                    {
                        var t = doc.CreateElement(lang);
                        t.InnerText = text.GetString(lang, id);

                        child.AppendChild(t);
                    }

                    texts.AppendChild(child);
                }
                element.AppendChild(texts);

                doc.AppendChild(element);
            }
            else if (segment is CompoSprites comp)
            {
                var element = doc.CreateElement("composprites");

                element.SetAttribute("rvio"   , rvio        .ToString());
                element.SetAttribute("version", comp.Version.ToString());
                
                foreach (var cs in comp.Composites)
                {
                    var child = doc.CreateElement("composprite");
                    child.SetAttribute("name", cs.Key);

                    foreach (var layer in cs.Value)
                    {
                        var sprite = doc.CreateElement("layer");
                        sprite.SetAttribute("sprite", layer.SpriteName);
                        if (rvio) sprite.SetAttribute("sheet", layer.SheetName);

                        sprite.SetAttribute("x", layer.X.ToString());
                        sprite.SetAttribute("y", layer.Y.ToString());
                        if (rvio)
                        {
                            sprite.SetAttribute("scaleX", layer.ScaleX.ToString());
                            sprite.SetAttribute("scaleY", layer.ScaleY.ToString());
                            sprite.SetAttribute("angle" , layer.Angle .ToString());
                            sprite.SetAttribute("flipX" , layer.FlipX .ToString());
                            sprite.SetAttribute("flipY" , layer.FlipY .ToString());
                        }

                        child.AppendChild(sprite);
                    }

                    element.AppendChild(child);
                }

                doc.AppendChild(element);
            }
            else
            {
                throw new NotSupportedException("Invalid segment");
            }

            return doc;
        }

        public static void Write(XmlDocument doc, Stream output)
        {
            XmlNode root = doc.LastChild ?? throw new ArgumentException("Invalid XML file", nameof(doc));

            if (root.Name == "spritesheet")
            {
                using var writer = new DatWriter(output, rvio: false, leaveOpen: true);

                var sprt = new SpriteSheet(tex: root.Attributes?["texture"]?.Value);

                var list = doc.SelectNodes("spritesheet/sprite");
                if (list != null)
                    foreach (XmlNode node in list)
                    {
                        var sprite = new Sprite(x     : short.Parse(node.Attributes?["x"]     ?.Value ?? "0"),
                                                y     : short.Parse(node.Attributes?["y"]     ?.Value ?? "0"),
                                                width : short.Parse(node.Attributes?["width"] ?.Value ?? "0"),
                                                height: short.Parse(node.Attributes?["height"]?.Value ?? "0"),
                                                pivotX: short.Parse(node.Attributes?["pivotX"]?.Value ?? "0"),
                                                pivotY: short.Parse(node.Attributes?["pivotY"]?.Value ?? "0"));
                        sprt.Sprites.Add(node.Attributes?["name"]?.Value ?? throw new ArgumentException("Invalid XML file", nameof(doc)), sprite);
                    }

                sprt.Write(writer, beginEnd: true);
            }
            else if (root.Name == "font")
            {
                using var writer = new DatWriter(output, rvio: false, leaveOpen: true);

                var font = new Font(tex     :             root.Attributes?["texture"] ?.Value,
                                    leading : short.Parse(root.Attributes?["leading"] ?.Value ?? "0"),
                                    tracking: short.Parse(root.Attributes?["tracking"]?.Value ?? "0"));

                var list = doc.SelectNodes("font/character");
                if (list != null)
                    foreach (XmlNode node in list)
                    {
                        var character = new Character(x:      short.Parse(node.Attributes?["x"]     ?.Value ?? "0"),
                                                      y:      short.Parse(node.Attributes?["y"]     ?.Value ?? "0"),
                                                      width:  short.Parse(node.Attributes?["width"] ?.Value ?? "0"),
                                                      height: short.Parse(node.Attributes?["height"]?.Value ?? "0"),
                                                      pivotY: short.Parse(node.Attributes?["pivotY"]?.Value ?? "0"));
                        font.Characters.Add((char)short.Parse(node.Attributes?["char"]?.Value ?? throw new ArgumentException("Invalid XML file", nameof(doc)), System.Globalization.NumberStyles.HexNumber), character);
                    }

                font.Write(writer, beginEnd: true);
            }
            else if (root.Name == "localization")
            {
                using var writer = new DatWriter(output, rvio: false, leaveOpen: true);

                var text = new Localization();

                var langs = doc.SelectSingleNode("localization/langs")?.ChildNodes;
                if (langs != null)
                    foreach (XmlNode node in langs)
                    {
                        text.Languages.Add(node.Name);
                    }

                var texts = doc.SelectNodes("localization/texts/text");
                if (texts != null)
                    foreach (XmlNode node in texts)
                    {
                        foreach (var lang in text.Languages)
                        {
                            string id = node.Attributes?["id"]?.Value ?? throw new ArgumentException($"Invalid XML file", nameof(doc));

                            text.TextIDs.Add(id);

                            var value = node.SelectSingleNode(lang);
                            if (value != null)
                                text.SetString(lang, id, value.InnerText);
                        }
                    }

                text.Write(writer, beginEnd: true);
            }
            else if (root.Name == "composprites")
            {
                using var writer = new DatWriter(output, rvio: bool.Parse(root.Attributes?["rvio"]?.Value ?? "false"), leaveOpen: true);

                var comp = new CompoSprites
                {
                    Version = short.Parse(root.Attributes?["version"]?.Value ?? "1")
                };

                var list = doc.SelectNodes("composprites/composprite");
                if (list != null)
                    foreach (XmlNode node in list)
                    {
                        var layers = new List<CompoSpriteLayer>();

                        var sprites = node.SelectNodes("layer");
                        if (sprites != null)
                            foreach (XmlNode sprite in sprites)
                            {
                                CompoSpriteLayer layer;
                                if (!writer.Rvio)
                                {
                                    layer = new(sprite:             sprite.Attributes?["sprite"]?.Value ?? ""  ,
                                                x     : short.Parse(sprite.Attributes?["x"]     ?.Value ?? "0"),
                                                y     : short.Parse(sprite.Attributes?["y"]     ?.Value ?? "0"));
                                }
                                else
                                {
                                    layer = new(sprite:             sprite.Attributes?["sprite"]?.Value ?? ""      ,
                                                sheet :             sprite.Attributes?["sheet"] ?.Value ?? ""      ,
                                                x     : short.Parse(sprite.Attributes?["x"]     ?.Value ?? "0")    ,
                                                y     : short.Parse(sprite.Attributes?["y"]     ?.Value ?? "0")    ,
                                                scaleX: float.Parse(sprite.Attributes?["scaleX"]?.Value ?? "1")    ,
                                                scaleY: float.Parse(sprite.Attributes?["scaleY"]?.Value ?? "1")    ,
                                                angle : float.Parse(sprite.Attributes?["angle"] ?.Value ?? "0")    ,
                                                flipX : bool .Parse(sprite.Attributes?["flipX"] ?.Value ?? "false"),
                                                flipY : bool .Parse(sprite.Attributes?["flipY"] ?.Value ?? "false"));
                                }
                                layers.Add(layer);
                            }

                        comp.Composites.Add(node.Attributes?["name"]?.Value ?? throw new ArgumentException("Invalid XML file", nameof(doc)), layers);
                    }

                comp.Write(writer, beginEnd: true);
            }
            else
            {
                throw new ArgumentException($"Invalid XML file", nameof(doc));
            }

        }

    }
}
