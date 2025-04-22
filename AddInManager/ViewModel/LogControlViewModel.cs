using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using RevitAddinManager.Model;
using RevitAddinManager.View.Control;

namespace RevitAddinManager.ViewModel;

public sealed class LogControlViewModel
{
    private object _lockObj = new object();
    private const int MAX_MESSGAES = 200;

    delegate void DMessageAdd(string message, System.Windows.Media.Brush color);

    private int _counter = 0;
    private FileSystemWatcher _watcher;
    private long _startLineTotal;
    private long _lastFileSize;
    public string LongFileName { get; set; }
    bool stopWatching;
    public FontFamily DisplayFontFamily { get; set; }
    private System.Windows.Media.Brush MessageColor { get; set; }
    public Dispatcher DispatcherObject { get; }
    public ObservableCollection<LogMessageString> MessageList { get; set; }
    public bool StopWatching
    {
        get
        {
            lock (_lockObj)
            {
                return stopWatching;
            }
        }
        set
        {
            lock (_lockObj)
            {
                stopWatching = value;
            }
        }
    }

    private ICommand clearLogCommand;
    public ICommand ClearLogCommand => clearLogCommand ??= new RelayCommand(ClearLogClick);
    private void ClearLogClick()
    {
        File.WriteAllText(LongFileName, String.Empty);
        MessageList.Clear();
    }

    public LogControlViewModel()
    {
        DispatcherObject = Dispatcher.CurrentDispatcher;
        MessageList = new ObservableCollection<LogMessageString>();
        DisplayFontFamily = new FontFamily("Courier New");
        LongFileName = DefaultSetting.PathLogFile;
    }

    public void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        StopWatching = true;
        Task.Factory.StartNew(() =>
        {
            try
            {
                if (_watcher == null) return;
                FileSystemEventHandler handler = new FileSystemEventHandler(FileWatcherChanged);
                _watcher.Changed -= handler;
                _watcher.Dispose();
                _watcher = null;
            }
            catch (Exception)
            {
                    //
            }
        });
    }

    public void LogFileWatcher(object sender, RoutedEventArgs e)
    {
        _startLineTotal = GetTotalLinesInFile(LongFileName, ref _lastFileSize);
       // if (_startLineTotal == 0) return;
        try { _lastFileSize = _lastFileSize - (_lastFileSize / _startLineTotal * MAX_MESSGAES); } 
        catch (Exception) { _lastFileSize = 0; }
        if (_lastFileSize < 0) _lastFileSize = 0;
        StopWatching = false;
        WatchLogFile(LongFileName);
        Task.Factory.StartNew(() =>
        {
            string path = Path.GetDirectoryName(LongFileName);
            string baseName = Path.GetFileName(LongFileName);
            _watcher = new FileSystemWatcher(path, baseName);
            FileSystemEventHandler handler = FileWatcherChanged;
            _watcher.Changed += handler;
            _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size;
            _watcher.EnableRaisingEvents = true;
        });

    }
    private long GetTotalLinesInFile(string filePath, ref long fileSize)
    {
        try
        {
            using (StreamReader r = new StreamReader(filePath))
            {
                long counts = 0;
                fileSize = new FileInfo(filePath).Length;
                while (r.ReadLine() != null) { counts++; }
                return counts;
            }
        }
        catch (Exception)
        {
            return 0;
        }
    }
    private bool IsExecuting { get; set; }
    private void FileWatcherChanged(object sender, FileSystemEventArgs e)
    {
        if (!IsExecuting)
        {
            IsExecuting = true;
            WatchLogFile(LongFileName);
            IsExecuting = false;
        }
    }

    private void WatchLogFile(string fileName)
    {
        int count = 1;
        int trys = 5;
        long newLength = 0;
        string newFileLines = "";
        bool success = false;
        while (count <= trys && !success && !StopWatching)
        {
            try
            {
                using (FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    newLength = stream.Length;
                    if (newLength >= _lastFileSize)
                        stream.Position = _lastFileSize;
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        newFileLines = reader.ReadToEnd();
                        string[] stringSeparators = new string[] { "\r\n" };
                        string[] lines = newFileLines.Split(stringSeparators, StringSplitOptions.None);
                        foreach (string s in lines)
                            if (!String.IsNullOrEmpty(s))
                            {
                                LístBoxColorMessageAdd(s);
                            }
                        _lastFileSize = newLength;
                    }
                }
                success = true;
            }
            catch (Exception)
            {
                // ignore
            }
            ++count;
        }
    }

    private void LístBoxColorMessageAdd(string message)
    {
        if (message.CaseInsensitiveContains("Modify"))
            ListBoxLogMessageAdd(message, System.Windows.Media.Brushes.DeepSkyBlue);
        else if (message.CaseInsensitiveContains("Delete"))
            ListBoxLogMessageAdd(message, System.Windows.Media.Brushes.Gray);
        else if (message.CaseInsensitiveContains("Add"))
            ListBoxLogMessageAdd(message, System.Windows.Media.Brushes.Blue);
        else if (message.CaseInsensitiveContains("Error"))
            ListBoxLogMessageAdd(message, System.Windows.Media.Brushes.Red);
        else if (message.CaseInsensitiveContains("Warning"))
            ListBoxLogMessageAdd(message, System.Windows.Media.Brushes.OrangeRed);
        else
            ListBoxLogMessageAdd(message, System.Windows.Media.Brushes.Black);
    }
    private void ListBoxLogMessageAdd(string message, System.Windows.Media.Brush color)
    {
        if (DispatcherObject.Thread != Thread.CurrentThread)
            DispatcherObject.Invoke(new DMessageAdd(ListBoxLogMessageAdd), DispatcherPriority.ApplicationIdle, message, color);
        else
        {
            MessageList.Add(new LogMessageString(String.Format("{0} {1}", ++_counter, message), color, FontWeights.Normal, 14));
            App.FrmLogControl.listBox_LogMessages.SelectedIndex = App.FrmLogControl.listBox_LogMessages.Items.Count - 1;
            App.FrmLogControl.listBox_LogMessages.ScrollIntoView(App.FrmLogControl.listBox_LogMessages.SelectedItem);
            if (MessageList.Count > MAX_MESSGAES)
                MessageList.RemoveAt(0);
        }
    }
}