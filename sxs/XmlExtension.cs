using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;

namespace sxs
{
    public static class XmlExtension
    {
        public static void WriteTabs(this XmlWriter writer, int countTabs)
        {
            writer.WriteWhitespace("\n");
            for (int i = 0; i < countTabs; i++, writer.WriteWhitespace("\t")) ;
        }

        public static void WriteTabs(this XmlWriter writer, int countTabs, string whitespace)
        {
            writer.WriteWhitespace("\n");
            for (int i = 0; i < countTabs; i++, writer.WriteWhitespace(whitespace)) ;
        }
    }
}
