using System;

namespace Kinovea.ScreenManager
{
    public class KeyFrameEventChangedArgs: EventArgs
    {
        public KeyFrameEventChangedArgs(KeyFrameEvent newEvent)
        {
            this.Event = newEvent;
        }

        public KeyFrameEvent Event { get; private set; }
    }
}