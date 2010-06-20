/*
Copyright � Joan Charmant 2008.
joan.charmant@gmail.com 
 
This file is part of Kinovea.

Kinovea is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License version 2 
as published by the Free Software Foundation.

Kinovea is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Kinovea. If not, see http://www.gnu.org/licenses/.

*/

using Kinovea.ScreenManager.Languages;
using System;
using System.Drawing;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public partial class formKeyframeComments : Form
    {

        // This is an info box common to all keyframes.
        // It can be activated or deactivated by the user.
        // When activated, it only display itself if we are stopped on a keyframe.
        // The content is then updated with keyframe content.

        #region Properties
        public bool UserActivated
        {
            get { return m_bUserActivated; }
            set { m_bUserActivated = value; }
        }
        
        #endregion

        #region Members
        private bool m_bUserActivated;
        private Keyframe m_Keyframe;
        private PlayerScreenUserInterface m_psui;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Contructors
        public formKeyframeComments(PlayerScreenUserInterface _psui)
        {
            InitializeComponent();
            RefreshUICulture();
            m_bUserActivated = false;
            m_psui = _psui;
        }
        #endregion

        #region Public Interface
        public void UpdateContent(Keyframe _keyframe)
        {
            // We keep only one window, and the keyframe it displays is swaped.

            if (m_Keyframe != _keyframe)
            {
                SaveInfos();
                m_Keyframe = _keyframe;
                LoadInfos();
            }
        }
        public void CommitChanges()
        {
            SaveInfos();  
        }
        public void RefreshUICulture()
        {
            this.Text = "   " + ScreenManagerLang.dlgKeyframeComment_Title;
        }
        #endregion

        #region Form event handlers
        private void formKeyframeComments_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // If the user close the mini window we only hide it.
                e.Cancel = true;
                m_bUserActivated = false;
                SaveInfos();
                ActivateKeyboardHandler();
            }
            else
            {
                // == CloseReason.FormOwnerClosing
            }

            this.Visible = false;
        }
        private void formKeyframeComments_MouseEnter(object sender, EventArgs e)
        {
            DeactivateKeyboardHandler();
        }
        private void formKeyframeComments_MouseLeave(object sender, EventArgs e)
        {
            CheckMouseLeave();
        }
        #endregion
        
        #region Styling event handlers
        private void btnBold_Click(object sender, EventArgs e)
        {
        	int style = GetSelectionStyle();
        	style = rtbComment.SelectionFont.Bold ? style - (int)FontStyle.Bold : style + (int)FontStyle.Bold;
        	rtbComment.SelectionFont = new Font(rtbComment.SelectionFont.FontFamily, rtbComment.SelectionFont.Size, (FontStyle)style);
        }
        private void btnItalic_Click(object sender, EventArgs e)
        {
        	int style = GetSelectionStyle();
        	style = rtbComment.SelectionFont.Italic ? style - (int)FontStyle.Italic : style + (int)FontStyle.Italic;
        	rtbComment.SelectionFont = new Font(rtbComment.SelectionFont.FontFamily, rtbComment.SelectionFont.Size, (FontStyle)style);
        }
        private void btnUnderline_Click(object sender, EventArgs e)
        {
        	int style = GetSelectionStyle();
        	style = rtbComment.SelectionFont.Underline ? style - (int)FontStyle.Underline : style + (int)FontStyle.Underline;
        	rtbComment.SelectionFont = new Font(rtbComment.SelectionFont.FontFamily, rtbComment.SelectionFont.Size, (FontStyle)style);
        }
        private void btnStrike_Click(object sender, EventArgs e)
        {
        	int style = GetSelectionStyle();
        	style = rtbComment.SelectionFont.Strikeout ? style - (int)FontStyle.Strikeout : style + (int)FontStyle.Strikeout;
        	rtbComment.SelectionFont = new Font(rtbComment.SelectionFont.FontFamily, rtbComment.SelectionFont.Size, (FontStyle)style);
        }
        private void btnForeColor_Click(object sender, EventArgs e)
        {
        	rtbComment.SelectionColor = Color.Red;
        }
        private void btnBackColor_Click(object sender, EventArgs e)
        {
        	rtbComment.SelectionBackColor = Color.Yellow;
        }
        #endregion

        #region Lower level helpers
        private void CheckMouseLeave()
        {
            // We really leave only if we left the whole control.
            // we have to do this because placing the mouse over the text boxes will raise a
            // formKeyframeComments_MouseLeave event...
            if (!this.Bounds.Contains(Control.MousePosition))
            {
                ActivateKeyboardHandler(); 
            }
        }
        private void DeactivateKeyboardHandler()
        {
            // Mouse enters the info box : deactivate the keyboard handling for the screens
            // so we can use <space>, <return>, etc. here.
            DelegatesPool dp = DelegatesPool.Instance();
            if (dp.DeactivateKeyboardHandler != null)
            {
                dp.DeactivateKeyboardHandler();
            }
        }
        private void ActivateKeyboardHandler()
        {
            // Mouse leave the info box : reactivate the keyboard handling for the screens
            // so we can use <space>, <return>, etc. as player shortcuts.
            // This is sometimes strange. You put the mouse away to start typing, 
            // and the first carriage return triggers the playback leaving the key image.
            
            DelegatesPool dp = DelegatesPool.Instance();
            if (dp.ActivateKeyboardHandler != null)
            {
                dp.ActivateKeyboardHandler();
            }
        }
        private void LoadInfos()
        {
            // Update
            txtTitle.Text = m_Keyframe.Title;
            rtbComment.Clear();
            rtbComment.Rtf = m_Keyframe.CommentRtf;
        }
        private void SaveInfos()
        {
            // Commit changes to the keyframe
            // This must not be called at each info modification otherwise the update routine breaks...
            
            log.Debug("Saving comment and title");
            if (m_Keyframe != null)
            {
	            m_Keyframe.CommentRtf = rtbComment.Rtf;
	
            	if(m_Keyframe.Title != txtTitle.Text)
            	{
            		m_Keyframe.Title = txtTitle.Text;	
            		m_psui.OnKeyframesTitleChanged();
            	}
            }
        }
        private int GetSelectionStyle()
        {
        	// Combine all the styles into an int, to have generic toggles methods.
        	int bold = rtbComment.SelectionFont.Bold ? (int)FontStyle.Bold : 0;
        	int italic = rtbComment.SelectionFont.Italic ? (int)FontStyle.Italic : 0;
        	int underline = rtbComment.SelectionFont.Underline ? (int)FontStyle.Underline : 0;
        	int strikeout = rtbComment.SelectionFont.Strikeout ? (int)FontStyle.Strikeout : 0;
        	
        	return bold + italic + underline + strikeout;
        }
        private void LogCurrentSelection()
        {
        	log.Debug(String.Format("Selection font name:{0}", rtbComment.SelectionFont.Name));
        	log.Debug(String.Format("Selection font size:{0}", rtbComment.SelectionFont.Size));
        	log.Debug(String.Format("Selection font bold:{0}", rtbComment.SelectionFont.Bold));
        	log.Debug(String.Format("Selection font italic:{0}", rtbComment.SelectionFont.Italic));
        	log.Debug(String.Format("Selection font underline:{0}", rtbComment.SelectionFont.Underline));
        	log.Debug(String.Format("Selection font strikeout:{0}", rtbComment.SelectionFont.Strikeout));
        	
        	log.Debug(String.Format("Selection back color: {0}", rtbComment.SelectionBackColor.ToString()));
        	log.Debug(String.Format("Selection fore color: {0}", rtbComment.SelectionColor.ToString()));
        	
        	/*log.Debug(String.Format("regular : {0}, bold : {1}, italic : {2}, underline : {3}, strikeout : {4}", 
        	                        (int)FontStyle.Regular, (int)FontStyle.Bold, (int)FontStyle.Italic, (int)FontStyle.Underline, (int)FontStyle.Strikeout));
        	log.Debug(String.Format("style : {0}", rtbComment.SelectionFont.Style));*/
        }
        #endregion

    }
}