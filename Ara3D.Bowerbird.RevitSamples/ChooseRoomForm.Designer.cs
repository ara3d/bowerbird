namespace Ara3D.Bowerbird.RevitSamples
{
    partial class ChooseRoomForm
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
            this.comboBoxLevels = new System.Windows.Forms.ComboBox();
            this.listBoxRoomList = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // comboBoxLevels
            // 
            this.comboBoxLevels.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxLevels.FormattingEnabled = true;
            this.comboBoxLevels.Location = new System.Drawing.Point(13, 13);
            this.comboBoxLevels.Name = "comboBoxLevels";
            this.comboBoxLevels.Size = new System.Drawing.Size(265, 21);
            this.comboBoxLevels.TabIndex = 0;
            this.comboBoxLevels.SelectedIndexChanged += new System.EventHandler(this.comboBoxLevels_SelectedIndexChanged);
            // 
            // listBoxRoomList
            // 
            this.listBoxRoomList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listBoxRoomList.FormattingEnabled = true;
            this.listBoxRoomList.Location = new System.Drawing.Point(13, 41);
            this.listBoxRoomList.Name = "listBoxRoomList";
            this.listBoxRoomList.Size = new System.Drawing.Size(265, 368);
            this.listBoxRoomList.TabIndex = 1;
            this.listBoxRoomList.SelectedIndexChanged += new System.EventHandler(this.listBoxRoomList_SelectedIndexChanged);
            this.listBoxRoomList.MouseMove += new System.Windows.Forms.MouseEventHandler(this.listBoxRoomList_MouseMove);
            // 
            // ChooseRoomForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(290, 422);
            this.Controls.Add(this.listBoxRoomList);
            this.Controls.Add(this.comboBoxLevels);
            this.Name = "ChooseRoomForm";
            this.Text = "Room Chooser";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxLevels;
        private System.Windows.Forms.ListBox listBoxRoomList;
    }
}