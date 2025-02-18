namespace dotchat
{
    partial class ServerForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnListen = new Button();
            lstChat = new ListBox();
            SuspendLayout();
            // 
            // btnListen
            // 
            btnListen.Font = new Font("Yu Gothic UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnListen.Location = new Point(606, 12);
            btnListen.Name = "btnListen";
            btnListen.Size = new Size(182, 40);
            btnListen.TabIndex = 0;
            btnListen.Text = "btnListen";
            btnListen.UseVisualStyleBackColor = true;
            btnListen.Click += btnListen_Click;
            // 
            // lstChat
            // 
            lstChat.FormattingEnabled = true;
            lstChat.ItemHeight = 15;
            lstChat.Location = new Point(12, 12);
            lstChat.Name = "lstChat";
            lstChat.Size = new Size(588, 394);
            lstChat.TabIndex = 5;
            // 
            // ServerForm
            // 
            AccessibleName = "";
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.WindowFrame;
            ClientSize = new Size(800, 450);
            Controls.Add(lstChat);
            Controls.Add(btnListen);
            Font = new Font("Yu Gothic UI", 9F, FontStyle.Bold);
            Name = "ServerForm";
            Text = "NotS - dotchat - Server";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnListen;
        private ListBox lstChat;
    }
}
