using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace dotchat
{
    public delegate void StringDelegate(string message);

    public partial class Form1 : Form
    {
        private TcpClient? client;
        private NetworkStream? stream;
        private Thread? thread;

        public Form1()
        {
            InitializeComponent();
        }

        private void UpdateChat(string message)
        {
            if (lstChat.InvokeRequired)
            {
                StringDelegate del = new(UpdateChat);
                this.Invoke(del, [message]);
                return;
            }

            lstChat.Items.Add(message);
        }

        private void ReceiveData()
        {
            try
            {
                int bytesRead;
                string message = string.Empty;

                byte[] buffer = new byte[1024];

                stream = client.GetStream();

                this.Invoke(new StringDelegate(UpdateChat), "Connected!");

                while (true)
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);

                    message = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                    if (message.ToLower() == "bye")
                    {
                        break;
                    }

                    this.Invoke(new StringDelegate(UpdateChat), message);
                }

                byte[] byeMessage = Encoding.ASCII.GetBytes("bye");

                stream.Write(byeMessage, 0, byeMessage.Length);

                stream.Close();
                client.Close();

                this.Invoke(new StringDelegate(UpdateChat), "Connection closed.");
            }
            catch (Exception ex)
            {
                this.Invoke(new StringDelegate(UpdateChat), $"Error: {ex.Message}");
            }
        }

        private void btnListen_Click(object sender, EventArgs e)
        {
            try
            {
                TcpListener listener = new(IPAddress.Any, 9000);
                listener.Start();

                this.lstChat.Items.Add("Listening for a client.");

                client = listener.AcceptTcpClient();
                this.lstChat.Items.Add("Connecting...");

                thread = new(ReceiveData);
                thread.Start();
            }
            catch (Exception ex)
            {
                lstChat.Items.Add($"Server Error: {ex.Message}");
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                lstChat.Items.Add("Connecting...");

                string serverIP = txtServerIP.Text;
                client = new(serverIP, 9000);

                thread = new(ReceiveData);
                thread.Start();
            }
            catch (Exception ex)
            {
                lstChat.Items.Add($"Client Connect Error: {ex.Message}");
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] buffer = new byte[1024];

                string message = txtMessage.Text;
                buffer = Encoding.ASCII.GetBytes(message);

                if (client != null && stream != null)
                {
                    stream.Write(buffer, 0, buffer.Length);
                }

                lstChat.Items.Add("You: " + message);

                txtMessage.Clear();
                txtMessage.Focus();
            }
            catch (Exception ex)
            {
                lstChat.Items.Add($"Client Send Error: {ex.Message}");
            }
        }
    }
}
