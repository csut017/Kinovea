using Kinovea.ScreenManager.Languages;
using System;

namespace Kinovea.ScreenManager.Data
{
    [Exporter("DataExport_Excel")]
    public class ExcelExporter
        : Exporter
    {
        public ExcelExporter()
        {
            this.AllowedSettings.IncludeEvents = true;
            this.Filter = ScreenManagerLang.DataExport_Excel_Filter;
            this.Title = ScreenManagerLang.DataExport_Excel_Title;
        }
        public override void Export(string filename, Metadata metadata, ExportSettings options)
        {
            throw new NotImplementedException();
        }
    }
}
