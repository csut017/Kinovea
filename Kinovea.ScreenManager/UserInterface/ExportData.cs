using Kinovea.ScreenManager.Data;
using Kinovea.ScreenManager.Languages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Kinovea.ScreenManager.UserInterface
{
    public partial class ExportData : Form
    {
        private static readonly Lazy<ICollection<Data.ExporterAttribute>> _exporters = new Lazy<ICollection<Data.ExporterAttribute>>(ScanForExporters);
        private readonly FrameServerPlayer _frameServer;
        private readonly BackgroundWorker _worker;
        private Data.Exporter _exporter;
        private string defaultFileName = string.Empty;

        public ExportData(FrameServerPlayer frameServer)
        {
            this._frameServer = frameServer;

            // Initialise the UI
            InitializeComponent();
            this.UpdateLanguage();

            // Initialise the background worker
            this._worker = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            this._worker.DoWork += this.DoExport;
            this._worker.RunWorkerCompleted += this.ExportCompleted;
            this._worker.ProgressChanged += this.ExportProgressChanged;

            // Load the exporters
            this.dataFormat.Items.AddRange(_exporters.Value.ToArray());
            if (this.dataFormat.Items.Count > 0)
            {
                this.dataFormat.SelectedIndex = 0;
                this.InitialiseOptions();
                this.UpdateFileName();
            }
        }

        private void ExportProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.exportProgress.Value = e.ProgressPercentage;
        }

        private Data.Exporter Exporter
        {
            get
            {
                var attrib = this.dataFormat.SelectedItem as Data.ExporterAttribute;
                var exporter = Activator.CreateInstance(attrib.ExporterType) as Data.Exporter;
                return exporter;
            }
        }

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

        private void cancelButton_Click(object sender, EventArgs e)
        {
            if (this._worker.IsBusy)
            {
                this._exporter.CancelAsync();
            }
            else
            {
                this.Close();
            }
        }

        private void CheckCanExport()
        {
            this.exportButton.Enabled = !string.IsNullOrEmpty(this.filename.Text);
        }

        private void dataFormat_SelectedValueChanged(object sender, EventArgs e)
        {
            this.InitialiseOptions();
            this.UpdateFileName();
        }

        private void DoExport(object sender, DoWorkEventArgs e)
        {
            var opts = e.Argument as Data.ExportSettings;
            this._exporter.ProgressUpdate += (o, evt) => this._worker.ReportProgress(evt.ProgressPercentage);
            try
            {
                this._exporter.Export(this._frameServer.Metadata, opts);
            }
            catch (Exception error)
            {
                e.Result = error;
            }
        }

        private void exportButton_Click(object sender, EventArgs e)
        {
            this.dataFormat.Enabled = false;
            this.filename.Enabled = false;
            this.optionsLabel.Enabled = false;
            this.exportButton.Enabled = false;
            this.exportProgress.Value = 0;
            this.exportProgress.Visible = true;

            this._exporter = this.Exporter;
            var opts = new Data.ExportSettings
            {
                IncludeComments = this.optionIncludeComments.Checked,
                IncludeEvents = this.optionIncludeEvents.Checked,
                OpenAfterSave = this.optionOpenAfterSave.Checked,
                Filename = this.filename.Text
            };
            this._worker.RunWorkerAsync(opts);
        }

        private void ExportCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.dataFormat.Enabled = true;
            this.filename.Enabled = true;
            this.optionsLabel.Enabled = true;
            this.exportButton.Enabled = true;
            this.exportProgress.Visible = false;

            if (e.Result is Exception error)
            {
                MessageBox.Show(error.Message, ScreenManagerLang.Error_ExportFailed, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (!this._exporter.IsCancelling)
            {
                this.Close();
            }

            this._exporter.IsCancelling = false;
        }

        private void ExportData_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = this._worker.IsBusy;
        }

        private void filename_TextChanged(object sender, EventArgs e)
        {
            this.CheckCanExport();
        }

        private void findFileButton_Click(object sender, EventArgs e)
        {
            var exporter = this.Exporter;
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Title = exporter.Title,
                RestoreDirectory = true,
                Filter = exporter.Filter,
                FilterIndex = 1,
                FileName = this.filename.Text
            };
            if (saveFileDialog.ShowDialog() != DialogResult.OK || string.IsNullOrEmpty(saveFileDialog.FileName)) return;
            this.filename.Text = saveFileDialog.FileName;
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

        private void UpdateFileName()
        {
            if (this.dataFormat.SelectedItem is Data.ExporterAttribute attrib)
            {
                var newFileName = Path.ChangeExtension(this._frameServer.Metadata.FullPath, attrib.DefaultExtension);
                if (this.filename.Text == this.defaultFileName)
                {
                    this.filename.Text = newFileName;
                }

                this.defaultFileName = newFileName;
            }
        }

        private void UpdateLanguage()
        {
            this.Text = ScreenManagerLang.dlgExportData_Title;
            this.dataFormatLabel.Text = ScreenManagerLang.dlgExportData_Format;
            this.filenameLabel.Text = ScreenManagerLang.dlgExportData_FileName;
            this.optionsLabel.Text = ScreenManagerLang.dlgExportData_Options;
            this.optionIncludeComments.Text = ScreenManagerLang.dlgExportData_IncludeComments;
            this.optionIncludeEvents.Text = ScreenManagerLang.dlgExportData_IncludeEvents;
            this.optionOpenAfterSave.Text = ScreenManagerLang.dlgExportData_OpenAfterSave;

            this.cancelButton.Text = ScreenManagerLang.dlgExportData_Cancel;
            this.exportButton.Text = ScreenManagerLang.dlgExportData_Export;
        }
    }
}