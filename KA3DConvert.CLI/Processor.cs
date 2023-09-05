using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace KA3DConvert.CLI
{
    internal static class Processor
    {
        private static bool PathIsXml([NotNullWhen(true)] string? path)
        {
            return Path.GetExtension(path) == ".xml";
        }


        public static void Convert(string input, string? output)
        {
            using var fs = File.OpenRead(input);

            if (PathIsXml(input))
            {
                output ??= Path.ChangeExtension(input, "dat");

                var doc = new XmlDocument();
                doc.Load(fs);

                using var fs1 = new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.Write);
                XmlSegment.Write(doc, fs1);

            }
            else
            {
                output ??= Path.ChangeExtension(input, "xml");

                var doc = XmlSegment.Read(fs);

                using var fs1 = File.Create(output);
                doc.Save(fs1);

            }

        }


    }
}
