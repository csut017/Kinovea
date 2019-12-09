namespace Kinovea.ScreenManager.UserInterface
{
    partial class ExportData
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dataFormat = new System.Windows.Forms.ComboBox();
            this.dataFormatLabel = new System.Windows.Forms.Label();
            this.optionsLabel = new System.Windows.Forms.GroupBox();
            this.optionOpenAfterSave = new System.Windows.Forms.CheckBox();
            this.optionIncludeComments = new System.Windows.Forms.CheckBox();
            this.optionIncludeEvents = new System.Windows.Forms.CheckBox();
            this.exportButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.optionsLabel.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataFormat
            // 
            this.dataFormat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dataFormat.FormattingEnabled = true;
            this.dataFormat.Location = new System.Drawing.Point(130, 12);
            this.dataFormat.Name = "dataFormat";
            this.dataFormat.Size = new System.Drawing.Size(257, 21);
            this.dataFormat.TabIndex = 1;
            this.dataFormat.SelectedValueChanged += new System.EventHandler(this.dataFormat_SelectedValueChanged);
            // 
            // dataFormatLabel
            // 
            this.dataFormatLabel.AutoSize = true;
            this.dataFormatLabel.Location = new System.Drawing.Point(12, 15);
            this.dataFormatLabel.Name = "dataFormatLabel";
            this.dataFormatLabel.Size = new System.Drawing.Size(112, 13);
            this.dataFormatLabel.TabIndex = 0;
            this.dataFormatLabel.Text = "dlgExportData_Format";
            // 
            // optionsLabel
            // 
            this.optionsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.optionsLabel.Controls.Add(this.optionOpenAfterSave);
            this.optionsLabel.Controls.Add(this.optionIncludeComments);
            this.optionsLabel.Controls.Add(this.optionIncludeEvents);
            this.optionsLabel.Location = new System.Drawing.Point(15, 39);
            this.optionsLabel.Name = "optionsLabel";
            this.optionsLabel.Size = new System.Drawing.Size(372, 104);
            this.optionsLabel.TabIndex = 2;
            this.optionsLabel.TabStop = false;
            this.optionsLabel.Text = "dlgExportData_Options";
            // 
            // optionOpenAfterSave
            // 
            this.optionOpenAfterSave.AutoSize = true;
            this.optionOpenAfterSave.Checked = true;
            this.optionOpenAfterSave.CheckState = System.Windows.Forms.CheckState.Checked;
            this.optionOpenAfterSave.Enabled = false;
            this.optionOpenAfterSave.Location = new System.Drawing.Point(6, 65);
            this.optionOpenAfterSave.Name = "optionOpenAfterSave";
            this.optionOpenAfterSave.Size = new System.Drawing.Size(172, 17);
            this.optionOpenAfterSave.TabIndex = 2;
            this.optionOpenAfterSave.Text = "dlgExportData_OpenAfterSave";
            this.optionOpenAfterSave.UseVisualStyleBackColor = true;
            // 
            // optionIncludeComments
            // 
            this.optionIncludeComments.AutoSize = true;
            this.optionIncludeComments.Checked = true;
            this.optionIncludeComments.CheckState = System.Windows.Forms.CheckState.Checked;
            this.optionIncludeComments.Enabled = false;
            this.optionIncludeComments.Location = new System.Drawing.Point(6, 19);
            this.optionIncludeComments.Name = "optionIncludeComments";
            this.optionIncludeComments.Size = new System.Drawing.Size(183, 17);
            this.optionIncludeComments.TabIndex = 0;
            this.optionIncludeComments.Text = "dlgExportData_IncludeComments";
            this.optionIncludeComments.UseVisualStyleBackColor = true;
            // 
            // optionIncludeEvents
            // 
            this.optionIncludeEvents.AutoSize = true;
            this.optionIncludeEvents.Checked = true;
            this.optionIncludeEvents.CheckState = System.Windows.Forms.CheckState.Checked;
            this.optionIncludeEvents.Enabled = false;
            this.optionIncludeEvents.Location = new System.Drawing.Point(6, 42);
            this.optionIncludeEvents.Name = "optionIncludeEvents";
            this.optionIncludeEvents.Size = new System.Drawing.Size(167, 17);
            this.optionIncludeEvents.TabIndex = 1;
            this.optionIncludeEvents.Text = "dlgExportData_IncludeEvents";
            this.optionIncludeEvents.UseVisualStyleBackColor = true;
            // 
            // exportButton
            // 
            this.exportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.exportButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.exportButton.Location = new System.Drawing.Point(231, 149);
            this.exportButton.Name = "exportButton";
            this.exportButton.Size = new System.Drawing.Size(75, 23);
            this.exportButton.TabIndex = 3;
            this.exportButton.Text = "dlgExportData_Export";
            this.exportButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(312, 149);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "dlgExportData_Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // ExportData
            // 
            this.AcceptButton = this.exportButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(399, 184);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.exportButton);
            this.Controls.Add(this.optionsLabel);
            this.Controls.Add(this.dataFormatLabel);
            this.Controls.Add(this.dataFormat);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExportData";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ExportData";
            this.optionsLabel.ResumeLayout(false);
            this.optionsLabel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox dataFormat;
        private System.Windows.Forms.Label dataFormatLabel;
        private System.Windows.Forms.GroupBox optionsLabel;
        private System.Windows.Forms.CheckBox optionIncludeEvents;
        private System.Windows.Forms.Button exportButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.CheckBox optionOpenAfterSave;
        private System.Windows.Forms.CheckBox optionIncludeComments;
    }
}