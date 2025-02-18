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

    public partial class BaseForm : Form
    {
        protected TcpClient? client;
        protected NetworkStream? stream;
        protected Thread? thread;
        protected ListBox? chatList;

        public BaseForm() {}

        public void SetChatList(ListBox chatList)
        {
            this.chatList = chatList;
        }

        public void MoveChatPosition()
        {
            if (chatList == null)
                return;

            chatList.TopIndex = chatList.Items.Count - 1;
            if (chatList.Items.Count > 100)
            {
                chatList.Items.RemoveAt(0);
            }
        }

        public static void EnableTextFieldEnterControl(TextBox textField, Button button)
        {
            textField.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    button.PerformClick();
                }
            };
        }

        public void UpdateChat(string message, bool isIncoming = true)
        {
            if (chatList == null)
                return;

            if (chatList.InvokeRequired)
            {
                StringDelegate del = new(UpdateChat);
                this.Invoke(del, [message]);
                return;
            }

            string formattedMessage = isIncoming ? ">> " + message : "<< " + message;
            chatList.Items.Add(formattedMessage);

            MoveChatPosition();
        }

        public void ReceiveData()
        {
            try
            {
                int bytesRead;
                byte[] lengthBuffer = new byte[4];
                StringBuilder messageBuilder = new();

                if (client == null)
                    throw new Exception("Client is null");

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
                        break;
                    

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
    }
}
