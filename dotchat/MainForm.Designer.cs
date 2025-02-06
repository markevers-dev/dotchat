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
            button1 = new Button();
            richTextBox1 = new RichTextBox();
            groupBox1 = new GroupBox();
            label1 = new Label();
            textBox2 = new TextBox();
            button2 = new Button();
            button3 = new Button();
            textBox1 = new TextBox();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Font = new Font("Yu Gothic UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            button1.Location = new Point(606, 12);
            button1.Name = "button1";
            button1.Size = new Size(182, 40);
            button1.TabIndex = 0;
            button1.Text = "btnListen";
            button1.UseVisualStyleBackColor = true;
            // 
            // richTextBox1
            // 
            richTextBox1.Font = new Font("Yu Gothic UI", 9F, FontStyle.Bold);
            richTextBox1.Location = new Point(12, 12);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(588, 388);
            richTextBox1.TabIndex = 1;
            richTextBox1.Text = "lstChat";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(textBox2);
            groupBox1.Controls.Add(button2);
            groupBox1.Font = new Font("Yu Gothic UI", 9F, FontStyle.Bold);
            groupBox1.ForeColor = SystemColors.ControlLight;
            groupBox1.Location = new Point(606, 92);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(182, 124);
            groupBox1.TabIndex = 2;
            groupBox1.TabStop = false;
            groupBox1.Text = "Connect to Server:";
            groupBox1.Enter += groupBox1_Enter;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 24);
            label1.Name = "label1";
            label1.Size = new Size(79, 15);
            label1.TabIndex = 2;
            label1.Text = "Chatserver IP:";
            // 
            // textBox2
            // 
            textBox2.Location = new Point(6, 42);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(170, 23);
            textBox2.TabIndex = 1;
            textBox2.Text = "txtServerIP";
            // 
            // button2
            // 
            button2.ForeColor = SystemColors.ControlText;
            button2.Location = new Point(6, 84);
            button2.Name = "button2";
            button2.Size = new Size(170, 23);
            button2.TabIndex = 0;
            button2.Text = "btnConnect";
            button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            button3.Font = new Font("Yu Gothic UI", 9F, FontStyle.Bold);
            button3.Location = new Point(512, 415);
            button3.Name = "button3";
            button3.Size = new Size(88, 23);
            button3.TabIndex = 3;
            button3.Text = "btnSend";
            button3.UseVisualStyleBackColor = true;
            button3.Click += button3_Click;
            // 
            // textBox1
            // 
            textBox1.Font = new Font("Yu Gothic UI", 9F, FontStyle.Bold);
            textBox1.Location = new Point(12, 415);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(494, 23);
            textBox1.TabIndex = 4;
            textBox1.Text = "txtMessage";
            // 
            // Form1
            // 
            AccessibleName = "";
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.WindowFrame;
            ClientSize = new Size(800, 450);
            Controls.Add(textBox1);
            Controls.Add(button3);
            Controls.Add(groupBox1);
            Controls.Add(richTextBox1);
            Controls.Add(button1);
            Font = new Font("Yu Gothic UI", 9F, FontStyle.Bold);
            Name = "Form1";
            Text = "NotS - dotchat";
            Load += Form1_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private RichTextBox richTextBox1;
        private GroupBox groupBox1;
        private Button button2;
        private Label label1;
        private TextBox textBox2;
        private Button button3;
        private TextBox textBox1;
    }
}
