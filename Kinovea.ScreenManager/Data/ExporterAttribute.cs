using Kinovea.ScreenManager.Languages;
using System;

namespace Kinovea.ScreenManager.Data
{
    public class ExporterAttribute : Attribute
    {
        public ExporterAttribute(string name, string defaultExtension)
        {
            this.Name = name;
            this.DefaultExtension = defaultExtension;
        }

        public string DefaultExtension { get; private set; }

        public string DisplayName
        {
            get
            {
                return ScreenManagerLang.ResourceManager.GetString(this.Name, ScreenManagerLang.Culture);
            }
        }

        public Type ExporterType { get; set; }

        public string Name { get; private set; }

        public override string ToString()
        {
            return this.DisplayName;
        }
    }
}