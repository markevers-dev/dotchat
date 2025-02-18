using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotchat 
{
    public partial class ClientForm : BaseForm
    {
        public ClientForm()
        {
            InitializeComponent();

            if (!DesignMode)
            {
                SetChatList(lstChat);
                EnableTextFieldEnterControl(this.txtServerIP, btnConnect);
                EnableTextFieldEnterControl(this.txtMessage, btnSend);
            }
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                btnConnect.Enabled = false;

                lstChat.Items.Add("Connecting...");

                string serverIP = txtServerIP.Text;
                client = new(serverIP, 9000);

                thread = new(ReceiveData);
                thread.Start();

                btnConnect.ForeColor = SystemColors.ControlDark;
            }
            catch (Exception ex)
            {
                lstChat.Items.Add($"Client Connect Error: {ex.Message}");
                btnConnect.Enabled = true;
            }
        }

        private void BtnSend_Click(object sender, EventArgs e)
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

                MoveChatPosition();

                txtMessage.Clear();
                txtMessage.Focus();
            }
        }
    }
}
