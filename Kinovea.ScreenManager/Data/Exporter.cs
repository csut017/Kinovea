using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager.Data
{
    public abstract class Exporter
    {
        public Exporter()
        {
            this.Filter = ScreenManagerLang.FileFilter_Default;
            this.Title = ScreenManagerLang.dlgExportSpreadsheet_Title;
            this.AllowedSettings = new ExportSettings();
        }

        public ExportSettings AllowedSettings
        {
            get; protected set;
        }

        public string Filter
        {
            get; protected set;
        }

        public string Title
        {
            get; protected set;
        }

        public abstract void Export(string filename, Metadata metadata, ExportSettings options);
    }
}