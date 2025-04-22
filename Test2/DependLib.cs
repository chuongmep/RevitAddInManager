namespace Test2
{
    public static class DependLib
    {
        public static double Plus()
        {
            return 2 + 10;
        }

        public static string? ShowDialogFolder()
        {
            //ookii-dialogs-wpf
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            dialog.Description = "Select a folder";
            dialog.UseDescriptionForTitle = true;
            dialog.ShowNewFolderButton = true;
            if (dialog.ShowDialog() == true)
            {
                return dialog.SelectedPath;
            }
            return null;
        }
    }
}