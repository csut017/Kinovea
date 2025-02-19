#region Licence
/*
Copyright � Joan Charmant 2008.
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

using Kinovea.ScreenManager.Languages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows.Forms;

using Kinovea.Services;
using Kinovea.Video;

namespace Kinovea.ScreenManager
{
    public class PlayerScreen : AbstractScreen
    {
        #region Events
        public event EventHandler OpenVideoAsked;
        public event EventHandler OpenReplayWatcherAsked;
        public event EventHandler OpenAnnotationsAsked;
        public event EventHandler SpeedChanged;
        public event EventHandler HighSpeedFactorChanged;
        public event EventHandler TimeOriginChanged;
        public event EventHandler PlayStarted;
        public event EventHandler PauseAsked;
        public event EventHandler<EventArgs<bool>> SelectionChanged;
        public event EventHandler<EventArgs<Bitmap>> ImageChanged;
        public event EventHandler ResetAsked;
        #endregion

        #region Properties
        public override bool Full
        {
            get { return frameServer.Loaded; }
        }
        public override UserControl UI
        {
            get { return view; }	
        }
        public override Guid Id
        {
            get { return id; }
            set { id = value; }
        }
        public override string FileName
        {
            get 
            { 
                return frameServer.Loaded ? Path.GetFileName(frameServer.VideoReader.FilePath) :
                                              ScreenManagerLang.statusEmptyScreen;
            }
        }
        public override string Status
        {
            get	{return FileName;}
        }
        public override string FilePath
        {
            get { return frameServer.VideoReader.FilePath; }
        }
        public override bool CapabilityDrawings
        {
            get { return true;}
        }
        public override ImageAspectRatio AspectRatio
        {
            get { return frameServer.VideoReader.Options.ImageAspectRatio; }
            set
            {
                bool uncached = frameServer.VideoReader.ChangeAspectRatio(value);
                
                if (uncached && frameServer.VideoReader.DecodingMode == VideoDecodingMode.Caching)
                    view.UpdateWorkingZone(true);
                    
                view.ReferenceImageSizeChanged();
            }
        }
        public override ImageRotation ImageRotation
        {
            get { return frameServer.VideoReader.Options.ImageRotation; }
            set
            {
                bool uncached = frameServer.VideoReader.ChangeImageRotation(value);

                if (uncached && frameServer.VideoReader.DecodingMode == VideoDecodingMode.Caching)
                    view.UpdateWorkingZone(true);

                view.ReferenceImageSizeChanged();
            }
        }
        public override Demosaicing Demosaicing
        {
            get
            {
                return frameServer.VideoReader.Options.Demosaicing;
            }

            set
            {
                if (!frameServer.VideoReader.CanChangeDemosaicing)
                    return;

                bool uncached = frameServer.VideoReader.ChangeDemosaicing(value);

                if (uncached && frameServer.VideoReader.DecodingMode == VideoDecodingMode.Caching)
                    view.UpdateWorkingZone(true);

                RefreshImage();
            }
        }
        public override bool Mirrored
        {
            get { return frameServer.Metadata.Mirrored; }
            set
            {
                frameServer.Metadata.Mirrored = value;
                RefreshImage();
            }
        }

        public FrameServerPlayer FrameServer
        {
            get { return frameServer; }
            set { frameServer = value; }
        }        
        public bool IsPlaying
        {
            get
            {
                if (!frameServer.Loaded)
                    return false;
                else
                    return view.IsCurrentlyPlaying;
            }
        }
        public bool IsWaitingForIdle
        {
            get { return view.IsWaitingForIdle; }
        }
        public bool IsSingleFrame
        {
            get
            {
                if (!frameServer.Loaded)
                    return false;
                else
                    return frameServer.VideoReader.IsSingleFrame;
            }	
        }
        public bool IsCaching
        {
            get
            {
                if (!frameServer.Loaded)
                    return false;
                else
                    return frameServer.VideoReader.DecodingMode == VideoDecodingMode.Caching;
            }
        }

        /// <summary>
        /// Returns the current frame time relative to selection start.
        /// The value is a physical time in microseconds, taking high speed factor into account.
        /// </summary>
        public long LocalTime
        {
            get { return view.LocalTime; }
        }

        /// <summary>
        /// Returns the last valid time relative to selection start.
        /// The value is a physical time in microseconds, taking high speed factor into account.
        /// </summary>
        public long LocalLastTime
        {
            get
            {
                return view.LocalLastTime;
            }
        }

        /// <summary>
        /// Returns the average time of one frame.
        /// The value is a physical time in microseconds, taking high speed factor into account.
        /// </summary>
        public long LocalFrameTime
        {
            get
            {
                return view.LocalFrameTime;
            }
        }

        /// <summary>
        /// Returns the time origin relative to selection start.
        /// The value is a physical time in microseconds, taking high speed factor into account.
        /// </summary>
        public long LocalTimeOriginPhysical
        {
            get
            {
                return view.LocalTimeOriginPhysical;
            }

            set
            {
                long absoluteTimestamp = RelativeRealTimeToAbsoluteTimestamp(value);
                frameServer.Metadata.TimeOrigin = absoluteTimestamp;
                view.TimeOriginUpdatedFromSync();
            }
        }
         
        public double FrameInterval
        {
            get 
            { 
                // Returns the playback interval between frames in Milliseconds, taking slow motion slider into account.
                if (frameServer.Loaded && frameServer.VideoReader.Info.FrameIntervalMilliseconds > 0)
                    return view.FrameInterval;
                else
                    return 40;
            }
        }
        public double RealtimePercentage
        {
            get { return view.RealtimePercentage; }
            set { view.RealtimePercentage = value;}
        }
        public bool Synched
        {
            get { return synched; }
            set 
            { 
                view.Synched = value;
                synched = value;
            }
        }
        
        
        
        public long Position
        {
            // Used to feed SyncPosition. 
            get 
            {
                if (frameServer.VideoReader.Current == null)
                    return 0;

                return frameServer.VideoReader.Current.Timestamp - frameServer.VideoReader.Info.FirstTimeStamp; 
            }
        }
        public bool SyncMerge
        {
            set 
            {
                view.SyncMerge = value;
                RefreshImage();
            }
        }
        public bool DualSaveInProgress
        {
            set { view.DualSaveInProgress = value; }
        }
        
        // Pseudo Filters (Impacts rendering)
        public bool Deinterlaced
        {
            get { return frameServer.VideoReader.Options.Deinterlace; }
            set
            {
                bool uncached = frameServer.VideoReader.ChangeDeinterlace(value);
                
                if (uncached && frameServer.VideoReader.DecodingMode == VideoDecodingMode.Caching)
                    view.UpdateWorkingZone(true);
                
                RefreshImage();
            }
        }
        public bool InteractiveFiltering {
            get {return view.InteractiveFiltering;}
        }
        public HistoryStack HistoryStack
        {
            get { return historyStack; }
        }
        #endregion

        #region members
        private Guid id = Guid.NewGuid();
        public PlayerScreenUserInterface view;
        private DrawingToolbarPresenter drawingToolbarPresenter = new DrawingToolbarPresenter();
        private HistoryStack historyStack; 
        private FrameServerPlayer frameServer;
        private bool synched;
        private int index;
        private ReplayWatcher replayWatcher;
        
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructor
        public PlayerScreen()
        {
            log.Debug("Constructing a PlayerScreen.");
            historyStack = new HistoryStack();
            frameServer = new FrameServerPlayer(historyStack);
            replayWatcher = new ReplayWatcher(this);
            view = new PlayerScreenUserInterface(frameServer, drawingToolbarPresenter);
            
            BindCommands();
        }
        #endregion

        private void BindCommands()
        {
            // Provides implementation for behaviors triggered from the view, either as commands or as event handlers.
            // Fixme: those using FrameServer.Metadata work only because the Metadata object is never replaced during the PlayerScreen life.

            view.OpenVideoAsked += (s, e) => OpenVideoAsked?.Invoke(this, e);
            view.OpenReplayWatcherAsked += (s, e) => OpenReplayWatcherAsked?.Invoke(this, e);
            view.OpenAnnotationsAsked += (s, e) => OpenAnnotationsAsked?.Invoke(this, e);
            view.CloseAsked += View_CloseAsked;
            view.SetAsActiveScreen += View_SetAsActiveScreen;
            view.SpeedChanged += View_SpeedChanged;
            view.TimeOriginChanged += View_TimeOriginChanged;
            view.KVAImported += View_KVAImported;
            view.PlayStarted += View_PlayStarted;
            view.PauseAsked += View_PauseAsked;
            view.SelectionChanged += View_SelectionChanged;
            view.ImageChanged += View_ImageChanged;
            view.ResetAsked += View_ResetAsked;

            // Requests for metadata modification coming from the view, these should push a memento on the history stack.
            view.KeyframeAdding += View_KeyframeAdding;
            view.KeyframeDeleting += View_KeyframeDeleting;
            view.DrawingAdding += View_DrawingAdding;
            view.DrawingDeleting += View_DrawingDeleting;
            view.MultiDrawingItemAdding += View_MultiDrawingItemAdding;
            view.MultiDrawingItemDeleting += View_MultiDrawingItemDeleting;
            view.DualCommandReceived += (s, e) => OnDualCommandReceived(e);
            view.DataAnalysisAsked += (s, e) => ShowDataAnalysis(e.Value);
            view.HistoryMementoGenerated += (s, e) => historyStack.PushNewCommand(e.Value);


            // Just for the magnifier. Remove as soon as possible when the adding of the magnifier is handled in Metadata.
            view.TrackableDrawingAdded += (s, e) => AddTrackableDrawing(e.TrackableDrawing);
            
            // Commands
            view.ToggleTrackingCommand = new ToggleCommand(ToggleTracking, IsTracking);
            view.TrackDrawingsCommand = new RelayCommand<VideoFrame>(TrackDrawings);
            
            frameServer.Metadata.AddTrackableDrawingCommand = new RelayCommand<ITrackable>(AddTrackableDrawing);
            frameServer.Metadata.CameraCalibrationAsked += (s, e) => ShowCameraCalibration();
        }

        #region General events handlers
        private void View_CloseAsked(object sender, EventArgs e)
        {
            OnCloseAsked(EventArgs.Empty);
        }
        
        public void View_SetAsActiveScreen(object sender, EventArgs e)
        {
            OnActivated(EventArgs.Empty);
        }

        public void View_SpeedChanged(object sender, EventArgs e)
        {
            if (SpeedChanged != null)
                SpeedChanged(this, EventArgs.Empty);
        }

        public void View_TimeOriginChanged(object sender, EventArgs e)
        {
            if (TimeOriginChanged != null)
                TimeOriginChanged(this, EventArgs.Empty);
        }

        public void View_KVAImported(object sender, EventArgs e)
        {
            if (HighSpeedFactorChanged != null)
                HighSpeedFactorChanged(this, EventArgs.Empty);
        }

        public void View_PlayStarted(object sender, EventArgs e)
        {
            if (PlayStarted != null)
                PlayStarted(this, EventArgs.Empty);
        }

        public void View_PauseAsked(object sender, EventArgs e)
        {
            if (PauseAsked != null)
                PauseAsked(this, EventArgs.Empty);
        }
        
        public void View_SelectionChanged(object sender, EventArgs<bool> e)
        {
            if (SelectionChanged != null)
                SelectionChanged(this, e);
        }
        
        public void View_ImageChanged(object sender, EventArgs<Bitmap> e)
        {
            if (ImageChanged != null)
                ImageChanged(this, e);
        }
        
        public void View_ResetAsked(object sender, EventArgs e)
        {
            if (ResetAsked != null)
                ResetAsked(this, e);
        }
        #endregion

        #region Requests for Metadata modification coming from the view
        private void View_KeyframeAdding(object sender, TimeEventArgs e)
        {
            if (frameServer.CurrentImage == null)
                return;

            long time = e.Time;
            string timecode = frameServer.TimeStampsToTimecode(time, TimeType.UserOrigin, PreferencesManager.PlayerPreferences.TimecodeFormat, true);
            Keyframe keyframe = new Keyframe(time, timecode, frameServer.Metadata);

            HistoryMementoAddKeyframe memento = new HistoryMementoAddKeyframe(frameServer.Metadata, keyframe.Id);
            frameServer.Metadata.AddKeyframe(keyframe);
            historyStack.PushNewCommand(memento);
        }

        private void View_KeyframeDeleting(object sender, KeyframeEventArgs e)
        {
            HistoryMemento memento = new HistoryMementoDeleteKeyframe(frameServer.Metadata, e.KeyframeId);
            frameServer.Metadata.DeleteKeyframe(e.KeyframeId);
            historyStack.PushNewCommand(memento);
        }

        private void View_DrawingAdding(object sender, DrawingEventArgs e)
        {
            AddDrawingWithMemento(e.ManagerId, e.Drawing);
        }
        
        private void View_DrawingDeleting(object sender, DrawingEventArgs e)
        {
            HistoryMemento memento = new HistoryMementoDeleteDrawing(frameServer.Metadata, e.ManagerId, e.Drawing.Id, e.Drawing.Name);
            frameServer.Metadata.DeleteDrawing(e.ManagerId, e.Drawing.Id);
            historyStack.PushNewCommand(memento);
        }

        private void View_MultiDrawingItemAdding(object sender, MultiDrawingItemEventArgs e)
        {
            HistoryMemento memento = new HistoryMementoAddMultiDrawingItem(frameServer.Metadata, e.Manager, e.Item.Id);
            frameServer.Metadata.AddMultidrawingItem(e.Manager, e.Item);
            historyStack.PushNewCommand(memento);
        }
        
        private void View_MultiDrawingItemDeleting(object sender, MultiDrawingItemEventArgs e)
        {
            HistoryMemento memento = new HistoryMementoDeleteMultiDrawingItem(frameServer.Metadata, e.Manager, e.Item.Id, SerializationFilter.All);
            frameServer.Metadata.DeleteMultiDrawingItem(e.Manager, e.Item.Id);
            historyStack.PushNewCommand(memento);
        }
        #endregion

        #region AbstractScreen Implementation
        public override void DisplayAsActiveScreen(bool _bActive)
        {
            view.DisplayAsActiveScreen(_bActive);
        }
        public override void BeforeClose()
        {
            // Called by the ScreenManager when this screen is about to be closed.
            // Note: We shouldn't call ResetToEmptyState here because we will want
            // the close screen routine to detect if there is something left in the 
            // metadata and alerts the user.
            if(frameServer.Loaded)
                view.StopPlaying();
        }
        public override void AfterClose()
        {
            frameServer.Metadata.Close();
            replayWatcher.Close();
            
            if(!frameServer.Loaded)
                return;
            
            frameServer.VideoReader.Close();
            view.ResetToEmptyState();
            view.AfterClose();

            drawingToolbarPresenter.Dispose();
        }
        public override void RefreshUICulture()
        {
            view.RefreshUICulture();
            drawingToolbarPresenter.RefreshUICulture();
        }
        public override void PreferencesUpdated()
        {
        }
        public override void RefreshImage()
        {
            view.RefreshImage();
        }
        public override void AddImageDrawing(string filename, bool isSvg)
        {
            if (!File.Exists(filename))
                return;

            view.BeforeAddImageDrawing();
            
            if (frameServer.Metadata.HitKeyframe == null)
                return;

            AbstractDrawing drawing = null;
            if (isSvg)
                drawing = new DrawingSVG(frameServer.VideoReader.Current.Timestamp, frameServer.VideoReader.Info.AverageTimeStampsPerFrame, filename);
            else
                drawing = new DrawingBitmap(frameServer.VideoReader.Current.Timestamp, frameServer.VideoReader.Info.AverageTimeStampsPerFrame, filename);

            if (drawing != null)
                AddDrawingWithMemento(frameServer.Metadata.HitKeyframe.Id, drawing);
        }
        public override void AddImageDrawing(Bitmap bmp)
        {
            view.BeforeAddImageDrawing();
            AbstractDrawing drawing = new DrawingBitmap(frameServer.VideoReader.Current.Timestamp, frameServer.VideoReader.Info.AverageTimeStampsPerFrame, bmp);
            frameServer.Metadata.AddDrawing(frameServer.Metadata.HitKeyframe.Id, drawing);
        }
        public override void FullScreen(bool _bFullScreen)
        {
            view.FullScreen(_bFullScreen);
        }

        public override void Identify(int index)
        {
            this.index = index;
        }
        

        public override void ExecuteScreenCommand(int cmd)
        {
            view.ExecuteScreenCommand(cmd);
        }

        public override void LoadKVA(string path)
        {
            MetadataSerializer s = new MetadataSerializer();
            s.Load(frameServer.Metadata, path, true);
        }
        #endregion
        
        #region Other public methods called from the ScreenManager
        public void EnsurePlaying()
        {
            view.EnsurePlaying();
        }
        
        public void StopPlaying()
        {
            view.StopPlaying();
        }
        
        public void GotoNextFrame(bool allowUIUpdate)
        {
            view.ForceCurrentFrame(-1, allowUIUpdate);
        }
        
        public void GotoTime(long microseconds, bool allowUIUpdate)
        {
            long timestamp = RelativeRealTimeToAbsoluteTimestamp(microseconds);
            view.ForcePosition(timestamp, allowUIUpdate);
        }
        
        public void GotoPrevKeyframe()
        {
            view.GotoPreviousKeyframe();
        }
        
        public void GotoNextKeyframe()
        {
            view.GotoNextKeyframe();
        }
        
        public void AddKeyframe()
        {
            view.AddKeyframe();
        }
        
        public void ResetSelectionImages(MemoPlayerScreen _memo)
        {
            view.ResetSelectionImages(_memo);
        }
        
        public MemoPlayerScreen GetMemo()
        {
            return view.GetMemo();
        }
        
        public void SetInteractiveEffect(InteractiveEffect _effect)
        {
            view.SetInteractiveEffect(_effect);
        }
        
        public void DeactivateInteractiveEffect()
        {
            view.DeactivateInteractiveEffect();
        }
        
        public void SetSyncMergeImage(Bitmap _SyncMergeImage, bool _bUpdateUI)
        {
            view.SetSyncMergeImage(_SyncMergeImage, _bUpdateUI);
        }
        
        public void Save()
        {
            view.Save();
        }
        
        public void ConfigureTimebase()
        {
            if (!frameServer.Loaded)
                return;

            double captureInterval = frameServer.Metadata.UserInterval / frameServer.Metadata.HighSpeedFactor;
            formConfigureSpeed fcs = new formConfigureSpeed(frameServer.VideoReader.Info.FrameIntervalMilliseconds, frameServer.Metadata.UserInterval, captureInterval);
            fcs.StartPosition = FormStartPosition.CenterScreen;

            if (fcs.ShowDialog() != DialogResult.OK)
            {
                fcs.Dispose();
                return;
            }

            frameServer.Metadata.UserInterval = fcs.UserInterval;
            frameServer.Metadata.HighSpeedFactor = fcs.UserInterval / fcs.CaptureInterval;
            fcs.Dispose();

            log.DebugFormat("Time configuration. File interval:{0:0.###}ms, User interval:{1:0.###}ms, Capture interval:{2:0.###}ms.",
                frameServer.VideoReader.Info.FrameIntervalMilliseconds, frameServer.Metadata.UserInterval, fcs.CaptureInterval);

            if (HighSpeedFactorChanged != null)
                HighSpeedFactorChanged(this, EventArgs.Empty);

            frameServer.Metadata.CalibrationHelper.CaptureFramesPerSecond = 1000 * frameServer.Metadata.HighSpeedFactor / frameServer.Metadata.UserInterval;
            frameServer.Metadata.UpdateTrajectoriesForKeyframes();

            view.UpdateTimebase();
            view.UpdateTimeLabels();
            view.RefreshImage();
        }
        
        public Bitmap GetFlushedImage()
        {
            return view.GetFlushedImage();
        }
        
        public void ShowCoordinateSystem()
        {
            frameServer.Metadata.ShowCoordinateSystem();
            view.RefreshImage();
        }

        public void ShowCameraCalibration()
        {
            List<List<PointF>> points = frameServer.Metadata.GetCameraCalibrationPoints();
            
            FormCalibrateDistortion fcd = new FormCalibrateDistortion(frameServer.CurrentImage, points, frameServer.Metadata.CalibrationHelper);
            FormsHelper.Locate(fcd);
            fcd.ShowDialog();
            fcd.Dispose();

            view.RefreshImage();
        }

        public void ShowTrajectoryAnalysis()
        {
            FormMultiTrajectoryAnalysis f = new FormMultiTrajectoryAnalysis(frameServer.Metadata);
            FormsHelper.Locate(f);
            f.ShowDialog();
            f.Dispose();
        }

        public void ShowScatterDiagram()
        {
            FormPointsAnalysis fpa = new FormPointsAnalysis(frameServer.Metadata);
            FormsHelper.Locate(fpa);
            fpa.ShowDialog();
            fpa.Dispose();
        }

        public void ShowAngularAnalysis()
        {
            FormAngularAnalysis f = new FormAngularAnalysis(frameServer.Metadata);
            FormsHelper.Locate(f);
            f.ShowDialog();
            f.Dispose();
        }

        public void ShowAngleAngleAnalysis()
        {
            FormAngleAngleAnalysis f = new FormAngleAngleAnalysis(frameServer.Metadata);
            FormsHelper.Locate(f);
            f.ShowDialog();
            f.Dispose();
        }

        public void ShowDataAnalysis(AbstractDrawing drawing)
        {
            /*if (drawing is DrawingCrossMark)
            {
                FormPointsAnalysis fpa = new FormPointsAnalysis(frameServer.Metadata);
                FormsHelper.Locate(fpa);
                fpa.ShowDialog();
                fpa.Dispose();
            }
            else if (drawing is DrawingTrack)
            {
                FormTrackAnalysis fta = new FormTrackAnalysis(frameServer.Metadata, drawing as DrawingTrack);
                FormsHelper.Locate(fta);
                fta.ShowDialog();
                fta.Dispose();
            }*/
        }
        #endregion

        public void AfterLoad()
        {
            OnActivated(EventArgs.Empty);
        }

        public void StartReplayWatcher(ScreenDescriptionPlayback sdp, string path)
        {
            replayWatcher.Start(sdp, path);
        }

        public void StopReplayWatcher()
        {
            replayWatcher.Close();
        }

        /// <summary>
        /// Convert from real time in microseconds relative to working zone start into absolute timestamps.
        /// </summary>
        private long RelativeRealTimeToAbsoluteTimestamp(long time)
        {
            double realtimeSeconds = (double)time / 1000000;
            double videoSeconds = realtimeSeconds * frameServer.Metadata.HighSpeedFactor;

            double correctedTPS = frameServer.VideoReader.Info.FrameIntervalMilliseconds * frameServer.VideoReader.Info.AverageTimeStampsPerSeconds / frameServer.Metadata.UserInterval;
            double timestamp = videoSeconds * correctedTPS;
            timestamp = Math.Round(timestamp);

            long relativeTimestamp = (long)timestamp;
            long absoluteTimestamp = relativeTimestamp + frameServer.VideoReader.WorkingZone.Start;
            
            return absoluteTimestamp;
        }

        private void AddDrawingWithMemento(Guid managerId, AbstractDrawing drawing)
        {
            // Temporary function.
            // Once the player screen ui uses the viewport, this event handler should be removed.
            // The code here should also be in the metadata manipulator until this function is removed.
            HistoryMementoAddDrawing memento = new HistoryMementoAddDrawing(frameServer.Metadata, managerId, drawing.Id, drawing.ToolDisplayName);
            frameServer.Metadata.AddDrawing(managerId, drawing);
            memento.UpdateCommandName(drawing.Name);
            historyStack.PushNewCommand(memento);
        }
        private void AddTrackableDrawing(ITrackable trackableDrawing)
        {
            frameServer.Metadata.TrackabilityManager.Add(trackableDrawing, frameServer.VideoReader.Current);
        }

        private void ToggleTracking(object parameter)
        {
            ITrackable trackableDrawing = ConvertToTrackable(parameter);
            if(trackableDrawing == null)
                return;
            
            frameServer.Metadata.TrackabilityManager.ToggleTracking(trackableDrawing);
        }
        private bool IsTracking(object parameter)
        {
            ITrackable trackableDrawing = ConvertToTrackable(parameter);
            if(trackableDrawing == null)
                return false;
            
            return frameServer.Metadata.TrackabilityManager.IsTracking(trackableDrawing);
        }
        
        private ITrackable ConvertToTrackable(object parameter)
        {
            ITrackable trackableDrawing = null;
            
            if(parameter is AbstractMultiDrawing)
            {
                AbstractMultiDrawing manager = parameter as AbstractMultiDrawing;
                if(manager != null)
                    trackableDrawing = manager.SelectedItem as ITrackable;    
            }
            else
            {
                trackableDrawing = parameter as ITrackable;
            }
            
            return trackableDrawing;
        }
        
        private void TrackDrawings(VideoFrame frameToUse)
        {
            VideoFrame frame = frameToUse ?? frameServer.VideoReader.Current;
            frameServer.Metadata.TrackabilityManager.Track(frame);
        }
    }
}
