using System;
using System.Collections.Generic;
using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
            if (Viewmodel.Elements == null)
            {
                TaskDialogResult result = DialogUtils.QuestionMsg("Select Option Snoop Element Or LinkElement");
                switch (result)
                {
                    case TaskDialogResult.CommandLink1:
                        PickElement();
                        break;
                    case TaskDialogResult.CommandLink2:
                        PickElements();
                        break;
                    case TaskDialogResult.CommandLink3:
                        PickLinkElement();
                        break;
                    case TaskDialogResult.CommandLink4:
                        break;
                }
            }
        }
        private void PickLinkElement()
        {
            try
            {
                Reference pickObject = Viewmodel.UIDoc.Selection.PickObject(ObjectType.LinkedElement, "Select Element In Linked ");
                Element e = Viewmodel.UIDoc.Document.GetElement(pickObject);
                if (e is RevitLinkInstance)
                {
                    RevitLinkInstance linkInstance = e as RevitLinkInstance;
                    Document linkDocument = linkInstance.GetLinkDocument();
                    Elements = new List<Element>() {linkDocument.GetElement(pickObject.LinkedElementId)};
                    Viewmodel.Elements = Elements;
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
                Elements = new List<Element>(){Viewmodel.UIDoc.Document.GetElement(pickObject)};
                Viewmodel.Elements = Elements;
                Viewmodel.State = "Element";
                // if (Elements.CanHaveTypeAssigned())
                // {
                //     ElementType = Viewmodel.UIDoc.Document.GetElement(Elements.GetTypeId());
                //     Viewmodel.ElementType = ElementType;
                // }
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException) { }
            catch (Exception)
            {
                // ignored
            }
        }

        private void PickElements()
        {
            try
            {
                IList<Reference> pickObject = Viewmodel.UIDoc.Selection.PickObjects(ObjectType.Element);
                Elements = ToElements(pickObject, Viewmodel.UIDoc.Document);
                Viewmodel.Elements = Elements;
                Viewmodel.State = "Elements";
                // if (Elements.CanHaveTypeAssigned())
                // {
                //     ElementType = Viewmodel.UIDoc.Document.GetElement(Elements.GetTypeId());
                //     Viewmodel.ElementType = ElementType;
                // }
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException) { }
            catch (Exception)
            {
                // ignored
            }
        }
        List<Element> ToElements(IList<Reference> references, Document document)
        {
            List<Element> elements = new List<Element>();
            foreach (Reference reference in references)
            {
                elements.Add(document.GetElement(reference));
            }
            return elements;
        }

        public void Toggle()
        {
            Viewmodel.frmmain?.Hide();
            Viewmodel.Elements = null;
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
