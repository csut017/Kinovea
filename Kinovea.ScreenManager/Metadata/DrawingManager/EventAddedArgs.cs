using System;

namespace Kinovea.ScreenManager
{
    public class EventAddedArgs: EventArgs
    {
        public EventAddedArgs(KeyFrameEvent newEvent)
        {
            this.Event = newEvent;
        }

        public KeyFrameEvent Event { get; private set; }
    }
}