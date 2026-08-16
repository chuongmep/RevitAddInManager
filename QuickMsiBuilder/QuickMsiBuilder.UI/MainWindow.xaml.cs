using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using QuickMsiBuilder.CLI;

namespace QuickMsiBuilder.UI
{
    public partial class MainWindow : Window
    {
        private readonly BuildHistoryStore _historyStore = new BuildHistoryStore();
        private readonly List<RevitYearOption> _revitYears;
        private List<AddinEntryOption> _entries = new List<AddinEntryOption>();

        public MainWindow()
        {
            InitializeComponent();

            _revitYears = RevitYearRange.Supported.Select(year => new RevitYearOption(year)).ToList();
            RefreshRevitYears();

            txtAuthor.Text = MsiBuildOptions.DefaultAuthor;

            // Args: [1] dll path, [2] revit years, [3] full class name, [4] add-in type.
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1 && !string.IsNullOrEmpty(args[1]))
            {
                txtDllPath.Text = args[1];
                ExtractMetadata(args[1]);
            }

            SelectRevitYears(args.Length > 2 ? args[2] : MsiBuildOptions.DefaultRevitYear);
            if (args.Length > 3 && !string.IsNullOrEmpty(args[3])) SetClassNames(args[3]);
            if (args.Length > 4) SelectAddinType(args[4]);

            // Loaded last so a previous release wins over the defaults read from the assembly.
            LoadHistory(txtDllPath.Text, true);
        }

