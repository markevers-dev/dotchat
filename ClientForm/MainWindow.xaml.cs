using System.IO;
using System.Text;
using System.Threading;
using BaseFormLib;
using System.Windows;
using System.Net;

namespace ClientForm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : BaseForm
    {
        private string? username;

        public MainWindow()
        {
            InitializeComponent();

            ChatList = lstChat;
            EnableTextFieldEnterControl(this.txtUsername, btnConnect);
            EnableTextFieldEnterControl(this.txtMessage, btnSend);
        }

        private static bool IsServerIpValid(string ip)
        {
            return IPAddress.TryParse(ip, out _);
        }

        private static bool IsUsernameValid(string username)
        {
            if (username == null || username.Length > 35)
                return false;
            return true;
        }

        private void TxtServerIP_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                txtUsername.Focus();
            }
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                txtUsernameError.Visibility = Visibility.Collapsed;
                btnDisconnect.Visibility = Visibility.Collapsed;
                grpConnect.Visibility = Visibility.Visible;
                stkMessage.Visibility = Visibility.Collapsed;

                username = txtUsername.Text;
                if (!IsUsernameValid(username))
                {
                    txtUsernameError.Visibility = Visibility.Visible;
                    return;
                }

                lstChat.Items.Add("<< Connecting...");

                string serverIP = txtServerIP.Text;
                Client = new(serverIP, 9000);
                Stream = Client.GetStream();

                byte[] usernameBytes = Encoding.ASCII.GetBytes(username);
                byte[] lengthBytes = BitConverter.GetBytes(usernameBytes.Length);
                Stream.Write(lengthBytes, 0, lengthBytes.Length);
                Stream.Write(usernameBytes, 0, usernameBytes.Length);

                _ = Task.Run(() => HandleServerResponseAsync());

                grpConnect.Visibility = Visibility.Collapsed;
                btnDisconnect.Visibility = Visibility.Visible;
                stkMessage.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                lstChat.Items.Add($"Client Connect Error: {ex.Message}");
                grpConnect.Visibility = Visibility.Visible;
                btnDisconnect.Visibility = Visibility.Collapsed;
                stkMessage.Visibility = Visibility.Collapsed;
            }
        }

        private async Task HandleServerResponseAsync()
        {
            try
            {
                if (Client == null)
                    throw new Exception("Client is null");

                using var stream = Client.GetStream();
                await UpdateChat("Connected to server!", true);

                byte[] buffer = new byte[1024];

                while (true)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                    // Check for username errors
                    if (message.StartsWith("ERROR:"))
                    {
                        await UpdateChat(message, true);
                        Client.Close();
                        return;
                    }

                    await UpdateChat(">> " + message, true);
                }

                await UpdateChat("Disconnected from server.", true);
            }
            catch (Exception ex)
            {
                await UpdateChat($"Error receiving data: {ex.Message}", true);
            }
        }


        private void BtnDisconnect_Click(object sender, EventArgs e)
        {
            if (Client == null)
                return;

            Client.Close();
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

                if (Client != null && Stream != null)
                {
                    Stream.Write(lengthBytes, 0, lengthBytes.Length);
                    Stream.Write(messageBytes, 0, messageBytes.Length);
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