namespace dotchat
{
    partial class Form1
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
            ConnectBox = new GroupBox();
            ChatserverIPLabel = new Label();
            txtServerIP = new TextBox();
            btnConnect = new Button();
            btnSend = new Button();
            txtMessage = new TextBox();
            lstChat = new ListBox();
            ConnectBox.SuspendLayout();
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
            // ConnectBox
            // 
            ConnectBox.Controls.Add(ChatserverIPLabel);
            ConnectBox.Controls.Add(txtServerIP);
            ConnectBox.Controls.Add(btnConnect);
            ConnectBox.Font = new Font("Yu Gothic UI", 9F, FontStyle.Bold);
            ConnectBox.ForeColor = SystemColors.ControlLight;
            ConnectBox.Location = new Point(606, 92);
            ConnectBox.Name = "ConnectBox";
            ConnectBox.Size = new Size(182, 124);
            ConnectBox.TabIndex = 2;
            ConnectBox.TabStop = false;
            ConnectBox.Text = "Connect to Server:";
            ConnectBox.Enter += groupBox1_Enter;
            // 
            // ChatserverIPLabel
            // 
            ChatserverIPLabel.AutoSize = true;
            ChatserverIPLabel.Location = new Point(6, 24);
            ChatserverIPLabel.Name = "ChatserverIPLabel";
            ChatserverIPLabel.Size = new Size(79, 15);
            ChatserverIPLabel.TabIndex = 2;
            ChatserverIPLabel.Text = "Chatserver IP:";
            // 
            // txtServerIP
            // 
            txtServerIP.Location = new Point(6, 42);
            txtServerIP.Name = "txtServerIP";
            txtServerIP.Size = new Size(170, 23);
            txtServerIP.TabIndex = 1;
            txtServerIP.Text = "txtServerIP";
            // 
            // btnConnect
            // 
            btnConnect.ForeColor = SystemColors.ControlText;
            btnConnect.Location = new Point(6, 84);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(170, 23);
            btnConnect.TabIndex = 0;
            btnConnect.Text = "btnConnect";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // btnSend
            // 
            btnSend.Font = new Font("Yu Gothic UI", 9F, FontStyle.Bold);
            btnSend.Location = new Point(512, 415);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(88, 23);
            btnSend.TabIndex = 3;
            btnSend.Text = "btnSend";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // txtMessage
            // 
            txtMessage.Font = new Font("Yu Gothic UI", 9F, FontStyle.Bold);
            txtMessage.Location = new Point(12, 415);
            txtMessage.Name = "txtMessage";
            txtMessage.Size = new Size(494, 23);
            txtMessage.TabIndex = 4;
            txtMessage.Text = "txtMessage";
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
            // Form1
            // 
            AccessibleName = "";
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.WindowFrame;
            ClientSize = new Size(800, 450);
            Controls.Add(lstChat);
            Controls.Add(txtMessage);
            Controls.Add(btnSend);
            Controls.Add(ConnectBox);
            Controls.Add(btnListen);
            Font = new Font("Yu Gothic UI", 9F, FontStyle.Bold);
            Name = "Form1";
            Text = "NotS - dotchat";
            Load += Form1_Load;
            ConnectBox.ResumeLayout(false);
            ConnectBox.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnListen;
        private GroupBox ConnectBox;
        private Button btnConnect;
        private Label ChatserverIPLabel;
        private TextBox txtServerIP;
        private Button btnSend;
        private TextBox txtMessage;
        private ListBox lstChat;
    }
}
