using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MessageBox = System.Windows.Forms.MessageBox;

namespace AddinManager.View
{
    /// <summary>
    /// Interaction logic for AssemblyLoader.xaml
    /// </summary>
    public partial class AssemblyLoader : Window
    {
        private string m_assemName;
        public string resultPath;
        private bool isFound;
        public AssemblyLoader(string assemName)
        {
            InitializeComponent();
            this.m_assemName = assemName;
            this.tbxAssembly.Content = assemName;
        }
        private void ShowWarning()
        {
            string text = new StringBuilder("The dependent assembly can't be loaded: \"").Append(this.m_assemName).AppendFormat("\".", new object[0]).ToString();
            MessageBox.Show(text, "Add-in Manager Internal", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(m_assemName))
            {
                MessageBox.Show("Assembly null",Resource.AppName);
                return;
            }
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Assembly files (*.dll;*.exe,*.mcl)|*.dll;*.exe;*.mcl|All files|*.*||";
                string str = this.m_assemName.Substring(0, this.m_assemName.IndexOf(','));
                openFileDialog.FileName = str + ".*";
                if (openFileDialog.ShowDialog()==System.Windows.Forms.DialogResult.OK)
                {
                    this.ShowWarning();
                }
                this.TbxAssemPath.Text = openFileDialog.FileName;
            }
        }

        private void OKButtonClick(object sender, RoutedEventArgs e)
        {
            if (File.Exists(this.TbxAssemPath.Text))
            {
                this.resultPath = this.TbxAssemPath.Text;
                this.isFound = true;
            }
            else
            {
                this.ShowWarning();
            }
            base.Close();
        }

        private void AssemblyLoader_OnClosing(object sender, CancelEventArgs e)
        {
            if (!this.isFound)
            {
                this.ShowWarning();
            }
        }
        private void Close_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
