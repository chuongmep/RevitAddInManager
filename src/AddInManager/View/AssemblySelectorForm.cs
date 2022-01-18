using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace AddInManager.View
{
	public partial class AssemblySelectorForm : Form
	{
		public AssemblySelectorForm(string assemName)
		{
			this.InitializeComponent();
			this.m_assemName = assemName;
			this.assemNameTextBox.Text = assemName;
		}

		private void AssemblySelectorForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!this.m_found)
			{
				this.ShowWarning();
			}
		}

		private void browseButton_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog openFileDialog = new OpenFileDialog())
			{
				openFileDialog.Filter = "Assembly files (*.dll;*.exe,*.mcl)|*.dll;*.exe;*.mcl|All files|*.*||";
				string str = this.m_assemName.Substring(0, this.m_assemName.IndexOf(','));
				openFileDialog.FileName = str + ".*";
				if (openFileDialog.ShowDialog() != DialogResult.OK)
				{
					this.ShowWarning();
				}
				this.assemPathTextBox.Text = openFileDialog.FileName;
			}
		}

		private void okButton_Click(object sender, EventArgs e)
		{
			if (File.Exists(this.assemPathTextBox.Text))
			{
				this.m_resultPath = this.assemPathTextBox.Text;
				this.m_found = true;
			}
			else
			{
				this.ShowWarning();
			}
			base.Close();
		}

		private void ShowWarning()
		{
			string text = new StringBuilder("The dependent assembly can't be loaded: \"").Append(this.m_assemName).AppendFormat("\".", new object[0]).ToString();
			MessageBox.Show(text, "Add-in Manager Internal", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
		}

		private string m_assemName;

		private bool m_found;

		public string m_resultPath;
	}
}
