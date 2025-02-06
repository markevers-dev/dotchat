using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace dotchat
{
    public delegate void StringDelegate(string message, bool isIncoming = true);

    public partial class Form1 : Form
    {
        private TcpClient? client;
        private NetworkStream? stream;
        private Thread? thread;

        public Form1()
        {
            InitializeComponent();

            this.txtMessage.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    btnSend.PerformClick();
                }
            };

        }

        private void moveChatPosition()
        {
            lstChat.TopIndex = lstChat.Items.Count - 1;

            if (lstChat.Items.Count > 100)
            {
                lstChat.Items.RemoveAt(0);
            }
        }

        private void UpdateChat(string message, bool isIncoming = true)
        {
            if (lstChat.InvokeRequired)
            {
                StringDelegate del = new(UpdateChat);
                this.Invoke(del, [message]);
                return;
            }

            string formattedMessage = isIncoming ? ">> " + message : "<< " + message;
            lstChat.Items.Add(formattedMessage);

            moveChatPosition();
        }

        private void ReceiveData()
        {
            try
            {
                int bytesRead;
                byte[] lengthBuffer = new byte[4];
                StringBuilder messageBuilder = new();

                stream = client.GetStream();
                this.Invoke(new StringDelegate(UpdateChat), "Connected!", true);

                while (true)
                {
                    bytesRead = stream.Read(lengthBuffer, 0, lengthBuffer.Length);
                    if (bytesRead == 0)
                        break;

                    int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

                    byte[] messageBytes = new byte[messageLength];

                    bytesRead = stream.Read(messageBytes, 0, messageLength);
                    if (bytesRead == 0)
                        break;

                    string message = Encoding.ASCII.GetString(messageBytes, 0, bytesRead);

                    if (message.ToLower() == "bye")
                    {
                        break;
                    }

                    this.Invoke(new StringDelegate(UpdateChat), message, true);
                }

                byte[] byeMessage = Encoding.ASCII.GetBytes("bye");
                stream.Write(byeMessage, 0, byeMessage.Length);

                stream.Close();
                client.Close();

                this.Invoke(new StringDelegate(UpdateChat), "Connection closed.", true);
            }
            catch (Exception ex)
            {
                this.Invoke(new StringDelegate(UpdateChat), $"Error: {ex.Message}", true);
            }
        }

        private void btnListen_Click(object sender, EventArgs e)
        {
            try
            {
                TcpListener listener = new(IPAddress.Any, 9000);
                listener.Start();

                this.lstChat.Items.Add("Listening for a client.");

                btnListen.Enabled = false;
                btnConnect.Enabled = false;
                btnSend.Enabled = false;
                txtMessage.Enabled = false;

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
                btnListen.Enabled = false;
                btnConnect.Enabled = false;

                lstChat.Items.Add("Connecting...");

                string serverIP = txtServerIP.Text;
                client = new(serverIP, 9000);

                thread = new(ReceiveData);
                thread.Start();
            }
            catch (Exception ex)
            {
                lstChat.Items.Add($"Client Connect Error: {ex.Message}");
                btnListen.Enabled = true;
                btnConnect.Enabled = true;
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string message = string.Empty;
            try
            {
                message = txtMessage.Text;

                if (string.IsNullOrEmpty(message))
                    return;

                byte[] messageBytes = Encoding.ASCII.GetBytes(message);
                byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);

                if (client != null && stream != null)
                {
                    stream.Write(lengthBytes, 0, lengthBytes.Length);
                    stream.Write(messageBytes, 0, messageBytes.Length);
                }
            }
            catch (Exception ex)
            {
                lstChat.Items.Add($"Client Send Error: {ex.Message}");
            }
            finally
            {
                lstChat.Items.Add("<< " + message);

                moveChatPosition();

                txtMessage.Clear();
                txtMessage.Focus();
            }
        }
    }
}
