#region License
/*
Copyright � Joan Charmant 2008
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
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

using AForge.Imaging.Filters;
using Kinovea.Services;

namespace Kinovea.ScreenManager
{
    public class Keyframe : AbstractDrawingManager, IComparable
    {
        #region Properties
        public override Guid Id
        {
            get { return id; }
            set { id = value; }
        }
        public long Position
        {
            get { return position; }
            set { position = value;}
        }
        public Bitmap Thumbnail
        {
            get { return thumbnail; }
        }
        public Bitmap DisabledThumbnail
        {
            get { return disabledThumbnail; }
        }
        public bool HasThumbnails
        {
            get { return thumbnail != null; }
        }
        public List<AbstractDrawing> Drawings
        {
            get { return drawings; }
        }
        public string Comments
        {
            get { return comments; }
            set { comments = value; }
        }
        public string Title
        {
            get 
            {
                return string.IsNullOrEmpty(title) ? timecode : title;
            }
            set 
            { 
                title = value;
                metadata.UpdateTrajectoriesForKeyframes();
            }
        }
        public string TimeCode
        {
            get { return timecode; }
            set { timecode = value; }
        }
        public bool Disabled
        {
            get { return disabled; }
            set { disabled = value; }
        }
        public int ContentHash
        {
            get { return GetContentHash();}
        }

        public IList<KeyFrameEvent> Events { get; } = new List<KeyFrameEvent>();
        #endregion

        #region Members
        private Guid id = Guid.NewGuid();
        private bool initialized;
        private long position = -1;            // Position is absolute in all timestamps.
        private string title;
        private string timecode;
        private string comments;
        private Bitmap thumbnail;
        private Bitmap disabledThumbnail;
        private List<AbstractDrawing> drawings = new List<AbstractDrawing>();
        private bool disabled;
        private Metadata metadata;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Events
        public event EventHandler<KeyFrameEventChangedArgs> EventAdded;
        public event EventHandler<KeyFrameEventChangedArgs> EventRemoved;
        #endregion

        #region Constructor
        public Keyframe(long position, string timecode, Metadata metadata)
        {
            this.position = position;
            this.timecode = timecode;
            this.metadata = metadata;
        }
        public Keyframe(Guid id, long position, string title, string timecode, string comments, List<AbstractDrawing> drawings, Metadata metadata)
        {
            this.id = id;
            this.position = position;
            this.title = title;
            this.timecode = timecode;
            this.comments = comments;
            this.drawings = drawings;
            this.metadata = metadata;
        }
        public Keyframe(XmlReader xmlReader, PointF scale, TimestampMapper timestampMapper, Metadata metadata)
            : this(0, "", metadata)
        {
            ReadXml(xmlReader, scale, timestampMapper);
        }
        #endregion

        #region Public Interface
        public void InitializePosition(long position)
        {
            this.position = position;
        }
        public void InitializeImage(Bitmap image)
        {
            if (image == null)
                return;
            
            Rectangle rect = UIHelper.RatioStretch(image.Size, new Size(100, 75));
            this.thumbnail = new Bitmap(image, rect.Width, rect.Height);
            this.disabledThumbnail = Grayscale.CommonAlgorithms.BT709.Apply(thumbnail);
        }

        #region Events
        public HistoryMemento ToggleEvent(EventDefinition evt)
        {
            var current = this.Events.FirstOrDefault(kfe => kfe.Name == evt.Title);
            if (current != null)
            {
                return this.RemoveEvent(current)
                 ? new HistoryMementoRemoveEvent(this, current)
                 : null;
            }

            current = evt.GenerateKeyFrameEvent();
            return this.AddEvent(current) 
                ? new HistoryMementoAddEvent(this, current)
                : null;
        }

        public bool AddEvent(KeyFrameEvent evt)
        {
            if (this.Events.Contains(evt)) return false;

            this.Events.Add(evt);
            this.EventAdded?.Invoke(this, new KeyFrameEventChangedArgs(evt));
            return true;
        }

        public bool RemoveEvent(KeyFrameEvent evt)
        {
            if (!this.Events.Contains(evt)) return false;

            this.Events.Remove(evt);
            this.EventRemoved?.Invoke(this, new KeyFrameEventChangedArgs(evt));
            return true;
        }
        #endregion
        #endregion

        #region AbstractDrawingManager implementation
        public override AbstractDrawing GetDrawing(Guid id)
        {
            return drawings.FirstOrDefault(d => d.Id == id);
        }
        public override void AddDrawing(AbstractDrawing drawing)
        {
            // insert to the top of z-order except for grids.
            if (drawing is DrawingPlane)
                drawings.Add(drawing);
            else
                drawings.Insert(0, drawing);
        }
        public override void RemoveDrawing(Guid id)
        {
            drawings.RemoveAll(d => d.Id == id);
        }
        public override void Clear()
        {
            drawings.Clear();
        }
        #endregion

        #region KVA Serialization
        public void WriteXml(XmlWriter w)
        {
            w.WriteStartElement("Position");
            string userTime = metadata.TimeCodeBuilder(position, TimeType.UserOrigin, TimecodeFormat.Unknown, false);
            w.WriteAttributeString("UserTime", userTime);
            w.WriteString(position.ToString());
            w.WriteEndElement();

            if (!string.IsNullOrEmpty(Title))
                w.WriteElementString("Title", Title);

            if (!string.IsNullOrEmpty(comments))
                w.WriteElementString("Comment", comments);

            if (drawings.Count > 0)
            {
                // Drawings are written in reverse order to match order of addition.
                w.WriteStartElement("Drawings");
                for (int i = drawings.Count - 1; i >= 0; i--)
                {
                    IKvaSerializable serializableDrawing = drawings[i] as IKvaSerializable;
                    if (serializableDrawing == null)
                        continue;

                    DrawingSerializer.Serialize(w, serializableDrawing, SerializationFilter.All);
                }
                w.WriteEndElement();
            }

            if (this.Events.Count > 0)
            {
                w.WriteStartElement("events");
                foreach (var evt in this.Events)
                {
                    w.WriteStartElement("event");
                    evt.WriteXml(w);
                    w.WriteEndElement();
                }
                w.WriteEndElement();
            }
        }
        private void ReadXml(XmlReader r, PointF scale, TimestampMapper timestampMapper)
        {
            if (r.MoveToAttribute("id"))
                id = new Guid(r.ReadContentAsString());

            r.ReadStartElement();

            while (r.NodeType == XmlNodeType.Element)
            {
                switch (r.Name)
                {
                    case "Position":
                        int inputPosition = r.ReadElementContentAsInt();
                        position = timestampMapper(inputPosition, false);
                        break;
                    case "Title":
                        title = r.ReadElementContentAsString();
                        break;
                    case "Comment":
                        comments = r.ReadElementContentAsString();
                        break;
                    case "Drawings":
                        ParseDrawings(r, scale);
                        break;
                    case "events":
                        this.ParseEvents(r);
                        break;
                    default:
                        string unparsed = r.ReadOuterXml();
                        log.DebugFormat("Unparsed content in KVA XML: {0}", unparsed);
                        break;
                }
            }

            r.ReadEndElement();
        }
        private void ParseDrawings(XmlReader r, PointF scale)
        {
            // Note:
            // We do not do a metadata.AddDrawing at this point, only add the drawing internally to the keyframe without side-effects.
            // We wait for the complete keyframe to be parsed and ready, and then merge-insert it into the existing collection.
            // The drawing post-initialization will only be done at that point. 
            // This prevents doing post-initializations too early when the keyframe is not yet added to the metadata collection.

            bool isEmpty = r.IsEmptyElement;
            
            r.ReadStartElement();

            if (isEmpty)
                return;

            while (r.NodeType == XmlNodeType.Element)
            {
                AbstractDrawing drawing = DrawingSerializer.Deserialize(r, scale, TimeHelper.IdentityTimestampMapper, metadata);
                if (drawing == null || !drawing.IsValid)
                    continue;

                AddDrawing(drawing);
                drawing.InfosFading.ReferenceTimestamp = this.Position;
                drawing.InfosFading.AverageTimeStampsPerFrame = metadata.AverageTimeStampsPerFrame;
            }

            r.ReadEndElement();
        }

        private void ParseEvents(XmlReader r)
        {
            if (r.IsEmptyElement) return;

            r.Read();
            while ((r.NodeType == XmlNodeType.Element) && (r.Name == "event"))
            {
                if (r.HasAttributes) this.Events.Add(KeyFrameEvent.ReadFromXml(r));
                r.Read();
            }
            r.ReadEndElement();
        }

        #endregion

        #region IComparable Implementation
        public int CompareTo(object obj)
        {
            if(obj is Keyframe)
                return this.position.CompareTo(((Keyframe)obj).position);
            else
                throw new ArgumentException("Impossible comparison");
        }
        #endregion

        #region LowerLevel Helpers
        private int GetContentHash()
        {
            int hash = 0;
            foreach (AbstractDrawing drawing in drawings)
                hash ^= drawing.ContentHash;

            foreach (var evt in this.Events)
                hash ^= evt.Name.GetHashCode();

            if(comments != null)
                hash ^= comments.GetHashCode();
            
            if(!string.IsNullOrEmpty(title))
                hash ^= title.GetHashCode();
            
            hash ^= timecode.GetHashCode();

            return hash;
        }
        #endregion
    }
}
