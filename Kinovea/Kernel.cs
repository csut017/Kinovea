#region Licence
/*
Copyright � Joan Charmant 2008-2009.
jcharmant@gmail.com 
 
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Windows.Forms;

using Kinovea.FileBrowser;
using Kinovea.Root.Languages;
using Kinovea.ScreenManager;
using Kinovea.ScreenManager.Languages;
using Kinovea.Services;
using Kinovea.Updater;
using Kinovea.Video;
using Kinovea.Camera;
using System.Drawing;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Kinovea.Root
{
    public class RootKernel : IKernel 
    {
        #region Properties
        public ScreenManagerKernel ScreenManager
        {
            get { return screenManager; }
        }
        #endregion
        
        #region Members
        private KinoveaMainWindow mainWindow;
        private FileBrowserKernel fileBrowser;
        private UpdaterKernel updater;
        private ScreenManagerKernel screenManager;
        private Stopwatch stopwatch = new Stopwatch();
        
        #region Menus
        private ToolStripMenuItem mnuFile = new ToolStripMenuItem();
        private ToolStripMenuItem mnuOpenFile = new ToolStripMenuItem();
        private ToolStripMenuItem mnuOpenReplayWatcher = new ToolStripMenuItem();
        private ToolStripMenuItem mnuHistory = new ToolStripMenuItem();
        private ToolStripMenuItem mnuHistoryReset = new ToolStripMenuItem();
        private ToolStripMenuItem mnuQuit = new ToolStripMenuItem();
        private ToolStripMenuItem mnuEdit = new ToolStripMenuItem();
        private ToolStripMenuItem mnuUndo = new ToolStripMenuItem();
        private ToolStripMenuItem mnuRedo = new ToolStripMenuItem();
        private ToolStripMenuItem mnuView = new ToolStripMenuItem();
        public ToolStripMenuItem mnuToggleFileExplorer = new ToolStripMenuItem();
        public ToolStripMenuItem mnuFullScreen = new ToolStripMenuItem();
        private ToolStripMenuItem mnuImage = new ToolStripMenuItem();
        private ToolStripMenuItem mnuVideo = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTools = new ToolStripMenuItem();
        private ToolStripMenuItem mnuOptions = new ToolStripMenuItem();
        private ToolStripMenuItem mnuLanguages = new ToolStripMenuItem();
        private Dictionary<string, ToolStripMenuItem> languageMenus = new Dictionary<string, ToolStripMenuItem>();
        private ToolStripMenuItem mnuPreferences = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTimecode = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTimecodeClassic = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTimecodeFrames = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTimecodeMilliseconds = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTimecodeMicroseconds = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTimecodeTimeAndFrames = new ToolStripMenuItem();
        private ToolStripMenuItem mnuTimecodeNormalized = new ToolStripMenuItem();
        private ToolStripMenuItem mnuHelp = new ToolStripMenuItem();
        private ToolStripMenuItem mnuHelpContents = new ToolStripMenuItem();
        private ToolStripMenuItem mnuApplicationFolder = new ToolStripMenuItem();
        private ToolStripMenuItem mnuWebsite = new ToolStripMenuItem();
        private ToolStripMenuItem mnuAbout = new ToolStripMenuItem();
        #endregion
        
        private ToolStripButton toolOpenFile = new ToolStripButton();
        private ToolStripStatusLabel statusLabel = new ToolStripStatusLabel();

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public RootKernel()
        {
            log.Debug("Loading video readers.");
            List<Type> videoReaders = new List<Type>();
            videoReaders.Add(typeof(Video.Bitmap.VideoReaderBitmap));
            videoReaders.Add(typeof(Video.FFMpeg.VideoReaderFFMpeg));
            videoReaders.Add(typeof(Video.GIF.VideoReaderGIF));
            videoReaders.Add(typeof(Video.SVG.VideoReaderSVG));
            videoReaders.Add(typeof(Video.Synthetic.VideoReaderSynthetic));
            VideoTypeManager.LoadVideoReaders(videoReaders);

            log.Debug("Loading camera managers.");
            List<Type> cameraManagers = new List<Type>();
            cameraManagers.Add(typeof(Camera.Basler.CameraManagerBasler));
            cameraManagers.Add(typeof(Camera.DirectShow.CameraManagerDirectShow));
            cameraManagers.Add(typeof(Camera.FrameGenerator.CameraManagerFrameGenerator));
            cameraManagers.Add(typeof(Camera.HTTP.CameraManagerHTTP));
            cameraManagers.Add(typeof(Camera.IDS.CameraManagerIDS));
            CameraTypeManager.LoadCameraManagers(cameraManagers);

            log.Debug("Loading tools.");
            ToolManager.LoadTools();

            BuildSubTree();
            mainWindow = new KinoveaMainWindow(this);
            NotificationCenter.RecentFilesChanged += NotificationCenter_RecentFilesChanged;
            NotificationCenter.FullScreenToggle += NotificationCenter_FullscreenToggle;
            NotificationCenter.StatusUpdated += (s, e) => statusLabel.Text = e.Status;
            NotificationCenter.PreferenceTabAsked += NotificationCenter_PreferenceTabAsked; 

            log.Debug("Plug sub modules at UI extension points (Menus, Toolbars, Statusbar, Windows).");
            ExtendMenu(mainWindow.menuStrip);
            ExtendToolBar(mainWindow.toolStrip);
            ExtendStatusBar(mainWindow.statusStrip);
            ExtendUI();

            log.Debug("Register global services offered at Root level.");
            
            Services.FormsHelper.SetMainForm(mainWindow);
        }
        #endregion

        #region Prepare & Launch
        public void Prepare()
        {
            log.Debug("Setting current ui culture.");
            Thread.CurrentThread.CurrentUICulture = PreferencesManager.GeneralPreferences.GetSupportedCulture();
            RefreshUICulture();
            CheckLanguageMenu();
            CheckTimecodeMenu();
        }
        public void Launch()
        {
            screenManager.RecoverCrash();

            log.Debug("Calling Application.Run() to boot up the UI.");
            Application.Run(mainWindow);
        }
        #endregion
        
        #region IKernel Implementation
        public void BuildSubTree()
        {   
            stopwatch.Restart();
            log.Debug("Building the modules tree.");
            fileBrowser = new FileBrowserKernel();
            updater = new UpdaterKernel();
            screenManager = new ScreenManagerKernel();
            log.DebugFormat("Modules tree built in {0} ms.", stopwatch.ElapsedMilliseconds);
        }
        public void ExtendMenu(ToolStrip menu)
        {
            menu.AllowMerge = true;
            GetModuleMenus(menu);
            GetSubModulesMenus(menu);
        }
        public void ExtendToolBar(ToolStrip toolbar)
        {
            toolbar.AllowMerge = true;
            GetModuleToolBar(toolbar);
            GetSubModulesToolBars(toolbar);
            toolbar.Visible = true;
        }
        public void ExtendStatusBar(ToolStrip statusbar)
        {
            if(statusbar != null)
            {
                // This level
                statusLabel = new ToolStripStatusLabel();
                statusLabel.ForeColor = Color.White;
                statusbar.Items.AddRange(new ToolStripItem[] { statusLabel });
            }
        }
        public void ExtendUI()
        {
            fileBrowser.ExtendUI();
            updater.ExtendUI();
            screenManager.ExtendUI();

            mainWindow.PlugUI(fileBrowser.UI, screenManager.UI);
            mainWindow.SupervisorControl.buttonCloseExplo.BringToFront();
        }
        public void RefreshUICulture()
        {
            log.Debug("RefreshUICulture - Reload localized strings for the whole tree.");
            RefreshCultureMenu();
            CheckLanguageMenu();
            CheckTimecodeMenu();
            
            toolOpenFile.ToolTipText = ScreenManagerLang.mnuOpenVideo;
            
            fileBrowser.RefreshUICulture();
            updater.RefreshUICulture();
            screenManager.RefreshUICulture();
            
            log.Debug("RefreshUICulture - Whole tree culture reloaded.");
        }
        public void PreferencesUpdated()
        {
            RefreshUICulture();
            
            fileBrowser.PreferencesUpdated();
            updater.PreferencesUpdated();
            screenManager.PreferencesUpdated();
        }
        public bool CloseSubModules()
        {
            log.Debug("Root is closing. Call close on all sub modules.");
            bool cancel = screenManager.CloseSubModules();
            if(!cancel)
            {
                fileBrowser.CloseSubModules();
                updater.CloseSubModules();
            }

            return cancel;
        }
        #endregion

        #region Extension point helpers
        private void GetModuleMenus(ToolStrip menu)
        {
            // Affectation of .Text property happens in RefreshCultureMenu
            
            #region File
            mnuFile.MergeAction = MergeAction.Append;
            mnuOpenFile.Image = Properties.Resources.folder;
            mnuOpenFile.ShortcutKeys = Keys.Control | Keys.O;
            mnuOpenFile.Click += mnuOpenFileOnClick;

            mnuOpenReplayWatcher.Image = Properties.Resources.user_detective;
            mnuOpenReplayWatcher.Click += mnuOpenReplayWatcherOnClick;

            mnuHistory.Image = Properties.Resources.time;
            
            NotificationCenter.RaiseRecentFilesChanged(this);
            mnuHistoryReset.Image = Properties.Resources.bin_empty;
            mnuHistoryReset.Click += mnuHistoryResetOnClick;
            
            mnuQuit.Image = Properties.Resources.quit;
            mnuQuit.Click += new EventHandler(menuQuitOnClick);

            mnuFile.DropDownItems.AddRange(new ToolStripItem[] {
                mnuOpenFile,
                mnuOpenReplayWatcher,
                mnuHistory, 
                new ToolStripSeparator(),
                // -> Here will be plugged the other file menus (save, export)
                new ToolStripSeparator(), 
                mnuQuit });
            
            #endregion

            #region Edit
            mnuEdit.MergeAction = MergeAction.Append;
            
            mnuUndo.Tag = RootLang.ResourceManager;
            mnuUndo.Image = Properties.Resources.arrow_undo;
            mnuUndo.ShortcutKeys = Keys.Control | Keys.Z;
            mnuUndo.Enabled = false;

            mnuRedo.Tag = RootLang.ResourceManager;
            mnuRedo.Image = Properties.Resources.arrow_redo;
            mnuRedo.ShortcutKeys = Keys.Control | Keys.Y;
            mnuRedo.Enabled = false;

            HistoryMenuManager.RegisterMenus(mnuUndo, mnuRedo);
            
            mnuEdit.DropDownItems.AddRange(new ToolStripItem[] { mnuUndo, mnuRedo });
            #endregion

            #region View
            mnuToggleFileExplorer.Image = Properties.Resources.explorer;
            mnuToggleFileExplorer.Checked = true;
            mnuToggleFileExplorer.CheckState = System.Windows.Forms.CheckState.Checked;
            mnuToggleFileExplorer.ShortcutKeys = Keys.F4;
            mnuToggleFileExplorer.Click += new EventHandler(mnuToggleFileExplorerOnClick);
            mnuFullScreen.Image = Properties.Resources.fullscreen;
            mnuFullScreen.ShortcutKeys = Keys.F11;
            mnuFullScreen.Click += new EventHandler(mnuFullScreenOnClick);
            
            mnuView.DropDownItems.AddRange(new ToolStripItem[] { mnuToggleFileExplorer, mnuFullScreen, new ToolStripSeparator() });
            #endregion

            #region Options
            mnuLanguages.Image = Properties.Resources.international;
            foreach(KeyValuePair<string, string> lang in LanguageManager.Languages)
            {
                ToolStripMenuItem mnuLang = new ToolStripMenuItem(lang.Value);
                mnuLang.Tag = lang.Key;
                mnuLang.Click += mnuLanguage_OnClick;
                languageMenus.Add(lang.Key, mnuLang);
                mnuLanguages.DropDownItems.Add(mnuLang);
            }
            
            mnuPreferences.Image = Properties.Resources.wrench;
            mnuPreferences.Click += new EventHandler(mnuPreferencesOnClick);
            mnuTimecode.Image = Properties.Resources.time_edit;
            
            mnuTimecodeClassic.Click += new EventHandler(mnuTimecodeClassic_OnClick);
            mnuTimecodeFrames.Click += new EventHandler(mnuTimecodeFrames_OnClick);
            mnuTimecodeMilliseconds.Click += new EventHandler(mnuTimecodeMilliseconds_OnClick);
            mnuTimecodeMicroseconds.Click += new EventHandler(mnuTimecodeMicroseconds_OnClick);
            mnuTimecodeTimeAndFrames.Click += new EventHandler(mnuTimecodeTimeAndFrames_OnClick);
            mnuTimecodeNormalized.Click += new EventHandler(mnuTimecodeNormalized_OnClick);

            mnuTimecode.DropDownItems.AddRange(new ToolStripItem[] { mnuTimecodeClassic, mnuTimecodeFrames, mnuTimecodeMilliseconds, mnuTimecodeMicroseconds, mnuTimecodeTimeAndFrames});
            
            mnuOptions.DropDownItems.AddRange(new ToolStripItem[] { 
                mnuLanguages, 
                mnuTimecode, 
                new ToolStripSeparator(), 
                mnuPreferences});
            #endregion

            #region Help
            mnuHelpContents.Image = Properties.Resources.book_open;
            mnuHelpContents.ShortcutKeys = Keys.F1;
            mnuHelpContents.Click += new EventHandler(mnuHelpContents_OnClick);
            mnuApplicationFolder.Image = Properties.Resources.bug;
            mnuApplicationFolder.Click += (s, e) =>
            {
                FilesystemHelper.LocateFile(Path.Combine(Software.SettingsDirectory, "log.txt"));
            };

            mnuWebsite.Image = Properties.Resources.website;
            mnuWebsite.Click += (s,e) => Process.Start("https://www.kinovea.org");
            mnuAbout.Image = Properties.Resources.information;
            mnuAbout.Click += new EventHandler(mnuAbout_OnClick);

            mnuHelp.DropDownItems.AddRange(new ToolStripItem[] { 
                mnuHelpContents, 
                new ToolStripSeparator(), 
                mnuApplicationFolder, 
                new ToolStripSeparator(),
                mnuWebsite,
                mnuAbout });
            #endregion

            // Top level merge.
            MenuStrip thisMenuStrip = new MenuStrip();
            thisMenuStrip.Items.AddRange(new ToolStripItem[] { mnuFile, mnuEdit, mnuView, mnuImage, mnuVideo, mnuTools, mnuOptions, mnuHelp });
            thisMenuStrip.AllowMerge = true;

            ToolStripManager.Merge(thisMenuStrip, menu);
            
            // We need to affect the Text properties before merging with submenus.
            RefreshCultureMenu();
        }
        private void GetSubModulesMenus(ToolStrip menu)
        {
            fileBrowser.ExtendMenu(menu);
            updater.ExtendMenu(menu);
            screenManager.ExtendMenu(menu);
        }
        private void GetModuleToolBar(ToolStrip toolbar)
        {
            // Open.
            toolOpenFile.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolOpenFile.Image = Properties.Resources.folder;
            toolOpenFile.ToolTipText = ScreenManagerLang.mnuOpenVideo;
            toolOpenFile.Click += new EventHandler(mnuOpenFileOnClick);
            
            toolbar.Items.Add(toolOpenFile);
        }
        private void GetSubModulesToolBars(ToolStrip toolbar)
        {
            fileBrowser.ExtendToolBar(toolbar);
            updater.ExtendToolBar(toolbar);
            screenManager.ExtendToolBar(toolbar);
        }
        private void RefreshCultureMenu()
        {
            mnuFile.Text = RootLang.mnuFile;
            mnuOpenFile.Text = ScreenManagerLang.mnuOpenVideo;
            mnuOpenReplayWatcher.Text = ScreenManagerLang.mnuOpenReplayWatcher;
            mnuHistory.Text = RootLang.mnuHistory;
            mnuHistoryReset.Text = RootLang.mnuHistoryReset;
            mnuQuit.Text = RootLang.Generic_Quit;
            
            mnuEdit.Text = RootLang.mnuEdit;
            mnuUndo.Text = RootLang.mnuUndo;
            mnuRedo.Text = RootLang.mnuRedo;
            
            mnuView.Text = RootLang.mnuScreens;
            mnuToggleFileExplorer.Text = RootLang.mnuToggleFileExplorer;
            mnuFullScreen.Text = RootLang.mnuFullScreen;
            
            mnuImage.Text = RootLang.mnuImage;
            mnuVideo.Text = RootLang.mnuVideo;
            mnuTools.Text = RootLang.mnuTools;
            
            mnuOptions.Text = RootLang.mnuOptions;
            mnuLanguages.Text = RootLang.mnuLanguages;
            mnuPreferences.Text = RootLang.mnuPreferences;
            mnuTimecode.Text = RootLang.mnuTimeFormat;

            mnuTimecodeClassic.Text = "[h:][mm:]ss.xx[x]";
            mnuTimecodeClassic.Image = Properties.Resources.timecode;
            mnuTimecodeFrames.Text = RootLang.TimeCodeFormat_Frames;
            mnuTimecodeFrames.Image = Properties.Resources.framenumber;
            mnuTimecodeMilliseconds.Text = RootLang.TimeCodeFormat_Milliseconds;
            mnuTimecodeMilliseconds.Image = Properties.Resources.milliseconds;
            mnuTimecodeMicroseconds.Text = RootLang.TimeCodeFormat_Microseconds;
            mnuTimecodeMicroseconds.Image = Properties.Resources.microseconds;
            mnuTimecodeTimeAndFrames.Text = mnuTimecodeClassic.Text + " + " + RootLang.TimeCodeFormat_Frames;

            mnuHelp.Text = RootLang.mnuHelp;
            mnuHelpContents.Text = RootLang.mnuHelpContents;
            mnuApplicationFolder.Text = RootLang.mnuApplicationFolder;
            mnuWebsite.Text = "www.kinovea.org";
            mnuAbout.Text = RootLang.mnuAbout;
            mnuHelp.Text = RootLang.mnuHelp;
        }
        #endregion

        #region Menus Event Handlers

        #region File
        private void mnuOpenFileOnClick(object sender, EventArgs e)
        {
            NotificationCenter.RaiseStopPlayback(this);
            
            string filename = FilePicker.OpenVideo();
            if (!string.IsNullOrEmpty(filename))
                OpenFileFromPath(filename);
        }
        private void mnuOpenReplayWatcherOnClick(object sender, EventArgs e)
        {
            NotificationCenter.RaiseStopPlayback(this);

            string path = FilePicker.OpenReplayWatcher();
            if (path == null || !Directory.Exists(Path.GetDirectoryName(path)))
                return;

            ScreenDescriptionPlayback screenDescription = new ScreenDescriptionPlayback();
            screenDescription.FullPath = path;
            screenDescription.IsReplayWatcher = true;
            screenDescription.Stretch = true;
            screenDescription.Autoplay = true;
            screenDescription.SpeedPercentage = PreferencesManager.PlayerPreferences.DefaultReplaySpeed;
            LoaderVideo.LoadVideoInScreen(screenManager, path, screenDescription);

            screenManager.OrganizeScreens();
        }

        private void mnuHistoryResetOnClick(object sender, EventArgs e)
        {
            PreferencesManager.FileExplorerPreferences.ResetRecentFiles();
            PreferencesManager.Save();
        }
        private void menuQuitOnClick(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion

        #region View
        private void mnuToggleFileExplorerOnClick(object sender, EventArgs e)
        {
            if (mainWindow.SupervisorControl.IsExplorerCollapsed)
            {
                mainWindow.SupervisorControl.ExpandExplorer(true);
            }
            else
            {
                mainWindow.SupervisorControl.CollapseExplorer();
            }
        }
        private void mnuFullScreenOnClick(object sender, EventArgs e)
        {
            FullscreenToggle();
        }
        #endregion

        #region Options
        private void mnuLanguage_OnClick(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = sender as ToolStripMenuItem;
            if(menu != null && menu.Tag is string)
                SwitchCulture((string)menu.Tag);
        }
        private void SwitchCulture(string name)
        {
            try
            {
                CultureInfo oldCulture = Thread.CurrentThread.CurrentUICulture;
                CultureInfo newCulture = new CultureInfo(LanguageManager.FixCultureName(name));

                log.Debug(String.Format("Changing culture from [{0}] to [{1}].", oldCulture.Name, newCulture.Name));

                PreferencesManager.GeneralPreferences.SetCulture(newCulture.Name);
                Thread.CurrentThread.CurrentUICulture = PreferencesManager.GeneralPreferences.GetSupportedCulture();
                PreferencesManager.Save();

                RefreshUICulture();
            }
            catch (ArgumentException)
            {
                log.ErrorFormat("Could not switch from culture {0} to {1}.", Thread.CurrentThread.CurrentUICulture.Name, name);
            }
        }
        private void CheckLanguageMenu()
        {
            foreach(ToolStripMenuItem mnuLang in languageMenus.Values)
                mnuLang.Checked = false;

            string cultureName = LanguageManager.GetCurrentCultureName();
            
            try
            {
                languageMenus[cultureName].Checked = true;    
            }
            catch(KeyNotFoundException)
            {
                languageMenus["en"].Checked = true;            
            }
        }
        private void mnuPreferencesOnClick(object sender, EventArgs e)
        {
            FormPreferences2 fp = new FormPreferences2();
            fp.ShowDialog();
            fp.Dispose();
            
            // Refresh Preferences
            log.Debug("Setting current ui culture.");
            Thread.CurrentThread.CurrentUICulture = PreferencesManager.GeneralPreferences.GetSupportedCulture();
            PreferencesUpdated();
        }


        private void CheckTimecodeMenu()
        {
            mnuTimecodeClassic.Checked = false;
            mnuTimecodeFrames.Checked = false;
            mnuTimecodeMilliseconds.Checked = false;
            mnuTimecodeMicroseconds.Checked = false;
            mnuTimecodeTimeAndFrames.Checked = false;
            mnuTimecodeNormalized.Checked = false;
            
            TimecodeFormat tf = PreferencesManager.PlayerPreferences.TimecodeFormat;
            
            switch (tf)
            {
                case TimecodeFormat.ClassicTime:
                    mnuTimecodeClassic.Checked = true;
                    break;
                case TimecodeFormat.Frames:
                    mnuTimecodeFrames.Checked = true;
                    break;
                case TimecodeFormat.Milliseconds:
                    mnuTimecodeMilliseconds.Checked = true;
                    break;
                case TimecodeFormat.Microseconds:
                    mnuTimecodeMicroseconds.Checked = true;
                    break;
                case TimecodeFormat.TimeAndFrames:
                    mnuTimecodeTimeAndFrames.Checked = true;
                    break;
                case TimecodeFormat.Normalized:
                    mnuTimecodeNormalized.Checked = true;
                    break; 
                default:
                    break;
            }
        }
        private void mnuTimecodeClassic_OnClick(object sender, EventArgs e)
        {
            SwitchTimecode(TimecodeFormat.ClassicTime);
        }
        private void mnuTimecodeFrames_OnClick(object sender, EventArgs e)
        {
            SwitchTimecode(TimecodeFormat.Frames);
        }
        private void mnuTimecodeMilliseconds_OnClick(object sender, EventArgs e)
        {
            SwitchTimecode(TimecodeFormat.Milliseconds);
        }
        private void mnuTimecodeMicroseconds_OnClick(object sender, EventArgs e)
        {
            SwitchTimecode(TimecodeFormat.Microseconds);
        }
        private void mnuTimecodeTimeAndFrames_OnClick(object sender, EventArgs e)
        {
            SwitchTimecode(TimecodeFormat.TimeAndFrames);
        }
        private void mnuTimecodeNormalized_OnClick(object sender, EventArgs e)
        {
            SwitchTimecode(TimecodeFormat.Normalized);
        }
        private void SwitchTimecode(TimecodeFormat _timecode)
        {
            PreferencesManager.PlayerPreferences.TimecodeFormat = _timecode;
            RefreshUICulture();
            PreferencesManager.Save();
        }
        #endregion

        #region Help
        private void mnuHelpContents_OnClick(object sender, EventArgs e)
        {
            // Launch Help file from current UI language.
            string resourceUri = GetLocalizedHelpResource(true);
            if(resourceUri != null && resourceUri.Length > 0 && File.Exists(resourceUri))
            {
                Help.ShowHelp(mainWindow, resourceUri);
            }
            else
            {
                log.Error(String.Format("Cannot find the manual. ({0}).", resourceUri));
            }
        }
        private void mnuAbout_OnClick(object sender, EventArgs e)
        {
            FormAbout fa = new FormAbout();
            fa.ShowDialog();
            fa.Dispose();
        }
        #endregion

        #endregion        
        
        private void NotificationCenter_RecentFilesChanged(object sender, EventArgs e)
        {
            List<string> recentFiles = PreferencesManager.FileExplorerPreferences.RecentFiles;
            int maxRecentFiles = PreferencesManager.FileExplorerPreferences.MaxRecentFiles;
            
            mnuHistory.DropDownItems.Clear();
            
            if(recentFiles == null)
            {
                mnuHistory.Enabled = false;
                return;
            }
            
            int added = 0;
            int current = 0;
            while(added < maxRecentFiles && current < recentFiles.Count)
            {
                string file = recentFiles[current++];

                if(string.IsNullOrEmpty(file) || !File.Exists(file))
                    continue;
                
                ToolStripMenuItem menu = new ToolStripMenuItem();
                menu.Image = Properties.Resources.film_small;
                menu.Text = Path.GetFileName(file);
                menu.Click += (s, evt) => OpenFileFromPath(file);
                
                mnuHistory.DropDownItems.Add(menu);
                added++;
            }
            
            if(added > 0)
            {
                mnuHistory.DropDownItems.Add(new ToolStripSeparator());
                mnuHistory.DropDownItems.Add(mnuHistoryReset);
            }
            
            mnuHistory.Enabled = added > 0;
        }

        private void NotificationCenter_FullscreenToggle(object sender, EventArgs e)
        {
            FullscreenToggle();
        }

        private void NotificationCenter_PreferenceTabAsked(object sender, PreferenceTabEventArgs e)
        {
            FormPreferences2 fp = new FormPreferences2(e.Tab);
            fp.ShowDialog();
            fp.Dispose();

            Thread.CurrentThread.CurrentUICulture = PreferencesManager.GeneralPreferences.GetSupportedCulture();
            PreferencesUpdated();
        }
       
        #region Lower Level Methods
        private void OpenFileFromPath(string filePath)
        {
            if (File.Exists(filePath))
            {
                LoaderVideo.LoadVideoInScreen(screenManager, filePath, -1);
                screenManager.OrganizeScreens();
            }
            else
            {
                MessageBox.Show(ScreenManagerLang.LoadMovie_FileNotOpened, ScreenManagerLang.LoadMovie_Error, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }
        private string GetLocalizedHelpResource(bool manual)
        {
            // Find the local file path of a help resource (manual or help video) according to what is saved in the help index.
            
            string resourceUri = "";
            
            // Load the help file system.
            HelpIndex hiLocal = new HelpIndex(Software.LocalHelpIndex);

            if(!hiLocal.LoadSuccess)
            {
                log.Error("Cannot find the xml help index.");
                return "";
            }
                
            // Loop into the file to find the required resource in the matching locale, or fallback to english.
            string englishUri = "";
            bool localeFound = false;
            bool englishFound = false;
            int i = 0;

            string cultureName = LanguageManager.GetCurrentCultureName();
                            
            // Look for a matching locale, or English.
            int totalResource = manual ? hiLocal.UserGuides.Count : hiLocal.HelpVideos.Count;
            while (!localeFound && i < totalResource)
            {
                HelpItem hi = manual ? hiLocal.UserGuides[i] : hiLocal.HelpVideos[i];

                if (hi.Language == cultureName)
                {
                    localeFound = true;
                    resourceUri = hi.FileLocation;
                    break;
                }

                if (hi.Language == "en")
                {
                    englishFound = true;
                    englishUri = hi.FileLocation;
                }

                i++;
            }

            if (!localeFound && englishFound)
                resourceUri = englishUri;
            
            return resourceUri;
        }
        private void FullscreenToggle()
        {
            mainWindow.ToggleFullScreen();

            if (mainWindow.FullScreen)
                mainWindow.SupervisorControl.CollapseExplorer();
            else
                mainWindow.SupervisorControl.ExpandExplorer(true);

            // Propagates the call to screens.
            screenManager.FullScreen(mainWindow.FullScreen);
        }
        #endregion
    }
}
