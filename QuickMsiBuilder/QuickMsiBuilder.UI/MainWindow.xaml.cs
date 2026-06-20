using System.Diagnostics;
using System.Reflection;
using System.Windows;
using Microsoft.Win32;
using System.IO;

namespace QuickMsiBuilder.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Check for command line arguments
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                txtDllPath.Text = args[1];
                ExtractMetadata(args[1]);
            }
            if (args.Length > 2)
            {
                string revitYear = args[2];
                foreach (System.Windows.Controls.ComboBoxItem item in cbRevitYear.Items)
                {
                    if (item.Content.ToString() == revitYear)
                    {
                        item.IsSelected = true;
                        break;
                    }
                }
            }
        }

        private void OnBrowseDll(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "DLL Files (*.dll)|*.dll" };
            if (dialog.ShowDialog() == true)
            {
                txtDllPath.Text = dialog.FileName;
                ExtractMetadata(dialog.FileName);
            }
        }

        private void ExtractMetadata(string dllPath)
        {
            try
            {
                if (File.Exists(dllPath))
                {
                    var fileVersionInfo = FileVersionInfo.GetVersionInfo(dllPath);
                    if (!string.IsNullOrEmpty(fileVersionInfo.FileVersion))
                        txtVersion.Text = fileVersionInfo.FileVersion;

                    if (!string.IsNullOrEmpty(fileVersionInfo.CompanyName))
                        txtAuthor.Text = fileVersionInfo.CompanyName;

                    if (!string.IsNullOrEmpty(fileVersionInfo.FileDescription))
                        txtDescription.Text = fileVersionInfo.FileDescription;
                }
            }
            catch { /* Ignore errors in metadata extraction */ }
        }

        private void OnBrowseIcon(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "Icon Files (*.ico)|*.ico" };
            if (dialog.ShowDialog() == true) txtIconPath.Text = dialog.FileName;
        }

        private void OnBrowseBg(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "Image Files (*.png;*.jpg)|*.png;*.jpg" };
            if (dialog.ShowDialog() == true) txtBgPath.Text = dialog.FileName;
        }

        private void OnBuildMsi(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtDllPath.Text))
            {
                MessageBox.Show("Please select a target DLL.");
                return;
            }

            // Logic for Step 13
            string cliPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "QuickMsiBuilder.CLI.exe");
            string revitYear = ((System.Windows.Controls.ComboBoxItem)cbRevitYear.SelectedItem).Content.ToString();

            string arguments = $"\"{txtDllPath.Text}\" \"{txtVersion.Text}\" \"{txtAuthor.Text}\" \"{txtDescription.Text}\" \"{txtIconPath.Text}\" \"{txtBgPath.Text}\" \"{revitYear}\"";

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = cliPath,
                    Arguments = arguments,
                    UseShellExecute = true
                });
                MessageBox.Show("MSI build process started in background.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error launching CLI: {ex.Message}");
            }
        }
    }
}
