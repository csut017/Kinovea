using ClosedXML.Excel;
using Kinovea.ScreenManager.Data.Rtf;
using Kinovea.ScreenManager.Languages;
using System.Collections.Generic;
using System.Linq;

namespace Kinovea.ScreenManager.Data
{
    [Exporter("DataExport_Excel", "xlsx")]
    public class ExcelExporter
        : Exporter
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ExcelExporter()
        {
            this.Filter = ScreenManagerLang.DataExport_Excel_Filter;
            this.Title = ScreenManagerLang.DataExport_Excel_Title;

            // Allowed settings
            this.AllowedSettings.IncludeComments = true;
            this.AllowedSettings.IncludeEvents = true;
            this.AllowedSettings.OpenAfterSave = true;

            // Default settings
            this.DefaultSettings.IncludeComments = true;
            this.DefaultSettings.IncludeEvents = true;
            this.DefaultSettings.OpenAfterSave = true;
        }

        public override void Export(Metadata metadata, ExportSettings options)
        {
            _log.Debug("Starting Excel export");
            var output = new XLWorkbook();
            var ws = output.Worksheets.Add("Frames");

            // Add the headers
            _log.Debug("Adding headers");
            var column = 2;
            ws.Cell(1, 1).Value = "Name";
            ws.Cell(1, 2).Value = "Time";
            if (options.IncludeComments)
            {
                ++column;
                ws.Cell(1, 3).Value = "Comments";
            }


            var eventColumns = new Dictionary<string, int>();
            if (options.IncludeEvents)
            {
                _log.Debug("Scanning for events");
                var events = new Dictionary<string, bool>();
                foreach (var kf in metadata.Keyframes)
                {
                    foreach (var evt in kf.Events)
                    {
                        events[evt.Name] = true;
                    }
                }

                foreach (var evt in events.Keys.OrderBy(e => e))
                {
                    ++column;
                    ws.Cell(1, column).Value = evt;
                    eventColumns[evt] = column;
                }
            }

            // Add each row of data
            var row = 1;
            var count = 0;
            var total = metadata.Keyframes.Count;
            _log.Debug("Adding rows");
            foreach (var kf in metadata.Keyframes)
            {
                row++;
                ws.Cell(row, 1).Value = "'" + kf.Title;
                ws.Cell(row, 2).Value = ConvertTimeCodeToSeconds(kf.TimeCode);
                if (options.IncludeComments)
                {
                    var cell = ws.Cell(row, 3);
                    cell.Style.Alignment.WrapText = true;
                    AppendRichText(kf.Comments, cell);
                }

                if (options.IncludeEvents)
                {
                    foreach (var evt in kf.Events)
                    {
                        var evtCol = eventColumns[evt.Name];
                        ws.Cell(row, evtCol).Value = true;
                    }
                }

                ++count;
                this.ReportProgress(count * 100 / total);
                if (this.IsCancelling) return;
            }

            // Generate the table
            _log.Debug("Generating table");
            var range = ws.Range(1, 1, row, column);
            range.CreateTable("Frames");

            // Save everything
            _log.Debug("Saving export");
            output.SaveAs(options.Filename);
            if (this.IsCancelling) return;

            // Open the file if requested
            if (options.OpenAfterSave)
            {
                _log.Debug("Opening export");
                System.Diagnostics.Process.Start(options.Filename);
            }
        }

        private static void AppendRichText(string text, IXLCell cell)
        {
            if (string.IsNullOrEmpty(text)) return;
            Processor.Process(text, (txt, fmt) =>
            {
                cell.RichText
                    .AddText(txt)
                    .SetBold(fmt.IsBold)
                    .SetUnderline(fmt.IsUnderline ? XLFontUnderlineValues.Single : XLFontUnderlineValues.None)
                    .SetItalic(fmt.IsItalic)
                    .SetStrikethrough(fmt.IsStrikethrough);
            });
        }
    }
}