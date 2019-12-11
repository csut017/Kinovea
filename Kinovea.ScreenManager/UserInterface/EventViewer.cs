using Kinovea.ScreenManager.Languages;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Kinovea.ScreenManager.UserInterface
{
    public partial class EventViewer : Form
    {
        private readonly TextBox[] eventNames = new TextBox[10];
        private readonly PictureBox[] eventHistories = new PictureBox[10];
        private PlayerScreen _activeScreen;

        public EventViewer()
        {
            InitializeComponent();
            this.UpdateLanguage();

            for (var loop = 0; loop < 10; loop++)
            {
                var newTextBox = new TextBox
                {
                    Location = new Point(3, 3 + (26 * loop)),
                    Size = new Size(263, 20),
                    TabIndex = loop,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                this.mainContainer.Panel1.Controls.Add(newTextBox);
                this.eventNames[loop] = newTextBox;

                var newPictureBox = new PictureBox
                {
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    Location = new Point(3, 3 + (26 * loop)),
                    Size = new Size(532, 20),
                    TabIndex = loop,
                    TabStop = false
                };
                this.mainContainer.Panel2.Controls.Add(newPictureBox);
                this.eventHistories[loop] = newPictureBox;
            }
        }

        public PlayerScreen ActiveScreen
        {
            get => this._activeScreen;
            set
            {
                this._activeScreen = value;
                this.UpdateDisplay();
            }
        }

        private void UpdateDisplay()
        {
            if ((this._activeScreen == null) || (this._activeScreen.FrameServer == null)) return;

            var events = this._activeScreen.FrameServer.Metadata.Events.ToArray();
            for (var loop = 0; loop < 10; loop++)
            {
                this.eventNames[loop].Text = events[loop].Title;
            }
        }

        private void EventViewer_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Only shut down the event viewer if the application is closing
            if ((e.CloseReason == CloseReason.WindowsShutDown) || (e.CloseReason == CloseReason.ApplicationExitCall)) return;

            // Otherwise just hide it so it can be re-used later
            e.Cancel = true;
            this.Hide();
        }

        private void UpdateLanguage()
        {
            this.Text = ScreenManagerLang.dlgEventViewer_Title;
        }
    }
}