        private void OnWindowKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
        }

        #region History

        /// <summary>
        /// Fills the history picker for the given assembly. When <paramref name="applyLatest"/> is
        /// set, the most recent release is also applied to the form.
        /// </summary>
        private void LoadHistory(string dllPath, bool applyLatest)
        {
            var entries = _historyStore.GetFor(dllPath);

            cbHistory.ItemsSource = entries;
            btnReuse.IsEnabled = entries.Count > 0;

            if (entries.Count == 0)
            {
                txtHistoryHint.Text = "No previous release recorded for this assembly.";
                return;
            }

            var latest = entries[0];
            txtHistoryHint.Text = string.Format("Last release: {0} for Revit {1} on {2:yyyy-MM-dd HH:mm}",
                latest.Version, latest.RevitYears, latest.BuiltUtc.ToLocalTime());

            if (!applyLatest) return;

            cbHistory.SelectedIndex = 0;
            Apply(latest);
        }

        private void Apply(BuildHistoryEntry entry)
        {
            if (entry == null) return;

            if (!string.IsNullOrEmpty(entry.Version)) txtVersion.Text = entry.Version;
            if (!string.IsNullOrEmpty(entry.Author)) txtAuthor.Text = entry.Author;
            if (!string.IsNullOrEmpty(entry.Description)) txtDescription.Text = entry.Description;
            if (!string.IsNullOrEmpty(entry.FullClassName)) SetClassNames(entry.FullClassName);
            txtIconPath.Text = entry.IconPath ?? string.Empty;
            txtBgPath.Text = entry.BackgroundImagePath ?? string.Empty;

            SelectAddinType(entry.AddinType);
            SelectRevitYears(entry.RevitYears);
        }

        private void OnHistorySelected(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            btnReuse.IsEnabled = cbHistory.SelectedItem != null;
        }

        private void OnReuseHistory(object sender, RoutedEventArgs e)
        {
            Apply(cbHistory.SelectedItem as BuildHistoryEntry);
        }

        #endregion

        #region Form state

        private void SelectRevitYears(string years)
        {
            if (string.IsNullOrEmpty(years)) return;

            var wanted = years
                .Split(new[] { ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(value => value.Trim())
                .Where(value => value.Length > 0)
                .ToList();

            // The default list covers the releases currently in support. A request outside it - an
            // older Revit still in use, or a release newer than this build - is added rather than
            // dropped, so the tick box the user needs is always there.
            var added = false;
            foreach (var year in wanted.Where(year => _revitYears.All(option => option.Year != year)))
            {
                _revitYears.Add(new RevitYearOption(year));
                added = true;
            }

            if (added)
            {
                _revitYears.Sort((left, right) => string.CompareOrdinal(left.Year, right.Year));
                RefreshRevitYears();
            }

            foreach (var option in _revitYears) option.IsSelected = wanted.Contains(option.Year);
        }

        private void RefreshRevitYears()
        {
            icRevitYears.ItemsSource = null;
            icRevitYears.ItemsSource = _revitYears;
        }

        private string SelectedRevitYears()
        {
            return string.Join(",", _revitYears.Where(o => o.IsSelected).Select(o => o.Year).ToArray());
        }

        private void SelectAddinType(string addinType)
        {
            if (string.IsNullOrEmpty(addinType)) return;
            cbAddinType.SelectedIndex =
                string.Equals(addinType, "Application", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
        }

        private void OnBrowseDll(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "DLL Files (*.dll)|*.dll" };
            if (dialog.ShowDialog() == true)
            {
                txtDllPath.Text = dialog.FileName;
                ExtractMetadata(dialog.FileName);
                LoadHistory(dialog.FileName, true);
            }
        }

        /// <summary>
        /// Prefills the form from the assembly itself: version, publisher, description and the
        /// add-in entry points, so nothing has to be typed by hand for a normal build.
        /// </summary>
        private void ExtractMetadata(string dllPath)
        {
            try
            {
                if (!File.Exists(dllPath)) return;

                var details = AssemblyInspector.Inspect(dllPath);

                if (!string.IsNullOrEmpty(details.Version)) txtVersion.Text = details.Version;
                if (!string.IsNullOrEmpty(details.Company)) txtAuthor.Text = details.Company;
                if (!string.IsNullOrEmpty(details.Description)) txtDescription.Text = details.Description;

                LoadClassNames(details);
            }
            catch { /* Ignore errors in metadata extraction */ }
        }

        /// <summary>
        /// Lists every entry point the assembly declares, with all commands ticked - packaging the
        /// whole set is what a developer shipping their own add-in wants by default.
        /// </summary>
        private void LoadClassNames(AssemblyDetails details)
        {
            _entries = details.Candidates
                .Select(candidate => new AddinEntryOption(candidate, candidate.AddinType == RevitAddinType.Command))
                .ToList();

            // An assembly with only applications should not come up with nothing ticked.
            if (_entries.Count > 0 && _entries.All(entry => !entry.IsSelected))
            {
                foreach (var entry in _entries) entry.IsSelected = true;
            }

            icEntries.ItemsSource = _entries;
            ShowManualClassRow(_entries.Count == 0);

            txtEntriesHint.Text = _entries.Count == 0
                ? "No Revit entry point was found in this assembly. Enter the class name below."
                : "Every command found in the assembly is packaged. Untick what you do not want.";
        }

        /// <summary>
        /// The manual class row is a fallback for assemblies the inspector cannot read; it only gets
        /// in the way when real entry points are on screen.
        /// </summary>
        private void ShowManualClassRow(bool visible)
        {
            var visibility = visible ? Visibility.Visible : Visibility.Collapsed;
            lblManualClass.Visibility = visibility;
            txtManualClass.Visibility = visibility;
            cbAddinType.Visibility = visibility;
        }

        private string SelectedClassNames()
        {
            var ticked = _entries
                .Where(entry => entry.IsSelected)
                .Select(entry => entry.FullClassName)
                .ToArray();

            return ticked.Length > 0
                ? string.Join(",", ticked)
                : (txtManualClass.Text ?? string.Empty).Trim();
        }

        /// <summary>
        /// Restores a selection coming from the add-in or from a previous release. Names the assembly
        /// does not declare are added to the list so they stay visible and ticked.
        /// </summary>
        private void SetClassNames(string classNames)
        {
            if (string.IsNullOrEmpty(classNames)) return;

            var wanted = classNames
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(value => value.Trim())
                .Where(value => value.Length > 0)
                .ToList();
            if (wanted.Count == 0) return;

            var addinType = cbAddinType.SelectedIndex == 1 ? RevitAddinType.Application : RevitAddinType.Command;
            foreach (var name in wanted.Where(name => _entries.All(entry => entry.FullClassName != name)))
            {
                _entries.Add(new AddinEntryOption(new AddinCandidate(name, addinType), true));
            }

            foreach (var entry in _entries) entry.IsSelected = wanted.Contains(entry.FullClassName);

            icEntries.ItemsSource = null;
            icEntries.ItemsSource = _entries;
            ShowManualClassRow(_entries.Count == 0);
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

        private void OnClearIcon(object sender, RoutedEventArgs e)
        {
            txtIconPath.Text = string.Empty;
        }

        private void OnClearBg(object sender, RoutedEventArgs e)
        {
            txtBgPath.Text = string.Empty;
        }

        private void OnOpenLog(object sender, RoutedEventArgs e)
        {
            LogFile.Open(this);
        }

        #endregion

        #region Build

        private async void OnBuildMsi(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDllPath.Text))
            {
                MessageBox.Show(this, "Please select a target DLL.", Title);
                return;
            }

            if (!File.Exists(txtDllPath.Text))
            {
                MessageBox.Show(this, "The selected DLL no longer exists:\n" + txtDllPath.Text, Title);
                return;
            }

            var revitYears = SelectedRevitYears();
            if (string.IsNullOrEmpty(revitYears))
            {
                MessageBox.Show(this, "Select at least one Revit version.", Title);
                return;
            }

            var cliPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "QuickMsiBuilder.CLI.exe");
            if (!File.Exists(cliPath))
            {
                MessageBox.Show(this, "QuickMsiBuilder.CLI.exe was not found next to this application.", Title);
                return;
            }

            var addinType = cbAddinType.SelectedIndex == 1 ? "Application" : "Command";

            var arguments = Quote(
                txtDllPath.Text,
                txtVersion.Text,
                txtAuthor.Text,
                txtDescription.Text,
                txtIconPath.Text,
                txtBgPath.Text,
                revitYears,
                SelectedClassNames(),
                addinType);

            btnBuild.IsEnabled = false;
            txtStatus.Text = "Building MSI for Revit " + revitYears.Replace(",", ", ") + "...";
            try
            {
                var result = await Task.Run(() => RunCli(cliPath, arguments));
                var succeeded = result.ExitCode == 0;
                var msiPath = Program.ParseResultPath(result.Output);
                var message = Program.StripResultLines(result.Output);

                txtStatus.Text = succeeded
                    ? "Built " + Path.GetFileName(msiPath)
                    : "Build failed - see the log for details.";

                // The CLI records the release, so pick it up without overwriting what is on screen.
                if (succeeded) LoadHistory(txtDllPath.Text, false);

                BuildResultWindow.Show(this, succeeded, msiPath, message);
            }
            catch (Exception ex)
            {
                txtStatus.Text = ex.Message;
                MessageBox.Show(this, "Error launching CLI: " + ex.Message, Title);
            }
            finally
            {
                btnBuild.IsEnabled = true;
            }
        }

        private static CliResult RunCli(string cliPath, string arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = cliPath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = Process.Start(startInfo))
            {
                var output = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
                process.WaitForExit();
                return new CliResult { ExitCode = process.ExitCode, Output = output };
            }
        }

        /// <summary>
        /// Free text fields can contain quotes and backslashes, so the command line has to be
        /// escaped rather than string-interpolated.
        /// </summary>
        private static string Quote(params string[] values)
        {
            var builder = new StringBuilder();
            foreach (var value in values)
            {
                if (builder.Length > 0) builder.Append(' ');
                builder.Append(Escape(value ?? string.Empty));
            }

            return builder.ToString();
        }

        private static string Escape(string value)
        {
            var builder = new StringBuilder("\"");
            for (var i = 0; i < value.Length; i++)
            {
                var backslashes = 0;
                while (i < value.Length && value[i] == '\\')
                {
                    backslashes++;
                    i++;
                }

                if (i == value.Length)
                {
                    builder.Append('\\', backslashes * 2);
                    break;
                }

                if (value[i] == '"')
                {
                    builder.Append('\\', backslashes * 2 + 1).Append('"');
                }
                else
                {
                    builder.Append('\\', backslashes).Append(value[i]);
                }
            }

            return builder.Append('"').ToString();
        }

        private class CliResult
        {
            public int ExitCode { get; set; }
            public string Output { get; set; }
        }

        #endregion
    }
}
