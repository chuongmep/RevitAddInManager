using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;

namespace RevitAddinManager.Model
{
    public class EventWatcher
    {
        public UIControlledApplication App { get; set; }
        public bool IsWatch { get; set; }
        public EventWatcher(UIControlledApplication app)
        {
            App = app;
            TraceLog();
        }

        void TraceLog()
        {
           // App.ControlledApplication.DocumentChanged += WatchDocumentChange;
        }

        /// <summary>
        /// Check when user change every thing element inside model
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WatchDocumentChange(object sender, DocumentChangedEventArgs e)
        {
            ICollection<ElementId> deletedElementIds = e.GetDeletedElementIds();
            SendMessage(e,deletedElementIds,EventType.Delete);
            ICollection<ElementId> modifiedElementIds = e.GetModifiedElementIds();
            SendMessage(e,modifiedElementIds,EventType.Modify);
            ICollection<ElementId> addedElementIds = e.GetAddedElementIds();
            SendMessage(e,addedElementIds,EventType.Add);
        }

      
        void SendMessage(EventArgs @event,ICollection<ElementId> ids,EventType eventType)
        {
            foreach (ElementId elementId in ids)
            {
#if R26
                SendMessage(@event, elementId.Value.ToString(), eventType);
#else
                SendMessage(@event,elementId.IntegerValue.ToString(),eventType);
#endif
            }
        }
        void SendMessage(EventArgs @event,string message,EventType eventType)
        {
            string result = string.Join("|",DateTime.Now, @event.GetType().Name, eventType.ToString(), message);
            WriteLine(result);
        }
        void WriteLine(string s)
        {
            using (StreamWriter st = new StreamWriter(DefaultSetting.PathLogFile, true))
            {
                st.WriteLine(s);
                st.Close();
            }
        }

    }
    //TODO : Tao mot class Ghi lai theo yeu cau : Ngay, Ten Su kien, Lam Gi,
    public enum EventType
    {
        Add,
        Delete,
        Modify,
        None
    }
}
