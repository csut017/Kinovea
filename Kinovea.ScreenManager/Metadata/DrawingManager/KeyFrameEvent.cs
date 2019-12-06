using System;
using System.Xml;

namespace Kinovea.ScreenManager
{
    public class KeyFrameEvent
    {
        public string Name { get; set; }

        public void WriteXml(XmlWriter w)
        {
            w.WriteAttributeString("name", this.Name);
        }

        public static KeyFrameEvent ReadFromXml(XmlReader r)
        {
            var evt = new KeyFrameEvent
            {
                Name = r.GetAttribute("name")
            };
            return evt;
        }
    }
}