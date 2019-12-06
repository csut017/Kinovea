using System;

namespace Kinovea.ScreenManager
{
    public class EventDefinition
    {
        public string Title { get; set; }

        public KeyFrameEvent GenerateKeyFrameEvent()
        {
            return new KeyFrameEvent
            {
                Name = this.Title
            };
        }

        public override string ToString()
        {
            return this.Title;
        }
    }
}