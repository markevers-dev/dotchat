using System;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace dotchat
{
    public delegate void StringDelegate(string message);

    public partial class Form1 : Form
    {
        private TcpClient client;
        private NetworkStream stream;
        private Thread thread;

        public Form1()
        {
            InitializeComponent();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void UpdateChat(string message)
        {
            if (this.lstChat.InvokeRequired)
            {
                StringDelegate del = new(UpdateChat);

                this.Invoke(del, [message]);
            }
            else
            {
                this.lstChat.Items.Add(message);
            }
        }
    }
}
