namespace UsbClonerGUI
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.groupBoxDisks = new System.Windows.Forms.GroupBox();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.comboTarget = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboSource = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBoxActions = new System.Windows.Forms.GroupBox();
            this.chkDarkMode = new System.Windows.Forms.CheckBox();
            this.labelStatus = new System.Windows.Forms.Label();
            this.btnClone = new System.Windows.Forms.Button();
            this.chkConfirm = new System.Windows.Forms.CheckBox();
            this.progressBarClone = new System.Windows.Forms.ProgressBar();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBoxDisks.SuspendLayout();
            this.groupBoxActions.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxDisks
            // 
            this.groupBoxDisks.Controls.Add(this.btnRefresh);
            this.groupBoxDisks.Controls.Add(this.comboTarget);
            this.groupBoxDisks.Controls.Add(this.label2);
            this.groupBoxDisks.Controls.Add(this.comboSource);
            this.groupBoxDisks.Controls.Add(this.label1);
            this.groupBoxDisks.Location = new System.Drawing.Point(0, 80);
            this.groupBoxDisks.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBoxDisks.Name = "groupBoxDisks";
            this.groupBoxDisks.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBoxDisks.Size = new System.Drawing.Size(933, 138);
            this.groupBoxDisks.TabIndex = 0;
            this.groupBoxDisks.TabStop = false;
            this.groupBoxDisks.Text = "USB Disks";
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(712, 74);
            this.btnRefresh.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(132, 27);
            this.btnRefresh.TabIndex = 4;
            this.btnRefresh.Text = "Refresh Disks";
            this.btnRefresh.UseVisualStyleBackColor = true;
            // 
            // comboTarget
            // 
            this.comboTarget.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboTarget.DropDownWidth = 206;
            this.comboTarget.FormattingEnabled = true;
            this.comboTarget.Location = new System.Drawing.Point(414, 74);
            this.comboTarget.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboTarget.Name = "comboTarget";
            this.comboTarget.Size = new System.Drawing.Size(236, 23);
            this.comboTarget.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(411, 37);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Target (clone TO):";
            // 
            // comboSource
            // 
            this.comboSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboSource.FormattingEnabled = true;
            this.comboSource.Location = new System.Drawing.Point(105, 74);
            this.comboSource.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboSource.Name = "comboSource";
            this.comboSource.Size = new System.Drawing.Size(240, 23);
            this.comboSource.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 38);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(122, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Source (clone FROM):";
            // 
            // groupBoxActions
            // 
            this.groupBoxActions.Controls.Add(this.labelStatus);
            this.groupBoxActions.Controls.Add(this.btnClone);
            this.groupBoxActions.Controls.Add(this.chkConfirm);
            this.groupBoxActions.Location = new System.Drawing.Point(0, 226);
            this.groupBoxActions.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBoxActions.Name = "groupBoxActions";
            this.groupBoxActions.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBoxActions.Size = new System.Drawing.Size(933, 115);
            this.groupBoxActions.TabIndex = 1;
            this.groupBoxActions.TabStop = false;
            this.groupBoxActions.Text = "Actions";
            // 
            // chkDarkMode
            // 
            this.chkDarkMode.AutoSize = true;
            this.chkDarkMode.Location = new System.Drawing.Point(839, 27);
            this.chkDarkMode.Name = "chkDarkMode";
            this.chkDarkMode.Size = new System.Drawing.Size(84, 19);
            this.chkDarkMode.TabIndex = 3;
            this.chkDarkMode.Text = "Dark Mode";
            this.chkDarkMode.UseVisualStyleBackColor = true;
            this.chkDarkMode.CheckedChanged += new System.EventHandler(this.chkDarkMode_CheckedChanged);
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(19, 74);
            this.labelStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(64, 15);
            this.labelStatus.TabIndex = 2;
            this.labelStatus.Text = "Status: Idle";
            // 
            // btnClone
            // 
            this.btnClone.Location = new System.Drawing.Point(505, 18);
            this.btnClone.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnClone.Name = "btnClone";
            this.btnClone.Size = new System.Drawing.Size(88, 27);
            this.btnClone.TabIndex = 1;
            this.btnClone.Text = "Start Clone";
            this.btnClone.UseVisualStyleBackColor = true;
            // 
            // chkConfirm
            // 
            this.chkConfirm.AutoSize = true;
            this.chkConfirm.Location = new System.Drawing.Point(15, 23);
            this.chkConfirm.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.chkConfirm.Name = "chkConfirm";
            this.chkConfirm.Size = new System.Drawing.Size(320, 19);
            this.chkConfirm.TabIndex = 0;
            this.chkConfirm.Text = "I understand this will ERASE ALL DATA on the target disk";
            this.chkConfirm.UseVisualStyleBackColor = true;
            // 
            // progressBarClone
            // 
            this.progressBarClone.Location = new System.Drawing.Point(15, 350);
            this.progressBarClone.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.progressBarClone.Name = "progressBarClone";
            this.progressBarClone.Size = new System.Drawing.Size(904, 27);
            this.progressBarClone.TabIndex = 2;
            // 
            // txtLog
            // 
            this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLog.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtLog.Location = new System.Drawing.Point(15, 397);
            this.txtLog.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(908, 140);
            this.txtLog.TabIndex = 3;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(11, 29);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(180, 21);
            this.lblTitle.TabIndex = 4;
            this.lblTitle.Text = "USB Build Stick Cloner";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.Gray;
            this.label3.Location = new System.Drawing.Point(18, 54);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(280, 15);
            this.label3.TabIndex = 5;
            this.label3.Text = "Clone build USB sticks (BOOT + DATA) to new drives";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(937, 24);
            this.menuStrip1.TabIndex = 6;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(937, 549);
            this.Controls.Add(this.chkDarkMode);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.progressBarClone);
            this.Controls.Add(this.groupBoxActions);
            this.Controls.Add(this.groupBoxDisks);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "Form1";
            this.Text = "USB Build Stick Cloner";
            this.groupBoxDisks.ResumeLayout(false);
            this.groupBoxDisks.PerformLayout();
            this.groupBoxActions.ResumeLayout(false);
            this.groupBoxActions.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxDisks;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.ComboBox comboTarget;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboSource;
        private System.Windows.Forms.GroupBox groupBoxActions;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Button btnClone;
        private System.Windows.Forms.CheckBox chkConfirm;
        private System.Windows.Forms.CheckBox chkDarkMode;
        private System.Windows.Forms.ProgressBar progressBarClone;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
    }
}

