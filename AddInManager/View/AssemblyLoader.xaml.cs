using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using MessageBox = System.Windows.Forms.MessageBox;

namespace RevitAddinManager.View;

/// <summary>
/// Interaction logic for AssemblyLoader.xaml
/// </summary>
public partial class AssemblyLoader : Window
{
    private string _assemName;
    public string resultPath;
    private bool isFound;
    public AssemblyLoader(string assemName)
    {
        InitializeComponent();
        _assemName = assemName;
        tbxAssembly.Content = assemName;
    }
    private void ShowWarning()
    {
        var text = new StringBuilder("The dependent assembly can't be loaded: \"").Append(_assemName).AppendFormat("\".", new object[0]).ToString();
        MessageBox.Show(text, "Add-in Manager Internal", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
    }
    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_assemName))
        {
            MessageBox.Show("Assembly null",Resource.AppName);
            return;
        }
        using (var openFileDialog = new OpenFileDialog())
        {
            openFileDialog.Filter = "Assembly files (*.dll;*.exe,*.mcl)|*.dll;*.exe;*.mcl|All files|*.*||";
            var str = _assemName.Substring(0, _assemName.IndexOf(','));
            openFileDialog.FileName = str + ".*";
            if (openFileDialog.ShowDialog()==System.Windows.Forms.DialogResult.OK)
            {
                ShowWarning();
            }
            TbxAssemPath.Text = openFileDialog.FileName;
        }
    }

    private void OKButtonClick(object sender, RoutedEventArgs e)
    {
        if (File.Exists(TbxAssemPath.Text))
        {
            resultPath = TbxAssemPath.Text;
            isFound = true;
        }
        else
        {
            ShowWarning();
        }
        Close();
    }

    private void AssemblyLoader_OnClosing(object sender, CancelEventArgs e)
    {
        if (!isFound)
        {
            ShowWarning();
        }
    }
    private void Close_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}