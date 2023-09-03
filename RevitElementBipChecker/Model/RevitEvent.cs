using System;
using System.Linq;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitElementBipChecker.Model
{
    public class RevitEvent : IExternalEventHandler
    {
        private Action _doAction;
        private Document _doc;
        private readonly ExternalEvent _exEvent;
        private bool _skipFailures;
        private string _transactionName;
        private bool withTransaction;
        public RevitEvent()
        {
            _exEvent = ExternalEvent.Create(this);
        }
        public void Run(Action doAction, bool skipFailures, Document doc = null, string transactionName = null, bool withTrans = true)
        {
            _doAction = doAction;
            _skipFailures = skipFailures;
            _doc = doc;
            withTransaction = withTrans;
            _exEvent.Raise();
            _transactionName = transactionName;
        }
        public void Execute(UIApplication app)
        {
            try
            {
                if (_doAction != null)
                {
                    if (_doc == null) _doc = app.ActiveUIDocument.Document;
                    if (_skipFailures)
                        app.Application.FailuresProcessing += Application_FailuresProcessing;

                    if (withTransaction)
                    {
                        using (Transaction t = new Transaction(_doc, _transactionName ?? "RevitEvent"))
                        {
                            t.Start();
                            _doAction();
                            t.Commit();
                        }
                    }
                    else
                    {
                        _doAction();
                    }

                    if (_skipFailures)
                        app.Application.FailuresProcessing -= Application_FailuresProcessing;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + Environment.NewLine + exception.StackTrace);
                if (_skipFailures)
                    app.Application.FailuresProcessing -= Application_FailuresProcessing;
            }
        }

        private static void Application_FailuresProcessing(object sender, Autodesk.Revit.DB.Events.FailuresProcessingEventArgs e)
        {
            // Inside event handler, get all warnings
            var failList = e.GetFailuresAccessor().GetFailureMessages();
            if (failList.Any())
            {
                // skip all failures
                e.GetFailuresAccessor().DeleteAllWarnings();
                e.SetProcessingResult(FailureProcessingResult.Continue);
            }
        }

        public string GetName()
        {
            return "RevitEvent";
        }
    }
}
