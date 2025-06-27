namespace Ara3D.Bowerbird.Demo
{
    partial class BowerbirdForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BowerbirdForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.listBoxCommands = new System.Windows.Forms.ListBox();
            this.contextMenuStripCommands = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.runSelectedCommandToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.listBoxFiles = new System.Windows.Forms.ListBox();
            this.contextMenuStripFiles = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openSelectedFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.richTextBoxLog = new System.Windows.Forms.RichTextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.listBoxErrors = new System.Windows.Forms.ListBox();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.listBoxTypes = new System.Windows.Forms.ListBox();
            this.tabPage7 = new System.Windows.Forms.TabPage();
            this.listBoxAssemblies = new System.Windows.Forms.ListBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.button5 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxOutputDll = new System.Windows.Forms.TextBox();
            this.checkBoxLoad = new System.Windows.Forms.CheckBox();
            this.checkBoxEmit = new System.Windows.Forms.CheckBox();
            this.checkBoxParse = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxLibraryDir = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxSourceFiles = new System.Windows.Forms.TextBox();
            this.button4 = new System.Windows.Forms.Button();
            this.RecompileButton = new System.Windows.Forms.Button();
            this.checkBoxAutoRecompile = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.tabPage5.SuspendLayout();
            this.contextMenuStripCommands.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.contextMenuStripFiles.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage6.SuspendLayout();
            this.tabPage7.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControl2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(544, 502);
            this.splitContainer1.SplitterDistance = 180;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 1;
            // 
            // tabControl2
            // 
            this.tabControl2.Controls.Add(this.tabPage5);
            this.tabControl2.Controls.Add(this.tabPage4);
            this.tabControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl2.Location = new System.Drawing.Point(0, 0);
            this.tabControl2.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(180, 502);
            this.tabControl2.TabIndex = 0;
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.listBoxCommands);
            this.tabPage5.Location = new System.Drawing.Point(4, 24);
            this.tabPage5.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage5.Size = new System.Drawing.Size(172, 474);
            this.tabPage5.TabIndex = 1;
            this.tabPage5.Text = "Commands";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // listBoxCommands
            // 
            this.listBoxCommands.ContextMenuStrip = this.contextMenuStripCommands;
            this.listBoxCommands.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxCommands.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.listBoxCommands.FormattingEnabled = true;
            this.listBoxCommands.ItemHeight = 15;
            this.listBoxCommands.Location = new System.Drawing.Point(2, 2);
            this.listBoxCommands.Margin = new System.Windows.Forms.Padding(2);
            this.listBoxCommands.Name = "listBoxCommands";
            this.listBoxCommands.Size = new System.Drawing.Size(168, 470);
            this.listBoxCommands.TabIndex = 2;
            this.listBoxCommands.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listBoxCommands_MouseDoubleClick);
            // 
            // contextMenuStripCommands
            // 
            this.contextMenuStripCommands.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runSelectedCommandToolStripMenuItem});
            this.contextMenuStripCommands.Name = "contextMenuStripCommands";
            this.contextMenuStripCommands.Size = new System.Drawing.Size(212, 26);
            this.contextMenuStripCommands.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripCommands_Opening);
            // 
            // runSelectedCommandToolStripMenuItem
            // 
            this.runSelectedCommandToolStripMenuItem.Name = "runSelectedCommandToolStripMenuItem";
            this.runSelectedCommandToolStripMenuItem.Size = new System.Drawing.Size(211, 22);
            this.runSelectedCommandToolStripMenuItem.Text = "&Run selected command ...";
            this.runSelectedCommandToolStripMenuItem.Click += new System.EventHandler(this.runSelectedCommandToolStripMenuItem_Click);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.listBoxFiles);
            this.tabPage4.Location = new System.Drawing.Point(4, 24);
            this.tabPage4.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage4.Size = new System.Drawing.Size(172, 474);
            this.tabPage4.TabIndex = 0;
            this.tabPage4.Text = "Files";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // listBoxFiles
            // 
            this.listBoxFiles.ContextMenuStrip = this.contextMenuStripFiles;
            this.listBoxFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxFiles.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.listBoxFiles.FormattingEnabled = true;
            this.listBoxFiles.HorizontalScrollbar = true;
            this.listBoxFiles.ItemHeight = 15;
            this.listBoxFiles.Location = new System.Drawing.Point(2, 2);
            this.listBoxFiles.Margin = new System.Windows.Forms.Padding(2);
            this.listBoxFiles.Name = "listBoxFiles";
            this.listBoxFiles.Size = new System.Drawing.Size(168, 470);
            this.listBoxFiles.TabIndex = 1;
            this.listBoxFiles.SelectedIndexChanged += new System.EventHandler(this.listBoxFiles_SelectedIndexChanged);
            this.listBoxFiles.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listBoxFiles_MouseDoubleClick);
            // 
            // contextMenuStripFiles
            // 
            this.contextMenuStripFiles.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openSelectedFileToolStripMenuItem});
            this.contextMenuStripFiles.Name = "contextMenuStripFiles";
            this.contextMenuStripFiles.Size = new System.Drawing.Size(181, 26);
            this.contextMenuStripFiles.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripFiles_Opening);
            // 
            // openSelectedFileToolStripMenuItem
            // 
            this.openSelectedFileToolStripMenuItem.Name = "openSelectedFileToolStripMenuItem";
            this.openSelectedFileToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.openSelectedFileToolStripMenuItem.Text = "&Open selected file ...";
            this.openSelectedFileToolStripMenuItem.Click += new System.EventHandler(this.openSelectedFileToolStripMenuItem_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage6);
            this.tabControl1.Controls.Add(this.tabPage7);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(361, 502);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.richTextBoxLog);
            this.tabPage1.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage1.Size = new System.Drawing.Size(353, 474);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Log";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // richTextBoxLog
            // 
            this.richTextBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBoxLog.Location = new System.Drawing.Point(2, 2);
            this.richTextBoxLog.Name = "richTextBoxLog";
            this.richTextBoxLog.Size = new System.Drawing.Size(349, 470);
            this.richTextBoxLog.TabIndex = 0;
            this.richTextBoxLog.Text = "";
            this.richTextBoxLog.WordWrap = false;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.listBoxErrors);
            this.tabPage2.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage2.Size = new System.Drawing.Size(353, 474);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Errors";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // listBoxErrors
            // 
            this.listBoxErrors.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxErrors.FormattingEnabled = true;
            this.listBoxErrors.HorizontalScrollbar = true;
            this.listBoxErrors.ItemHeight = 15;
            this.listBoxErrors.Location = new System.Drawing.Point(2, 2);
            this.listBoxErrors.Margin = new System.Windows.Forms.Padding(2);
            this.listBoxErrors.Name = "listBoxErrors";
            this.listBoxErrors.Size = new System.Drawing.Size(349, 470);
            this.listBoxErrors.TabIndex = 0;
            // 
            // tabPage6
            // 
            this.tabPage6.Controls.Add(this.listBoxTypes);
            this.tabPage6.Location = new System.Drawing.Point(4, 24);
            this.tabPage6.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage6.Size = new System.Drawing.Size(353, 474);
            this.tabPage6.TabIndex = 3;
            this.tabPage6.Text = "Types";
            this.tabPage6.UseVisualStyleBackColor = true;
            // 
            // listBoxTypes
            // 
            this.listBoxTypes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxTypes.FormattingEnabled = true;
            this.listBoxTypes.HorizontalScrollbar = true;
            this.listBoxTypes.ItemHeight = 15;
            this.listBoxTypes.Location = new System.Drawing.Point(2, 2);
            this.listBoxTypes.Margin = new System.Windows.Forms.Padding(2);
            this.listBoxTypes.Name = "listBoxTypes";
            this.listBoxTypes.Size = new System.Drawing.Size(349, 470);
            this.listBoxTypes.TabIndex = 1;
            // 
            // tabPage7
            // 
            this.tabPage7.Controls.Add(this.listBoxAssemblies);
            this.tabPage7.Location = new System.Drawing.Point(4, 24);
            this.tabPage7.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage7.Name = "tabPage7";
            this.tabPage7.Size = new System.Drawing.Size(353, 474);
            this.tabPage7.TabIndex = 4;
            this.tabPage7.Text = "Assemblies";
            this.tabPage7.UseVisualStyleBackColor = true;
            // 
            // listBoxAssemblies
            // 
            this.listBoxAssemblies.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxAssemblies.FormattingEnabled = true;
            this.listBoxAssemblies.HorizontalScrollbar = true;
            this.listBoxAssemblies.ItemHeight = 15;
            this.listBoxAssemblies.Location = new System.Drawing.Point(0, 0);
            this.listBoxAssemblies.Margin = new System.Windows.Forms.Padding(2);
            this.listBoxAssemblies.Name = "listBoxAssemblies";
            this.listBoxAssemblies.Size = new System.Drawing.Size(353, 474);
            this.listBoxAssemblies.TabIndex = 2;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.button5);
            this.tabPage3.Controls.Add(this.button2);
            this.tabPage3.Controls.Add(this.groupBox1);
            this.tabPage3.Controls.Add(this.label2);
            this.tabPage3.Controls.Add(this.textBoxLibraryDir);
            this.tabPage3.Controls.Add(this.label1);
            this.tabPage3.Controls.Add(this.textBoxSourceFiles);
            this.tabPage3.Controls.Add(this.button4);
            this.tabPage3.Controls.Add(this.RecompileButton);
            this.tabPage3.Controls.Add(this.checkBoxAutoRecompile);
            this.tabPage3.Controls.Add(this.button1);
            this.tabPage3.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tabPage3.Location = new System.Drawing.Point(4, 24);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage3.Size = new System.Drawing.Size(353, 474);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Settings";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            this.button5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button5.Location = new System.Drawing.Point(284, 201);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(61, 23);
            this.button5.TabIndex = 17;
            this.button5.Text = "Open ...";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.Location = new System.Drawing.Point(284, 151);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(61, 23);
            this.button2.TabIndex = 16;
            this.button2.Text = "Open ...";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.textBoxOutputDll);
            this.groupBox1.Controls.Add(this.checkBoxLoad);
            this.groupBox1.Controls.Add(this.checkBoxEmit);
            this.groupBox1.Controls.Add(this.checkBoxParse);
            this.groupBox1.Location = new System.Drawing.Point(9, 230);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(336, 151);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Status";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 95);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(68, 15);
            this.label3.TabIndex = 16;
            this.label3.Text = "Output DLL";
            // 
            // textBoxOutputDll
            // 
            this.textBoxOutputDll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOutputDll.Enabled = false;
            this.textBoxOutputDll.Location = new System.Drawing.Point(7, 113);
            this.textBoxOutputDll.Name = "textBoxOutputDll";
            this.textBoxOutputDll.Size = new System.Drawing.Size(323, 23);
            this.textBoxOutputDll.TabIndex = 15;
            // 
            // checkBoxLoad
            // 
            this.checkBoxLoad.AutoSize = true;
            this.checkBoxLoad.Location = new System.Drawing.Point(7, 73);
            this.checkBoxLoad.Name = "checkBoxLoad";
            this.checkBoxLoad.Size = new System.Drawing.Size(110, 19);
            this.checkBoxLoad.TabIndex = 2;
            this.checkBoxLoad.Text = "Load Successful";
            this.checkBoxLoad.UseVisualStyleBackColor = true;
            // 
            // checkBoxEmit
            // 
            this.checkBoxEmit.AutoSize = true;
            this.checkBoxEmit.Location = new System.Drawing.Point(7, 48);
            this.checkBoxEmit.Name = "checkBoxEmit";
            this.checkBoxEmit.Size = new System.Drawing.Size(108, 19);
            this.checkBoxEmit.TabIndex = 1;
            this.checkBoxEmit.Text = "Emit Successful";
            this.checkBoxEmit.UseVisualStyleBackColor = true;
            // 
            // checkBoxParse
            // 
            this.checkBoxParse.AutoSize = true;
            this.checkBoxParse.Location = new System.Drawing.Point(7, 23);
            this.checkBoxParse.Name = "checkBoxParse";
            this.checkBoxParse.Size = new System.Drawing.Size(112, 19);
            this.checkBoxParse.TabIndex = 0;
            this.checkBoxParse.Text = "Parse Successful";
            this.checkBoxParse.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 183);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(120, 15);
            this.label2.TabIndex = 12;
            this.label2.Text = "Library Files Directory";
            // 
            // textBoxLibraryDir
            // 
            this.textBoxLibraryDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLibraryDir.Enabled = false;
            this.textBoxLibraryDir.Location = new System.Drawing.Point(5, 201);
            this.textBoxLibraryDir.Name = "textBoxLibraryDir";
            this.textBoxLibraryDir.Size = new System.Drawing.Size(273, 23);
            this.textBoxLibraryDir.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 134);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 15);
            this.label1.TabIndex = 10;
            this.label1.Text = "Source File Directory";
            // 
            // textBoxSourceFiles
            // 
            this.textBoxSourceFiles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxSourceFiles.Enabled = false;
            this.textBoxSourceFiles.Location = new System.Drawing.Point(4, 152);
            this.textBoxSourceFiles.Name = "textBoxSourceFiles";
            this.textBoxSourceFiles.Size = new System.Drawing.Size(274, 23);
            this.textBoxSourceFiles.TabIndex = 9;
            // 
            // button4
            // 
            this.button4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button4.Location = new System.Drawing.Point(5, 5);
            this.button4.Margin = new System.Windows.Forms.Padding(2);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(344, 30);
            this.button4.TabIndex = 8;
            this.button4.Text = "Bowerbird Documentation ...";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.aboutBowerbirdButtonClick);
            // 
            // button3
            // 
            this.RecompileButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RecompileButton.Location = new System.Drawing.Point(4, 98);
            this.RecompileButton.Margin = new System.Windows.Forms.Padding(2);
            this.RecompileButton.Name = "RecompileButton";
            this.RecompileButton.Size = new System.Drawing.Size(344, 30);
            this.RecompileButton.TabIndex = 7;
            this.RecompileButton.Text = "Recompile now";
            this.RecompileButton.UseVisualStyleBackColor = true;
            this.RecompileButton.Click += new System.EventHandler(this.RecompileButton_Click);
            // 
            // checkBoxAutoRecompile
            // 
            this.checkBoxAutoRecompile.AutoSize = true;
            this.checkBoxAutoRecompile.Location = new System.Drawing.Point(5, 75);
            this.checkBoxAutoRecompile.Margin = new System.Windows.Forms.Padding(2);
            this.checkBoxAutoRecompile.Name = "checkBoxAutoRecompile";
            this.checkBoxAutoRecompile.Size = new System.Drawing.Size(110, 19);
            this.checkBoxAutoRecompile.TabIndex = 6;
            this.checkBoxAutoRecompile.Text = "Auto-recompile";
            this.checkBoxAutoRecompile.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(5, 39);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(344, 30);
            this.button1.TabIndex = 4;
            this.button1.Text = "Clear log";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.clearLogButonClick);
            // 
            // BowerbirdForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(544, 502);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "BowerbirdForm";
            this.Text = "Bowerbird - BETA";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControl2.ResumeLayout(false);
            this.tabPage5.ResumeLayout(false);
            this.contextMenuStripCommands.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.contextMenuStripFiles.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage6.ResumeLayout(false);
            this.tabPage7.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ListBox listBoxErrors;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.ListBox listBoxFiles;
        private System.Windows.Forms.ListBox listBoxCommands;
        private System.Windows.Forms.TabPage tabPage6;
        private System.Windows.Forms.ListBox listBoxTypes;
        private System.Windows.Forms.TabPage tabPage7;
        private System.Windows.Forms.ListBox listBoxAssemblies;
        private System.Windows.Forms.RichTextBox richTextBoxLog;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxOutputDll;
        private System.Windows.Forms.CheckBox checkBoxLoad;
        private System.Windows.Forms.CheckBox checkBoxEmit;
        private System.Windows.Forms.CheckBox checkBoxParse;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxLibraryDir;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxSourceFiles;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button RecompileButton;
        private System.Windows.Forms.CheckBox checkBoxAutoRecompile;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripCommands;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripFiles;
        private System.Windows.Forms.ToolStripMenuItem runSelectedCommandToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openSelectedFileToolStripMenuItem;
    }
}

