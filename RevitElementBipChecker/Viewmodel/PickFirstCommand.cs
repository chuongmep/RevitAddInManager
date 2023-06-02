using System;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using RevitElementBipChecker.Model;

namespace RevitElementBipChecker.Viewmodel
{
    public class PickFirstCommand : BaseElement
    {
        public PickFirstCommand(BipCheckerViewmodel viewmodel)
        {
            this.Viewmodel = viewmodel;
            PickFirst();
            viewmodel.GetDatabase();
        }
        void PickFirst()
        {
            if (Viewmodel.Element == null)
            {
                bool isintance = DialogUtils.QuestionMsg("Select Option Snoop Element Or LinkElement");
                if (isintance == false)
                {
                    PickLink_Element();
                }
                else
                {
                    PickElement();
                }
            }
        }
        private void PickLink_Element()
        {
            try
            {
                Reference pickObject = Viewmodel.UIDoc.Selection.PickObject(ObjectType.LinkedElement, "Select Element In Linked ");
                Element e = Viewmodel.UIDoc.Document.GetElement(pickObject);
                if (e is RevitLinkInstance)
                {
                    RevitLinkInstance linkInstance = e as RevitLinkInstance;
                    Document linkDocument = linkInstance.GetLinkDocument();
                    Element = linkDocument.GetElement(pickObject.LinkedElementId);
                    Viewmodel.Element = Element;
                    Viewmodel.State = "Link Element";
                }

            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException) { }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());

            }
        }

        void PickElement()
        {

            try
            {
                Reference pickObject = Viewmodel.UIDoc.Selection.PickObject(ObjectType.Element);
                Element = Viewmodel.UIDoc.Document.GetElement(pickObject);
                Viewmodel.Element = Element;
                Viewmodel.State = "Element";
                if (Element.CanHaveTypeAssigned())
                {
                    ElementType = Viewmodel.UIDoc.Document.GetElement(Element.GetTypeId());
                    Viewmodel.ElementType = ElementType;
                }
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException) { }
            catch (Exception)
            {
                // ignored
            }
        }

        public void Toggle()
        {
            Viewmodel.frmmain?.Hide();
            Viewmodel.Element = null;
            PickFirst();
            Viewmodel.GetDatabase();
            Viewmodel.frmmain?.Show();
        }

        public void ToggleTypeInstance()
        {
            Viewmodel.GetDatabase();
        }
    }
}
