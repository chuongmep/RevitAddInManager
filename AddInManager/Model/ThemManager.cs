using System.Windows;

namespace RevitAddinManager.Model;

public static class ThemManager
{
    public static void ChangeThem(bool isStartup)
    {
        try
        {
            if (!isStartup) App.ThemId += 1;
            App.FrmAddInManager.Resources.MergedDictionaries.Clear();
            App.FrmLogControl.Resources.MergedDictionaries.Clear();
            switch (App.ThemId)
            {
                case 0:
                    App.FrmAddInManager.Resources.MergedDictionaries.Add(new ResourceDictionary()
                    {
                        Source = new Uri("/RevitAddinManager;component/Themes/Styles/DarkTheme.xaml",
                            UriKind.RelativeOrAbsolute)
                    });
                    App.FrmLogControl.Resources.MergedDictionaries.Add(new ResourceDictionary()
                    {
                        Source = new Uri("/RevitAddinManager;component/Themes/Styles/DarkTheme.xaml",
                            UriKind.RelativeOrAbsolute)
                    });
                    break;
                case 1:
                    App.FrmAddInManager.Resources.MergedDictionaries.Add(new ResourceDictionary()
                    {
                        Source = new Uri("/RevitAddinManager;component/Themes/Styles/LightTheme.xaml",
                            UriKind.RelativeOrAbsolute)
                    });
                    App.FrmLogControl.Resources.MergedDictionaries.Add(new ResourceDictionary()
                    {
                        Source = new Uri("/RevitAddinManager;component/Themes/Styles/LightTheme.xaml",
                            UriKind.RelativeOrAbsolute)
                    });
                    break;
                default:
                    App.FrmAddInManager.Resources.MergedDictionaries.Add(new ResourceDictionary()
                    {
                        Source = new Uri(
                            "/PresentationFramework.Royale;V3.0.0.0;31bf3856ad364e35;component/themes/royale.normalcolor.xaml",
                            UriKind.RelativeOrAbsolute)
                    });
                    App.FrmLogControl.Resources.MergedDictionaries.Add(new ResourceDictionary()
                    {
                        Source = new Uri("/RevitAddinManager;component/Themes/Styles/LightTheme.xaml",
                            UriKind.RelativeOrAbsolute)
                    });
                    App.ThemId = -1;
                    break;
            }
        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString());
        }
    }
}