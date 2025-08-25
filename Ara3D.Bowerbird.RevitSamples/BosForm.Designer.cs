namespace Ara3D.Bowerbird.RevitSamples
{
    partial class BosForm
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
            label1 = new System.Windows.Forms.Label();
            textBoxIdle = new System.Windows.Forms.TextBox();
            textBoxId = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 18);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(77, 25);
            label1.TabIndex = 0;
            label1.Text = "Last Idle";
            // 
            // textBoxIdle
            // 
            textBoxIdle.AllowDrop = true;
            textBoxIdle.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxIdle.Location = new System.Drawing.Point(17, 56);
            textBoxIdle.Name = "textBoxIdle";
            textBoxIdle.Size = new System.Drawing.Size(333, 31);
            textBoxIdle.TabIndex = 1;
            // 
            // textBoxId
            // 
            textBoxId.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxId.Location = new System.Drawing.Point(17, 137);
            textBoxId.Name = "textBoxId";
            textBoxId.Size = new System.Drawing.Size(333, 31);
            textBoxId.TabIndex = 3;
            textBoxId.TextChanged += textBox2_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(12, 99);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(30, 25);
            label2.TabIndex = 2;
            label2.Text = "ID";
            // 
            // BosForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(362, 389);
            Controls.Add(textBoxId);
            Controls.Add(label2);
            Controls.Add(textBoxIdle);
            Controls.Add(label1);
            Name = "BosForm";
            Text = "BosForm";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxIdle;
        private System.Windows.Forms.TextBox textBoxId;
        private System.Windows.Forms.Label label2;
    }
}