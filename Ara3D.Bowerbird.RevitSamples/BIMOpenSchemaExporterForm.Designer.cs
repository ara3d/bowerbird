namespace Ara3D.Bowerbird.RevitSamples
{
    partial class BIMOpenSchemaExporterForm
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
            checkBoxExportParquet = new System.Windows.Forms.CheckBox();
            textBox1 = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            button1 = new System.Windows.Forms.Button();
            checkBoxExportDuckDB = new System.Windows.Forms.CheckBox();
            checkBoxIncludeLinks = new System.Windows.Forms.CheckBox();
            linkLabel1 = new System.Windows.Forms.LinkLabel();
            checkBox1 = new System.Windows.Forms.CheckBox();
            SuspendLayout();
            // 
            // checkBoxExportParquet
            // 
            checkBoxExportParquet.AutoSize = true;
            checkBoxExportParquet.Location = new System.Drawing.Point(12, 115);
            checkBoxExportParquet.Name = "checkBoxExportParquet";
            checkBoxExportParquet.Size = new System.Drawing.Size(154, 29);
            checkBoxExportParquet.TabIndex = 0;
            checkBoxExportParquet.Text = "Export Parquet";
            checkBoxExportParquet.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            textBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBox1.Location = new System.Drawing.Point(12, 43);
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(430, 31);
            textBox1.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 15);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(140, 25);
            label1.TabIndex = 2;
            label1.Text = "Export Directory";
            // 
            // button1
            // 
            button1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            button1.Location = new System.Drawing.Point(448, 43);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(43, 34);
            button1.TabIndex = 3;
            button1.Text = "...";
            button1.UseVisualStyleBackColor = true;
            // 
            // checkBoxExportDuckDB
            // 
            checkBoxExportDuckDB.AutoSize = true;
            checkBoxExportDuckDB.Location = new System.Drawing.Point(12, 150);
            checkBoxExportDuckDB.Name = "checkBoxExportDuckDB";
            checkBoxExportDuckDB.Size = new System.Drawing.Size(157, 29);
            checkBoxExportDuckDB.TabIndex = 4;
            checkBoxExportDuckDB.Text = "Export DuckDB";
            checkBoxExportDuckDB.UseVisualStyleBackColor = true;
            // 
            // checkBoxIncludeLinks
            // 
            checkBoxIncludeLinks.AutoSize = true;
            checkBoxIncludeLinks.Location = new System.Drawing.Point(12, 185);
            checkBoxIncludeLinks.Name = "checkBoxIncludeLinks";
            checkBoxIncludeLinks.Size = new System.Drawing.Size(247, 29);
            checkBoxIncludeLinks.TabIndex = 5;
            checkBoxIncludeLinks.Text = "Include Linked Documents";
            checkBoxIncludeLinks.UseVisualStyleBackColor = true;
            // 
            // linkLabel1
            // 
            linkLabel1.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            linkLabel1.AutoSize = true;
            linkLabel1.Location = new System.Drawing.Point(72, 235);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new System.Drawing.Size(367, 25);
            linkLabel1.TabIndex = 6;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "https://github.com/ara3d/bim-open-schema";
            linkLabel1.Click += linkLabel1_Click;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new System.Drawing.Point(12, 80);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new System.Drawing.Size(259, 29);
            checkBox1.TabIndex = 7;
            checkBox1.Text = "Use Revit input file directory";
            checkBox1.UseVisualStyleBackColor = true;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // BIMOpenSchemaExporterForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(503, 283);
            Controls.Add(checkBox1);
            Controls.Add(linkLabel1);
            Controls.Add(checkBoxIncludeLinks);
            Controls.Add(checkBoxExportDuckDB);
            Controls.Add(button1);
            Controls.Add(label1);
            Controls.Add(textBox1);
            Controls.Add(checkBoxExportParquet);
            Name = "BIMOpenSchemaExporterForm";
            Text = "Ara 3D BIM Open Schema Exporter";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxExportParquet;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox checkBoxExportDuckDB;
        private System.Windows.Forms.CheckBox checkBoxIncludeLinks;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.CheckBox checkBox1;
    }
}