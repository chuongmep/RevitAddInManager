using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using QuickMsiBuilder.CLI;

namespace QuickMsiBuilder.UI
{
    /// <summary>
    /// Short result dialog. The full CLI output belongs in the log, not in a message box, so this
    /// only states the outcome and offers the two things worth doing next.
    /// </summary>
    public partial class BuildResultWindow : Window
    {
        private readonly string _msiPath;

        private BuildResultWindow(bool succeeded, string msiPath, string message)
        {
            InitializeComponent();

            _msiPath = msiPath;

            if (succeeded)
            {
                txtHeadline.Text = "MSI build finished.";
                txtSummary.Text = "The installer is ready.";
                txtPath.Text = msiPath;
                btnFolder.IsEnabled = !string.IsNullOrEmpty(msiPath) && File.Exists(msiPath);
            }
            else
            {
                txtHeadline.Text = "MSI build failed.";
                txtSummary.Text = "Open the log for the full output.";
                txtPath.Text = Shorten(message);
                btnFolder.Visibility = Visibility.Collapsed;
            }
        }

        public static void Show(Window owner, bool succeeded, string msiPath, string message)
        {
            var window = new BuildResultWindow(succeeded, msiPath, message) { Owner = owner };
            window.ShowDialog();
        }

        /// <summary>
        /// Failures are usually one useful line buried in WiX chatter; keep the tail, which is where
        /// the actual error lands.
        /// </summary>
        private static string Shorten(string message)
        {
            if (string.IsNullOrEmpty(message)) return "No output was produced.";

            var lines = message
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            var take = Math.Min(6, lines.Length);
            var tail = new string[take];
            Array.Copy(lines, lines.Length - take, tail, 0, take);

            return string.Join(Environment.NewLine, tail);
        }

        private void OnOpenFolder(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_msiPath)) return;

            try
            {
                // Opens the folder with the MSI already highlighted.
                Process.Start("explorer.exe", "/select,\"" + _msiPath + "\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Could not open the folder: " + ex.Message, Title);
            }
        }

        private void OnOpenLog(object sender, RoutedEventArgs e)
        {
            LogFile.Open(this);
        }

        private void OnOk(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
        }
    }
}
