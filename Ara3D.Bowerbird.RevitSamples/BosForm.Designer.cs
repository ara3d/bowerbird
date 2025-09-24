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
            textBoxHeartbeat = new System.Windows.Forms.TextBox();
            label4 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(11, 18);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(77, 25);
            label1.TabIndex = 0;
            label1.Text = "Last Idle";
            // 
            // textBoxIdle
            // 
            textBoxIdle.AllowDrop = true;
            textBoxIdle.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxIdle.Location = new System.Drawing.Point(17, 57);
            textBoxIdle.Name = "textBoxIdle";
            textBoxIdle.Size = new System.Drawing.Size(450, 31);
            textBoxIdle.TabIndex = 1;
            // 
            // textBoxId
            // 
            textBoxId.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxId.Location = new System.Drawing.Point(17, 205);
            textBoxId.Name = "textBoxId";
            textBoxId.Size = new System.Drawing.Size(450, 31);
            textBoxId.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(11, 166);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(30, 25);
            label2.TabIndex = 2;
            label2.Text = "ID";
            // 
            // button1
            // 
            button1.Location = new System.Drawing.Point(17, 347);
            button1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(107, 38);
            button1.TabIndex = 4;
            button1.Text = "Resend All";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // textBox1
            // 
            textBox1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBox1.Location = new System.Drawing.Point(17, 290);
            textBox1.Name = "textBox1";
            textBox1.Size = new System.Drawing.Size(450, 31);
            textBox1.TabIndex = 6;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(11, 251);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(100, 25);
            label3.TabIndex = 5;
            label3.Text = "Queue Size";
            // 
            // textBoxHeartbeat
            // 
            textBoxHeartbeat.AllowDrop = true;
            textBoxHeartbeat.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            textBoxHeartbeat.Location = new System.Drawing.Point(17, 132);
            textBoxHeartbeat.Name = "textBoxHeartbeat";
            textBoxHeartbeat.Size = new System.Drawing.Size(450, 31);
            textBoxHeartbeat.TabIndex = 8;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(11, 93);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(126, 25);
            label4.TabIndex = 7;
            label4.Text = "Last Heartbeat";
            // 
            // BosForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(479, 537);
            Controls.Add(textBoxHeartbeat);
            Controls.Add(label4);
            Controls.Add(textBox1);
            Controls.Add(label3);
            Controls.Add(button1);
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
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxHeartbeat;
    }
}