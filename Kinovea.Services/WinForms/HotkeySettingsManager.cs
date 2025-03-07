﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Kinovea.Services
{
    public static class HotkeySettingsManager
    {
        private static ToolStripMenuItem dummy = new ToolStripMenuItem();

        public static Dictionary<string, HotkeyCommand[]> Hotkeys
        {
            get { return hotkeys; }
        }

        private static Dictionary<string, HotkeyCommand[]> hotkeys;

        public static HotkeyCommand[] LoadHotkeys(string name)
        {
            if (hotkeys == null)
                hotkeys = CreateDefaultSettings();

            HotkeyCommand[] result = null;
            hotkeys.TryGetValue(name, out result);
            return result;
        }

        public static bool IsHandler(string category, Keys keys)
        {
            HotkeyCommand[] handledHotkeys = HotkeySettingsManager.LoadHotkeys(category);
            return handledHotkeys.Any(hk => hk != null && hk.KeyData == keys);
        }

        public static void Import(Dictionary<string, HotkeyCommand[]> imported)
        {
            if (hotkeys == null)
                hotkeys = CreateDefaultSettings();

            foreach (string category in imported.Keys)
                foreach (HotkeyCommand command in imported[category])
                    Update(category, command);
        }

        public static Keys GetMenuShortcut(string category, int commandCode)
        {
            Keys keys = Keys.None;

            if (hotkeys.ContainsKey(category))
            {
                HotkeyCommand[] result = null;
                hotkeys.TryGetValue(category, out result);

                foreach (HotkeyCommand c in hotkeys[category])
                {
                    if (c.CommandCode == commandCode)
                    {
                        // Some keys like 'Enter' can't be used as menu shortcuts.
                        try
                        {
                            dummy.ShortcutKeys = c.KeyData;
                            keys = c.KeyData;
                        }
                        catch
                        {
                            // This shortcut key cannot be used as a menu shortcut.
                            keys = Keys.None;
                        }

                        break;
                    }
                }
            }

            return keys; 
        }

        /// <summary>
        /// Returns false if there is a conflict on the hotkey for this category.
        /// </summary>
        public static bool IsUnique(string category, HotkeyCommand command)
        {
            if (!hotkeys.ContainsKey(category) || command.KeyData == Keys.None)
                return true;

            foreach (HotkeyCommand c in hotkeys[category])
            {
                if (c.CommandCode == command.CommandCode || c.KeyData != command.KeyData)
                    continue;

                return false;
            }

            return true;
        }

        public static void Update(string category, HotkeyCommand command)
        {
            // By convention only one hotkey is supported for each command.
            if (!hotkeys.ContainsKey(category))
                return;

            foreach (HotkeyCommand c in hotkeys[category])
            {
                // We test by the command name because the command code is subject to change
                // when we add commands between existing ones.
                if (c.Name == command.Name)
                {
                    c.KeyData = command.KeyData;
                    break;
                }
            }
        }

        public static void ResetToDefault(string category, HotkeyCommand command)
        {
            Dictionary<string, HotkeyCommand[]> defaultHotkeys = CreateDefaultSettings();

            if (!defaultHotkeys.ContainsKey(category))
                return;

            foreach (HotkeyCommand c in defaultHotkeys[category])
            {
                if (c.CommandCode == command.CommandCode)
                {
                    command.KeyData = c.KeyData;
                    break;
                }
            }
        }

        private static Dictionary<string, HotkeyCommand[]> CreateDefaultSettings()
        {
            Func<object, Keys, HotkeyCommand> hk = (en, k) => new HotkeyCommand((int)en, en.ToString(), k);

            Dictionary<string, HotkeyCommand[]> result = new Dictionary<string, HotkeyCommand[]>
            {
                { "FileExplorer", new HotkeyCommand[]{
                    hk(FileExplorerCommands.LaunchSelected, Keys.Enter),
                    //hk(FileExplorerCommands.RenameSelected, Keys.F2),
                    hk(FileExplorerCommands.DeleteSelected, Keys.Delete)
                    }
                },
                { "ThumbnailViewerContainer", new HotkeyCommand[]{
                    hk(ThumbnailViewerContainerCommands.DecreaseSize, Keys.Control | Keys.Subtract), 
                    hk(ThumbnailViewerContainerCommands.IncreaseSize, Keys.Control | Keys.Add)
                    }
                },
                { "ThumbnailViewerFiles", new HotkeyCommand[]{
                    hk(ThumbnailViewerFilesCommands.LaunchSelected, Keys.Enter), 
                    hk(ThumbnailViewerFilesCommands.RenameSelected, Keys.F2),
                    hk(ThumbnailViewerFilesCommands.DeleteSelected, Keys.Delete),
                    hk(ThumbnailViewerFilesCommands.Refresh, Keys.F5)
                    }
                },
                { "ThumbnailViewerCamera", new HotkeyCommand[]{
                    hk(ThumbnailViewerCameraCommands.LaunchSelected, Keys.Enter), 
                    hk(ThumbnailViewerCameraCommands.RenameSelected, Keys.F2),
                    hk(ThumbnailViewerCameraCommands.Refresh, Keys.F5)
                    }
                },
                { "DualPlayer", new HotkeyCommand[]{
                    hk(DualPlayerCommands.TogglePlay, Keys.Space),
                    hk(DualPlayerCommands.GotoPreviousImage, Keys.Left),
                    hk(DualPlayerCommands.GotoFirstImage, Keys.Home), 
                    hk(DualPlayerCommands.GotoPreviousKeyframe, Keys.Control | Keys.Left), 
                    hk(DualPlayerCommands.GotoNextImage, Keys.Right), 
                    hk(DualPlayerCommands.GotoLastImage, Keys.End), 
                    hk(DualPlayerCommands.GotoNextKeyframe, Keys.Control | Keys.Right), 
                    hk(DualPlayerCommands.GotoSyncPoint, Keys.F8), 
                    hk(DualPlayerCommands.ToggleSyncMerge, Keys.F9), 
                    hk(DualPlayerCommands.AddKeyframe, Keys.Insert)
                    }
                },
                { "PlayerScreen", new HotkeyCommand[]{
                    hk(PlayerScreenCommands.TogglePlay, Keys.Space), 
                    hk(PlayerScreenCommands.ResetViewport, Keys.Escape), 
                    hk(PlayerScreenCommands.GotoPreviousImage, Keys.Left), 
                    hk(PlayerScreenCommands.GotoPreviousImageForceLoop, Keys.Shift | Keys.Left), 
                    hk(PlayerScreenCommands.GotoFirstImage, Keys.Home), 
                    hk(PlayerScreenCommands.GotoPreviousKeyframe, Keys.Control | Keys.Left), 
                    hk(PlayerScreenCommands.BackwardRound10Percent, Keys.PageUp),
                    hk(PlayerScreenCommands.BackwardRound1Percent, Keys.Shift | Keys.PageUp),
                    hk(PlayerScreenCommands.GotoNextImage, Keys.Right), 
                    hk(PlayerScreenCommands.GotoLastImage, Keys.End), 
                    hk(PlayerScreenCommands.GotoNextKeyframe, Keys.Control | Keys.Right), 
                    hk(PlayerScreenCommands.ForwardRound10Percent, Keys.PageDown),
                    hk(PlayerScreenCommands.ForwardRound1Percent, Keys.Shift | Keys.PageDown),
                    hk(PlayerScreenCommands.GotoSyncPoint, Keys.F8), 
                    hk(PlayerScreenCommands.ToggleSyncMerge, Keys.F9), 
                    hk(PlayerScreenCommands.IncreaseZoom, Keys.Control | Keys.Add), 
                    hk(PlayerScreenCommands.DecreaseZoom, Keys.Control | Keys.Subtract), 
                    hk(PlayerScreenCommands.ResetZoom, Keys.Control | Keys.NumPad0), 
                    hk(PlayerScreenCommands.IncreaseSyncAlpha, Keys.Alt | Keys.Add), 
                    hk(PlayerScreenCommands.DecreaseSyncAlpha, Keys.Alt | Keys.Subtract), 
                    hk(PlayerScreenCommands.AddKeyframe, Keys.Insert), 
                    hk(PlayerScreenCommands.DeleteKeyframe, Keys.Control | Keys.Delete),
                    hk(PlayerScreenCommands.CutDrawing, Keys.Control | Keys.X),
                    hk(PlayerScreenCommands.CopyDrawing, Keys.Control | Keys.C),
                    hk(PlayerScreenCommands.PasteDrawing, Keys.Control | Keys.V),
                    hk(PlayerScreenCommands.DeleteDrawing, Keys.Delete), 
                    hk(PlayerScreenCommands.CopyImage, Keys.Control | Keys.Shift | Keys.C), 
                    hk(PlayerScreenCommands.ValidateDrawing, Keys.Enter),
                    hk(PlayerScreenCommands.IncreaseSpeed1, Keys.Control | Keys.Up), 
                    hk(PlayerScreenCommands.IncreaseSpeedRoundTo10, Keys.Shift | Keys.Up), 
                    hk(PlayerScreenCommands.IncreaseSpeedRoundTo25, Keys.Up), 
                    hk(PlayerScreenCommands.DecreaseSpeed1, Keys.Control | Keys.Down),
                    hk(PlayerScreenCommands.DecreaseSpeedRoundTo10, Keys.Shift | Keys.Down),
                    hk(PlayerScreenCommands.DecreaseSpeedRoundTo25, Keys.Down),
                    hk(PlayerScreenCommands.Close, Keys.Control | Keys.F4),
                    hk(PlayerScreenCommands.ToggleEvent1, Keys.D1),
                    hk(PlayerScreenCommands.ToggleEvent2, Keys.D2),
                    hk(PlayerScreenCommands.ToggleEvent3, Keys.D3),
                    hk(PlayerScreenCommands.ToggleEvent4, Keys.D4),
                    hk(PlayerScreenCommands.ToggleEvent5, Keys.D5),
                    hk(PlayerScreenCommands.ToggleEvent6, Keys.D6),
                    hk(PlayerScreenCommands.ToggleEvent7, Keys.D7),
                    hk(PlayerScreenCommands.ToggleEvent8, Keys.D8),
                    hk(PlayerScreenCommands.ToggleEvent9, Keys.D9),
                    hk(PlayerScreenCommands.ToggleEvent0, Keys.D0),
                    }
                },
                { "DualCapture", new HotkeyCommand[]{
                    hk(DualCaptureCommands.ToggleGrabbing, Keys.Space),
                    hk(DualCaptureCommands.ToggleRecording, Keys.Control | Keys.Return),
                    hk(DualCaptureCommands.TakeSnapshot, Keys.Shift | Keys.Return)
                    }
                },
                { "CaptureScreen", new HotkeyCommand[]{
                    hk(CaptureScreenCommands.ToggleGrabbing, Keys.Space), 
                    hk(CaptureScreenCommands.ToggleRecording, Keys.Control | Keys.Return), 
                    hk(CaptureScreenCommands.TakeSnapshot, Keys.Shift | Keys.Return), 
                    hk(CaptureScreenCommands.ResetViewport, Keys.Escape), 
                    hk(CaptureScreenCommands.OpenConfiguration, Keys.F12), 
                    //hk(CaptureScreenCommands.IncreaseZoom, Keys.Control | Keys.Add), 
                    //hk(CaptureScreenCommands.DecreaseZoom, Keys.Control | Keys.Subtract), 
                    //hk(CaptureScreenCommands.ResetZoom, Keys.Control | Keys.NumPad0), 
                    hk(CaptureScreenCommands.IncreaseDelay, Keys.Up),
                    hk(CaptureScreenCommands.DecreaseDelay, Keys.Down), 
                    hk(CaptureScreenCommands.Close, Keys.Control | Keys.F4)
                    }
                }
            };

            return result;
        }
    }
}
