using Autodesk.Revit.UI;

namespace RevitElementBipChecker.Model
{
  public static class DialogUtils
    {
        /// <summary>
        /// Return Result Has Select Element Current Or Not
        /// </summary>
        /// <param name="Caption">Title Question</param>
        /// <returns></returns>
        public static TaskDialogResult QuestionMsg(string Caption)
        {
            
            var dialog = new TaskDialog(Caption);

            dialog.MainIcon = TaskDialogIcon.TaskDialogIconNone;
            dialog.MainInstruction = Caption;

            dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Single Element Current");
            dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Multiples Element Current");
            dialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink3, "Element In RvtLinked");

            return dialog.Show();
        }
    }
}
