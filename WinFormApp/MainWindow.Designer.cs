namespace WinFormApp
{
    partial class MainWindow
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
            this.btnRun = new System.Windows.Forms.Button();
            this.cmbProcesses = new System.Windows.Forms.ComboBox();
            this.btnSettings = new System.Windows.Forms.Button();
            this.lblEtwEventCounter = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStatusEtwEventCounter = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStatusTotalEtwCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStatusFilterEtwCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblStatusEtwDelay = new System.Windows.Forms.ToolStripStatusLabel();
            this.lstItems = new System.Windows.Forms.ListView();
            this.colPid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colTimestamp = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colEventTask = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colEventText = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(395, 11);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(108, 23);
            this.btnRun.TabIndex = 0;
            this.btnRun.Text = "Start";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // cmbProcesses
            // 
            this.cmbProcesses.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbProcesses.FormattingEnabled = true;
            this.cmbProcesses.Location = new System.Drawing.Point(12, 12);
            this.cmbProcesses.Name = "cmbProcesses";
            this.cmbProcesses.Size = new System.Drawing.Size(377, 21);
            this.cmbProcesses.TabIndex = 1;
            // 
            // btnSettings
            // 
            this.btnSettings.Location = new System.Drawing.Point(509, 11);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Size = new System.Drawing.Size(131, 23);
            this.btnSettings.TabIndex = 3;
            this.btnSettings.Text = "Settings";
            this.btnSettings.UseVisualStyleBackColor = true;
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // lblEtwEventCounter
            // 
            this.lblEtwEventCounter.AutoSize = true;
            this.lblEtwEventCounter.Location = new System.Drawing.Point(9, 405);
            this.lblEtwEventCounter.Name = "lblEtwEventCounter";
            this.lblEtwEventCounter.Size = new System.Drawing.Size(0, 13);
            this.lblEtwEventCounter.TabIndex = 4;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.lblStatusEtwEventCounter,
            this.lblStatusTotalEtwCount,
            this.lblStatusFilterEtwCount,
            this.lblStatusEtwDelay});
            this.statusStrip1.Location = new System.Drawing.Point(0, 412);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(654, 22);
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // lblStatusEtwEventCounter
            // 
            this.lblStatusEtwEventCounter.Name = "lblStatusEtwEventCounter";
            this.lblStatusEtwEventCounter.Size = new System.Drawing.Size(0, 17);
            // 
            // lblStatusTotalEtwCount
            // 
            this.lblStatusTotalEtwCount.Name = "lblStatusTotalEtwCount";
            this.lblStatusTotalEtwCount.Size = new System.Drawing.Size(131, 17);
            this.lblStatusTotalEtwCount.Text = "lblStatusTotalEtwCount";
            // 
            // lblStatusFilterEtwCount
            // 
            this.lblStatusFilterEtwCount.Name = "lblStatusFilterEtwCount";
            this.lblStatusFilterEtwCount.Size = new System.Drawing.Size(130, 17);
            this.lblStatusFilterEtwCount.Text = "lblStatusFilterEtwCount";
            // 
            // lblStatusEtwDelay
            // 
            this.lblStatusEtwDelay.Name = "lblStatusEtwDelay";
            this.lblStatusEtwDelay.Size = new System.Drawing.Size(100, 17);
            this.lblStatusEtwDelay.Text = "lblStatusEtwDelay";
            // 
            // lstItems
            // 
            this.lstItems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colTimestamp,
            this.colPid,
            this.colEventTask,
            this.colEventText});
            this.lstItems.FullRowSelect = true;
            this.lstItems.Location = new System.Drawing.Point(12, 39);
            this.lstItems.Name = "lstItems";
            this.lstItems.Size = new System.Drawing.Size(628, 361);
            this.lstItems.TabIndex = 6;
            this.lstItems.UseCompatibleStateImageBehavior = false;
            this.lstItems.View = System.Windows.Forms.View.Details;
            // 
            // colPid
            // 
            this.colPid.Text = "PID";
            // 
            // colTimestamp
            // 
            this.colTimestamp.Text = "Time";
            this.colTimestamp.Width = 80;
            // 
            // colEventTask
            // 
            this.colEventTask.Text = "Task";
            this.colEventTask.Width = 180;
            // 
            // colEventText
            // 
            this.colEventText.Text = "Text";
            this.colEventText.Width = 400;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(654, 434);
            this.Controls.Add(this.lstItems);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.lblEtwEventCounter);
            this.Controls.Add(this.btnSettings);
            this.Controls.Add(this.cmbProcesses);
            this.Controls.Add(this.btnRun);
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.Text = "Realtime ETW";
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.ComboBox cmbProcesses;
        private System.Windows.Forms.Button btnSettings;
        private System.Windows.Forms.Label lblEtwEventCounter;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusEtwEventCounter;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusTotalEtwCount;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusFilterEtwCount;
        private System.Windows.Forms.ToolStripStatusLabel lblStatusEtwDelay;
        private System.Windows.Forms.ListView lstItems;
        private System.Windows.Forms.ColumnHeader colPid;
        private System.Windows.Forms.ColumnHeader colTimestamp;
        private System.Windows.Forms.ColumnHeader colEventTask;
        private System.Windows.Forms.ColumnHeader colEventText;
    }
}