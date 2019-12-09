using Kinovea.ScreenManager.Languages;
using System;
using System.Linq;

namespace Kinovea.ScreenManager.Data
{
    public abstract class Exporter
    {
        private bool _isCancelling;

        public Exporter()
        {
            this.Filter = ScreenManagerLang.FileFilter_Default;
            this.Title = ScreenManagerLang.dlgExportSpreadsheet_Title;
            this.AllowedSettings = new ExportSettings();
            this.DefaultSettings = new ExportSettings();
        }

        public ExportSettings AllowedSettings
        {
            get; protected set;
        }

        public ExportSettings DefaultSettings
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

        public bool IsCancelling
        {
            get
            {
                lock (this)
                {
                    return this._isCancelling;
                }
            }

            set
            {
                lock (this)
                {
                    this._isCancelling = value;
                }
            }
        }

        public abstract void Export(Metadata metadata, ExportSettings options);

        protected static double ConvertTimeCodeToSeconds(string timeCode)
        {
            var seconds = 0.0;
            var parts = timeCode.Split(':');
            var partNum = 0;
            for (var loop = parts.Length - 1; loop >= 0; loop--)
            {
                switch (partNum)
                {
                    case 0:
                        seconds += Convert.ToDouble(parts[loop]);
                        break;

                    case 1:
                        seconds += (Convert.ToDouble(parts[loop]) * 60);
                        break;

                    case 2:
                        seconds += (Convert.ToDouble(parts[loop]) * 3600);
                        break;
                }
                partNum++;
            }

            return seconds;
        }

        public void CancelAsync()
        {
            this.IsCancelling = true;
        }
    }
}