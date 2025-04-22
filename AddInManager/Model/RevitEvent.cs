using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows;

namespace RevitAddinManager.Model
{
    public class RevitEvent : IExternalEventHandler
    {
        private Action DoAction;
        private readonly ExternalEvent ExEvent;
        private bool SkipFailures;
        private string TransactionName;
        private bool WithTransaction;

        public RevitEvent()
        {
            ExEvent = ExternalEvent.Create(this);
        }

        /// <summary>
        /// Execute A Command By IExternalEventHandler
        /// </summary>
        /// <param name="doAction">action to run</param>
        /// <param name="skipFailures">ignore error</param>
        /// <param name="doc">document</param>
        /// <param name="transactionName">transaction name</param>
        /// <param name="withTrans">is transaction</param>
        public void Run(Action doAction, bool skipFailures, string transactionName = null, bool withTrans = true)
        {
            this.DoAction = doAction;
            this.SkipFailures = skipFailures;
            WithTransaction = withTrans;
            ExEvent.Raise();
            this.TransactionName = transactionName;
        }

        public void Execute(UIApplication app)
        {
            try
            {
                if (DoAction != null)
                {
                   
                    if (SkipFailures)
                        app.Application.FailuresProcessing += Application_FailuresProcessing;

                    if (WithTransaction)
                    {
                        using (Transaction t = new Transaction(app.ActiveUIDocument.Document, TransactionName ?? "RevitEvent"))
                        {
                            t.Start();
                            DoAction();
                            t.Commit();
                        }
                    }
                    else
                    {
                        DoAction();
                    }

                    if (SkipFailures)
                        app.Application.FailuresProcessing -= Application_FailuresProcessing;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message + Environment.NewLine + exception.StackTrace);
                if (SkipFailures)
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