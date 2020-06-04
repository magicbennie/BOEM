namespace BlackOpsErrorMonitor
{
    partial class frmSettingsAndLayout
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
            this.chklstModules = new System.Windows.Forms.CheckedListBox();
            this.grpModules = new System.Windows.Forms.GroupBox();
            this.btnConfigureModule = new System.Windows.Forms.Button();
            this.btnSaveExit = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.numRefreshTime = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.lblTextColour = new System.Windows.Forms.Label();
            this.lblBackColour = new System.Windows.Forms.Label();
            this.btnTextColourPick = new System.Windows.Forms.Button();
            this.btnBackColourPick = new System.Windows.Forms.Button();
            this.chkOverrideSEH = new System.Windows.Forms.CheckBox();
            this.btnHelpSEH = new System.Windows.Forms.Button();
            this.grpModules.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRefreshTime)).BeginInit();
            this.SuspendLayout();
            // 
            // chklstModules
            // 
            this.chklstModules.FormattingEnabled = true;
            this.chklstModules.Items.AddRange(new object[] {
            "LevelTime",
            "LevelTimeBar",
            "numGEntitiesAllocated",
            "numGEntitiesUsed",
            "numSnapshotEntitiesP1",
            "numSnapshotEntitiesIncreaseP1",
            "numSnapshotEntitiesP2",
            "numSnapshotEntitiesP3",
            "numSnapshotEntitiesP4",
            "nextCachedSnapshotEntities",
            "nextCachedSnapshotClients",
            "nextCachedSnapshotFrames",
            "numSnapshotClients",
            "numSnapshotActors",
            "MemoryUsage",
            "LastNetSnapEntities",
            "numSnapshotEntitiesPercentage",
            "comFrameTime"});
            this.chklstModules.Location = new System.Drawing.Point(6, 19);
            this.chklstModules.Name = "chklstModules";
            this.chklstModules.Size = new System.Drawing.Size(260, 214);
            this.chklstModules.TabIndex = 0;
            this.chklstModules.SelectedIndexChanged += new System.EventHandler(this.chklstModules_SelectedIndexChanged);
            // 
            // grpModules
            // 
            this.grpModules.Controls.Add(this.btnConfigureModule);
            this.grpModules.Controls.Add(this.chklstModules);
            this.grpModules.Location = new System.Drawing.Point(12, 12);
            this.grpModules.Name = "grpModules";
            this.grpModules.Size = new System.Drawing.Size(272, 264);
            this.grpModules.TabIndex = 1;
            this.grpModules.TabStop = false;
            this.grpModules.Text = "Modules";
            // 
            // btnConfigureModule
            // 
            this.btnConfigureModule.Enabled = false;
            this.btnConfigureModule.Location = new System.Drawing.Point(6, 235);
            this.btnConfigureModule.Name = "btnConfigureModule";
            this.btnConfigureModule.Size = new System.Drawing.Size(260, 23);
            this.btnConfigureModule.TabIndex = 1;
            this.btnConfigureModule.Text = "Configure Module";
            this.btnConfigureModule.UseVisualStyleBackColor = true;
            this.btnConfigureModule.Click += new System.EventHandler(this.btnConfigureModule_Click);
            // 
            // btnSaveExit
            // 
            this.btnSaveExit.Location = new System.Drawing.Point(155, 396);
            this.btnSaveExit.Name = "btnSaveExit";
            this.btnSaveExit.Size = new System.Drawing.Size(129, 23);
            this.btnSaveExit.TabIndex = 2;
            this.btnSaveExit.Text = "Save && Exit";
            this.btnSaveExit.UseVisualStyleBackColor = true;
            this.btnSaveExit.Click += new System.EventHandler(this.btnSaveExit_Click);
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(12, 396);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(129, 23);
            this.btnExit.TabIndex = 3;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // numRefreshTime
            // 
            this.numRefreshTime.Location = new System.Drawing.Point(91, 282);
            this.numRefreshTime.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numRefreshTime.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.numRefreshTime.Name = "numRefreshTime";
            this.numRefreshTime.Size = new System.Drawing.Size(118, 20);
            this.numRefreshTime.TabIndex = 4;
            this.numRefreshTime.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 284);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Refresh Time:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(215, 284);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "milliseconds";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 313);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Text Colour:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 341);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(68, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Back Colour:";
            // 
            // lblTextColour
            // 
            this.lblTextColour.Location = new System.Drawing.Point(91, 308);
            this.lblTextColour.Name = "lblTextColour";
            this.lblTextColour.Size = new System.Drawing.Size(103, 23);
            this.lblTextColour.TabIndex = 9;
            // 
            // lblBackColour
            // 
            this.lblBackColour.BackColor = System.Drawing.SystemColors.ControlText;
            this.lblBackColour.Location = new System.Drawing.Point(92, 336);
            this.lblBackColour.Name = "lblBackColour";
            this.lblBackColour.Size = new System.Drawing.Size(102, 23);
            this.lblBackColour.TabIndex = 10;
            // 
            // btnTextColourPick
            // 
            this.btnTextColourPick.Location = new System.Drawing.Point(200, 308);
            this.btnTextColourPick.Name = "btnTextColourPick";
            this.btnTextColourPick.Size = new System.Drawing.Size(84, 23);
            this.btnTextColourPick.TabIndex = 11;
            this.btnTextColourPick.Text = "Pick";
            this.btnTextColourPick.UseVisualStyleBackColor = true;
            this.btnTextColourPick.Click += new System.EventHandler(this.btnTextColourPick_Click);
            // 
            // btnBackColourPick
            // 
            this.btnBackColourPick.Location = new System.Drawing.Point(200, 336);
            this.btnBackColourPick.Name = "btnBackColourPick";
            this.btnBackColourPick.Size = new System.Drawing.Size(84, 23);
            this.btnBackColourPick.TabIndex = 12;
            this.btnBackColourPick.Text = "Pick";
            this.btnBackColourPick.UseVisualStyleBackColor = true;
            this.btnBackColourPick.Click += new System.EventHandler(this.btnBackColourPick_Click);
            // 
            // chkOverrideSEH
            // 
            this.chkOverrideSEH.AutoSize = true;
            this.chkOverrideSEH.Location = new System.Drawing.Point(18, 371);
            this.chkOverrideSEH.Name = "chkOverrideSEH";
            this.chkOverrideSEH.Size = new System.Drawing.Size(127, 17);
            this.chkOverrideSEH.TabIndex = 14;
            this.chkOverrideSEH.Text = "Override Games SEH";
            this.chkOverrideSEH.UseVisualStyleBackColor = true;
            // 
            // btnHelpSEH
            // 
            this.btnHelpSEH.Location = new System.Drawing.Point(243, 365);
            this.btnHelpSEH.Name = "btnHelpSEH";
            this.btnHelpSEH.Size = new System.Drawing.Size(41, 23);
            this.btnHelpSEH.TabIndex = 15;
            this.btnHelpSEH.Text = "?";
            this.btnHelpSEH.UseVisualStyleBackColor = true;
            this.btnHelpSEH.Click += new System.EventHandler(this.btnHelpSEH_Click);
            // 
            // frmSettingsAndLayout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(295, 429);
            this.Controls.Add(this.btnHelpSEH);
            this.Controls.Add(this.chkOverrideSEH);
            this.Controls.Add(this.btnBackColourPick);
            this.Controls.Add(this.btnTextColourPick);
            this.Controls.Add(this.lblBackColour);
            this.Controls.Add(this.lblTextColour);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.numRefreshTime);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnSaveExit);
            this.Controls.Add(this.grpModules);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmSettingsAndLayout";
            this.ShowIcon = false;
            this.Text = "Settings and Layout";
            this.Load += new System.EventHandler(this.frmSettingsAndLayout_Load);
            this.grpModules.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numRefreshTime)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox chklstModules;
        private System.Windows.Forms.GroupBox grpModules;
        private System.Windows.Forms.Button btnSaveExit;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.NumericUpDown numRefreshTime;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnConfigureModule;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblTextColour;
        private System.Windows.Forms.Label lblBackColour;
        private System.Windows.Forms.Button btnTextColourPick;
        private System.Windows.Forms.Button btnBackColourPick;
        private System.Windows.Forms.CheckBox chkOverrideSEH;
        private System.Windows.Forms.Button btnHelpSEH;
        //private System.Windows.Forms.CheckBox chkOverrideSEH; 
        //private System.Windows.Forms.Button btnHelpSEH; 
    }
}