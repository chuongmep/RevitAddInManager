namespace AddInManager.View
{
	// Token: 0x02000010 RID: 16
	public partial class AssemblySelectorForm : global::System.Windows.Forms.Form
	{
		// Token: 0x06000073 RID: 115 RVA: 0x00003C9F File Offset: 0x00001E9F
		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		// Token: 0x06000074 RID: 116 RVA: 0x00003CC0 File Offset: 0x00001EC0
		private void InitializeComponent()
		{
			this.okButton = new global::System.Windows.Forms.Button();
			this.cancelButton = new global::System.Windows.Forms.Button();
			this.assemPathTextBox = new global::System.Windows.Forms.TextBox();
			this.browseButton = new global::System.Windows.Forms.Button();
			this.missingAssemDescripLabel = new global::System.Windows.Forms.Label();
			this.assemNameTextBox = new global::System.Windows.Forms.TextBox();
			this.selectAssemLabel = new global::System.Windows.Forms.Label();
			base.SuspendLayout();
			this.okButton.DialogResult = global::System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new global::System.Drawing.Point(213, 100);
			this.okButton.Name = "okButton";
			this.okButton.Size = new global::System.Drawing.Size(63, 23);
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new global::System.EventHandler(this.okButton_Click);
			this.cancelButton.DialogResult = global::System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new global::System.Drawing.Point(282, 100);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new global::System.Drawing.Size(62, 23);
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.assemPathTextBox.Location = new global::System.Drawing.Point(9, 74);
			this.assemPathTextBox.Name = "assemPathTextBox";
			this.assemPathTextBox.Size = new global::System.Drawing.Size(290, 20);
			this.assemPathTextBox.TabIndex = 2;
			this.browseButton.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 8.25f, global::System.Drawing.FontStyle.Bold, global::System.Drawing.GraphicsUnit.Point, 134);
			this.browseButton.Location = new global::System.Drawing.Point(305, 74);
			this.browseButton.Name = "browseButton";
			this.browseButton.Size = new global::System.Drawing.Size(39, 20);
			this.browseButton.TabIndex = 3;
			this.browseButton.Text = "&...";
			this.browseButton.UseVisualStyleBackColor = true;
			this.browseButton.Click += new global::System.EventHandler(this.browseButton_Click);
			this.missingAssemDescripLabel.AutoSize = true;
			this.missingAssemDescripLabel.Location = new global::System.Drawing.Point(6, 6);
			this.missingAssemDescripLabel.Name = "missingAssemDescripLabel";
			this.missingAssemDescripLabel.Size = new global::System.Drawing.Size(309, 13);
			this.missingAssemDescripLabel.TabIndex = 4;
			this.missingAssemDescripLabel.Text = "The following assembly name can not be resolved automatically:";
			this.assemNameTextBox.BorderStyle = global::System.Windows.Forms.BorderStyle.None;
			this.assemNameTextBox.Font = new global::System.Drawing.Font("Microsoft Sans Serif", 8.25f, global::System.Drawing.FontStyle.Bold, global::System.Drawing.GraphicsUnit.Point, 134);
			this.assemNameTextBox.ForeColor = global::System.Drawing.SystemColors.WindowText;
			this.assemNameTextBox.Location = new global::System.Drawing.Point(9, 23);
			this.assemNameTextBox.Multiline = true;
			this.assemNameTextBox.Name = "assemNameTextBox";
			this.assemNameTextBox.ReadOnly = true;
			this.assemNameTextBox.Size = new global::System.Drawing.Size(294, 28);
			this.assemNameTextBox.TabIndex = 5;
			this.assemNameTextBox.Text = "I'm a text box!\r\nI'm a text box!";
			this.selectAssemLabel.AutoSize = true;
			this.selectAssemLabel.Location = new global::System.Drawing.Point(6, 56);
			this.selectAssemLabel.Name = "selectAssemLabel";
			this.selectAssemLabel.Size = new global::System.Drawing.Size(197, 13);
			this.selectAssemLabel.TabIndex = 6;
			this.selectAssemLabel.Text = "Please select the assembly file manually:";
			base.AutoScaleDimensions = new global::System.Drawing.SizeF(96f, 96f);
			base.AutoScaleMode = global::System.Windows.Forms.AutoScaleMode.Dpi;
			base.CancelButton = this.cancelButton;
			base.ClientSize = new global::System.Drawing.Size(354, 129);
			base.Controls.Add(this.selectAssemLabel);
			base.Controls.Add(this.assemNameTextBox);
			base.Controls.Add(this.missingAssemDescripLabel);
			base.Controls.Add(this.browseButton);
			base.Controls.Add(this.assemPathTextBox);
			base.Controls.Add(this.cancelButton);
			base.Controls.Add(this.okButton);
			base.FormBorderStyle = global::System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.MaximizeBox = false;
			base.MinimizeBox = false;
			base.Name = "AssemblySelectorForm";
			base.ShowInTaskbar = false;
			base.StartPosition = global::System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Assembly File Selector";
			base.FormClosing += new global::System.Windows.Forms.FormClosingEventHandler(this.AssemblySelectorForm_FormClosing);
			base.ResumeLayout(false);
			base.PerformLayout();
		}

		// Token: 0x0400002E RID: 46
		private global::System.ComponentModel.IContainer components;

		// Token: 0x0400002F RID: 47
		private global::System.Windows.Forms.Button okButton;

		// Token: 0x04000030 RID: 48
		private global::System.Windows.Forms.Button cancelButton;

		// Token: 0x04000031 RID: 49
		private global::System.Windows.Forms.TextBox assemPathTextBox;

		// Token: 0x04000032 RID: 50
		private global::System.Windows.Forms.Button browseButton;

		// Token: 0x04000033 RID: 51
		private global::System.Windows.Forms.Label missingAssemDescripLabel;

		// Token: 0x04000034 RID: 52
		private global::System.Windows.Forms.TextBox assemNameTextBox;

		// Token: 0x04000035 RID: 53
		private global::System.Windows.Forms.Label selectAssemLabel;
	}
}
