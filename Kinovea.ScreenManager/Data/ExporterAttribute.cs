using Kinovea.ScreenManager.Languages;
using System;

namespace Kinovea.ScreenManager.Data
{
    public class ExporterAttribute : Attribute
    {
        public ExporterAttribute(string name)
        {
            this.Name = name;
        }

        public Type ExporterType { get; set; }

        public string Name { get; private set; }

        public string DisplayName
        {
            get
            {
                return ScreenManagerLang.ResourceManager.GetString(this.Name, ScreenManagerLang.Culture);
            }
        }

        public override string ToString()
        {
            return this.DisplayName;
        }
    }
}