﻿#region License
/*
Copyright © Joan Charmant 2013.
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
#endregion
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

using Kinovea.Camera;
using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    public partial class ThumbnailCamera : UserControl
    {
        #region Events
        public event EventHandler LaunchCamera;
        public event EventHandler CameraSelected;
        #endregion
        
        #region Properties
        public CameraSummary Summary { get; private set;}
        public Bitmap Image { get; private set;}
        #endregion
        
        #region Members
        private ContextMenuStrip  popMenu = new ContextMenuStrip();
        private ToolStripMenuItem mnuLaunch = new ToolStripMenuItem();
        private ToolStripMenuItem mnuRename = new ToolStripMenuItem();
        private bool selected;
        private static readonly Pen penSelected = new Pen(Color.DodgerBlue, 2);
        private static readonly Pen penUnselected = new Pen(Color.Silver, 2);
        private static readonly Pen penShadow = new Pen(Color.Lavender, 2);
        #endregion

        public ThumbnailCamera(CameraSummary summary)
        {
            InitializeComponent();
            this.Summary = summary;
            BuildContextMenus();
            RefreshUICulture();
        }
        
        #region Public methods
        public void SetSize(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            RatioStretch();
        }
        
        public void UpdateImage(Bitmap image)
        {
            this.Image = image;
            this.Invalidate();
        }
        public void RefreshUICulture()
        {
            lblAlias.Text = Summary.Alias;
            mnuLaunch.Text = ScreenManagerLang.mnuThumbnailPlay;
            mnuRename.Text = ScreenManagerLang.mnuThumbnailRename;
        }
        public void SetUnselected()
        {
            // This method does NOT trigger an event to notify the container.
            selected = false;
            this.Invalidate();
        }
        public void SetSelected()
        {
            // This method triggers an event to notify the container.
            Summary.Manager.GetSingleImage(Summary);
            
            if (selected)
                return;

            selected = true;
            this.Invalidate();
            
            if (CameraSelected != null)
                CameraSelected(this, EventArgs.Empty);
        }
        #endregion
        
        #region Private methods
        private void ThumbnailCamera_Paint(object sender, PaintEventArgs e)
        {
            if(Image == null)
                return;
            
            // Shadow
            e.Graphics.DrawLine(penShadow, picBox.Left + picBox.Width + 1, picBox.Top + penShadow.Width, picBox.Left + picBox.Width + 1, picBox.Top + picBox.Height + penShadow.Width);
            e.Graphics.DrawLine(penShadow, picBox.Left + penShadow.Width, picBox.Top + picBox.Height + 1, picBox.Left + penShadow.Width + picBox.Width, picBox.Top + picBox.Height + 1);
        }
        
        private void BuildContextMenus()
        {
            mnuLaunch.Image = Properties.Resources.film_go;
            mnuLaunch.Click += new EventHandler(mnuLaunch_Click);
            mnuRename.Image = Properties.Resources.rename;
            mnuRename.Click += new EventHandler(mnuRename_Click);
            popMenu.Items.AddRange(new ToolStripItem[] { mnuLaunch, mnuRename});
            this.ContextMenuStrip = popMenu;
        }
        
        private void AllControls_DoubleClick(object sender, EventArgs e)
        {
            if (LaunchCamera == null)
                return;
        
            this.Cursor = Cursors.WaitCursor;
            LaunchCamera(this, EventArgs.Empty);
            this.Cursor = Cursors.Default;
        }
        
        private void AllControls_Click(object sender, EventArgs e)
        {
            SetSelected();
        }

        private void mnuRename_Click(object sender, EventArgs e)
        {
            
        }
        
        private void mnuLaunch_Click(object sender, EventArgs e)
        {
            if (LaunchCamera == null)
                return;
        
            this.Cursor = Cursors.WaitCursor;
            LaunchCamera(this, EventArgs.Empty);
            this.Cursor = Cursors.Default;
        }
        
        private void RatioStretch()
        {
            lblAlias.Visible = this.Width >= 110;
            
            if(Image == null)
            {
                picBox.Height = (picBox.Width * 3) / 4;
                this.Invalidate();
                return;
            }
            
            int imageMargin = 6;
            int containerWidth = this.Width - imageMargin;
            int containerHeight = this.Height - lblAlias.Height - imageMargin;
            
            float widthRatio = (float)Image.Width / containerWidth;
            float heightRatio = (float)Image.Height / containerHeight;
            float ratio = Math.Max(widthRatio, heightRatio);
            picBox.Width = (int)(Image.Width / ratio);
            picBox.Height = (int)(Image.Height / ratio);
            
            picBox.Left = (this.Width - picBox.Width)/2;
            picBox.Top = (this.Height - lblAlias.Height - picBox.Height)/2;
            
            this.Invalidate();
        }
        
        private void PicBox_Paint(object sender, PaintEventArgs e)
        {
            // Configure for speed. These are thumbnails anyway.
            e.Graphics.PixelOffsetMode = PixelOffsetMode.None;
            e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
            e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
            e.Graphics.SmoothingMode = SmoothingMode.HighSpeed;

            if(Image == null)
                return;
                
            DrawImage(e.Graphics);
            DrawBorder(e.Graphics);
        }
        
        private void DrawImage(Graphics canvas)
        {
            // We always draw to the whole container,
            // it is the picBox that is ratio stretched, see SetSize().
            canvas.DrawImage(Image, 0, 0, picBox.Width, picBox.Height);
        }
        private void DrawBorder(Graphics canvas)
        {
            Pen p = selected ? penSelected : penUnselected;
            canvas.DrawRectangle(p, 1, 1, picBox.Width-2, picBox.Height-2);
            canvas.DrawRectangle(Pens.White, 2, 2, picBox.Width-5, picBox.Height-5);
        }
        #endregion
    }
}