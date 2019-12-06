using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    public class HistoryMementoAddEvent : HistoryMemento
    {
        private readonly Keyframe keyFrame;
        private readonly KeyFrameEvent evt;

        public override string CommandName { get; set; }

        public HistoryMementoAddEvent(Keyframe keyFrame, KeyFrameEvent evt)
        {
            this.CommandName = string.Format("{0} ({1})", ScreenManagerLang.CommandAddEvent_FriendlyName, evt.Name);
            this.keyFrame = keyFrame;
            this.evt = evt;
        }

        public override HistoryMemento PerformUndo()
        {
            this.keyFrame.RemoveEvent(this.evt);
            return new HistoryMementoRemoveEvent(this.keyFrame, this.evt)
            {
                CommandName = this.CommandName
            };
        }
    }
}
