using Kinovea.ScreenManager.Languages;

namespace Kinovea.ScreenManager
{
    public class HistoryMementoRemoveEvent : HistoryMemento
    {
        private readonly Keyframe keyFrame;
        private readonly KeyFrameEvent evt;

        public override string CommandName { get; set; }

        public HistoryMementoRemoveEvent(Keyframe keyFrame, KeyFrameEvent evt)
        {
            this.CommandName = string.Format("{0} ({1})", ScreenManagerLang.CommandRemoveEvent_FriendlyName, evt.Name);
            this.keyFrame = keyFrame;
            this.evt = evt;
        }

        public override HistoryMemento PerformUndo()
        {
            this.keyFrame.AddEvent(this.evt);
            return new HistoryMementoAddEvent(this.keyFrame, this.evt)
            {
                CommandName = this.CommandName
            };
        }
    }
}
