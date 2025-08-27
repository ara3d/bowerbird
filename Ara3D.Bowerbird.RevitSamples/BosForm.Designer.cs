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
            button1 = new System.Windows.Forms.Button();
            textBox1 = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(8, 11);
            label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(50, 15);
            label1.TabIndex = 0;
            label1.Text = "Last Idle";
            // 
            // textBoxIdle
            // 
            textBoxIdle.AllowDrop = true;
            textBoxIdle.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxIdle.Location = new System.Drawing.Point(12, 34);
            textBoxIdle.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            textBoxIdle.Name = "textBoxIdle";
            textBoxIdle.Size = new System.Drawing.Size(316, 23);
            textBoxIdle.TabIndex = 1;
            // 
            // textBoxId
            // 
            textBoxId.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxId.Location = new System.Drawing.Point(12, 82);
            textBoxId.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            textBoxId.Name = "textBoxId";
            textBoxId.Size = new System.Drawing.Size(316, 23);
            textBoxId.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(8, 59);
            label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(18, 15);
            label2.TabIndex = 2;
            label2.Text = "ID";
            // 
            // button1
            // 
            button1.Location = new System.Drawing.Point(12, 174);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(75, 23);
            button1.TabIndex = 4;
            button1.Text = "Resend All";
            button1.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            textBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBox1.Location = new System.Drawing.Point(12, 133);
            textBox1.Margin = new System.Windows.Forms.Padding(2);
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(316, 23);
            textBox1.TabIndex = 6;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(8, 110);
            label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(65, 15);
            label3.TabIndex = 5;
            label3.Text = "Queue Size";
            // 
            // BosForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(335, 274);
            Controls.Add(textBox1);
            Controls.Add(label3);
            Controls.Add(button1);
            Controls.Add(textBoxId);
            Controls.Add(label2);
            Controls.Add(textBoxIdle);
            Controls.Add(label1);
            Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
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
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label3;
    }
}