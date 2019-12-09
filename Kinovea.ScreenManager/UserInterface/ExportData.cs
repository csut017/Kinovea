using Kinovea.ScreenManager.Data;
using Kinovea.ScreenManager.Languages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Kinovea.ScreenManager.UserInterface
{
    public partial class ExportData : Form
    {
        private static readonly Lazy<ICollection<Data.ExporterAttribute>> exporters = new Lazy<ICollection<Data.ExporterAttribute>>(ScanForExporters);

        private static ICollection<ExporterAttribute> ScanForExporters()
        {
            var exporters = typeof(Data.Exporter)
                .Assembly
                .GetTypes()
                .Select(t =>
                {
                    if (!(t.GetCustomAttributes(typeof(Data.ExporterAttribute), false).FirstOrDefault() is Data.ExporterAttribute attrib)) return null;
                    attrib.ExporterType = t;
                    return attrib;
                })
                .Where(t => t != null)
                .OrderBy(a => a.DisplayName)
                .ToList();
            return exporters;
        }

        public ExportData()
        {
            InitializeComponent();
            this.UpdateLanguage();
            this.dataFormat.Items.AddRange(exporters.Value.ToArray());
            if (this.dataFormat.Items.Count > 0)
            {
                this.dataFormat.SelectedIndex = 0;
                this.InitialiseOptions();
            }
        }

        private void InitialiseOptions()
        {
            var attrib = this.dataFormat.SelectedItem as Data.ExporterAttribute;
            var exporter = Activator.CreateInstance(attrib.ExporterType) as Data.Exporter;

            // Allowed settings
            this.optionIncludeComments.Enabled = exporter.AllowedSettings.IncludeComments;
            this.optionIncludeEvents.Enabled = exporter.AllowedSettings.IncludeEvents;
            this.optionOpenAfterSave.Enabled = exporter.AllowedSettings.OpenAfterSave;

            // Default settings
            this.optionIncludeComments.Checked = exporter.DefaultSettings.IncludeComments;
            this.optionIncludeEvents.Checked = exporter.DefaultSettings.IncludeEvents;
            this.optionOpenAfterSave.Checked = exporter.DefaultSettings.OpenAfterSave;
        }

        public Data.Exporter Exporter
        {
            get
            {
                var attrib = this.dataFormat.SelectedItem as Data.ExporterAttribute;
                var exporter = Activator.CreateInstance(attrib.ExporterType) as Data.Exporter;
                return exporter;
            }
        }

        public Data.ExportSettings Options
        {
            get
            {
                return new Data.ExportSettings
                {
                    IncludeComments = this.optionIncludeComments.Checked,
                    IncludeEvents = this.optionIncludeEvents.Checked,
                    OpenAfterSave = this.optionOpenAfterSave.Checked,
                };
            }
        }

        private void UpdateLanguage()
        {
            this.Text = ScreenManagerLang.dlgExportData_Title;
            this.dataFormatLabel.Text = ScreenManagerLang.dlgExportData_Format;
            this.optionsLabel.Text = ScreenManagerLang.dlgExportData_Options;
            this.optionIncludeComments.Text = ScreenManagerLang.dlgExportData_IncludeComments;
            this.optionIncludeEvents.Text = ScreenManagerLang.dlgExportData_IncludeEvents;
            this.optionOpenAfterSave.Text = ScreenManagerLang.dlgExportData_OpenAfterSave;

            this.cancelButton.Text = ScreenManagerLang.dlgExportData_Cancel;
            this.exportButton.Text = ScreenManagerLang.dlgExportData_Export;
        }

        private void dataFormat_SelectedValueChanged(object sender, EventArgs e)
        {
            this.InitialiseOptions();
        }
    }
}