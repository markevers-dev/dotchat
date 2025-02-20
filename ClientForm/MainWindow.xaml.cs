using System.Text;
using BaseFormLib;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.IO;

namespace ClientForm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : BaseForm
    {
        // TODO: Add connection details (Username, IP, Port)
        // TODO: Disable controls when after disconnected

        private string? username;
        private TcpClient? client;
        private static readonly string usernameUniqueError = "Username has already been taken, please try another username!";
        private bool isDisconnecting = false;

        public MainWindow()
        {
            InitializeComponent();

            ChatList = lstChat;
            EnableTextFieldEnterControl(this.txtUsername, btnConnect);
            EnableTextFieldEnterControl(this.txtMessage, btnSend);
            EnableTextFieldEnterControl(this.txtBuffer, btnBuffer);
        }

        private static bool IsUsernameValid(string username)
        {
            if (string.IsNullOrEmpty(username) || username.Length > 35)
                return false;

            return Regex.IsMatch(username, @"^[A-Za-z0-9_-]+$");
        }

        private void TxtServerIP_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                txtUsername.Focus();
            }
        }

        public async void BtnBuffer_Click(object sender, RoutedEventArgs e)
        {
            txtBufferError.Visibility = Visibility.Collapsed;

            if (!int.TryParse(txtBuffer.Text, out int size))
            {
                txtBufferError.Visibility = Visibility.Visible;
                return;
            }

            if (size < minBufferSize || size > maxBufferSize)
            {
                txtBufferError.Visibility = Visibility.Visible;
                return;
            }

            BufferSize = size;

            if (client != null)
            {
                client.ReceiveBufferSize = BufferSize;
                client.SendBufferSize = BufferSize;
            }

            await UpdateChat($"Buffer size updated to: {size}. Rec={client.ReceiveBufferSize}. Snd={client.SendBufferSize}");
        }

        private async void BtnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                isDisconnecting = false;
                txtUsernameError.Visibility = Visibility.Collapsed;
                grpControls.Visibility = Visibility.Collapsed;
                grpConnect.Visibility = Visibility.Visible;
                stkMessage.Visibility = Visibility.Collapsed;

                username = txtUsername.Text;
                if (!IsUsernameValid(username))
                {
                    txtUsernameError.Visibility = Visibility.Visible;
                    return;
                }

                if (IPAddress.TryParse(txtServerIP.Text ?? string.Empty, out IPAddress? serverIP))
                {
                    if (int.TryParse(txtServerPort.Text, out int port))
                    {
                        if (port < 1 || port > 65535)
                        {
                            await UpdateChat("Server port must be between 1 and 65535.");
                            return;
                        }

                        client = new(serverIP.ToString(), port);
                        Stream = client.GetStream();

                        byte[] usernameBytes = Encoding.ASCII.GetBytes(username);
                        byte[] lengthBytes = BitConverter.GetBytes(usernameBytes.Length);
                        Stream.Write(lengthBytes, 0, lengthBytes.Length);
                        Stream.Write(usernameBytes, 0, usernameBytes.Length);

                        _ = Task.Run(() => HandleServerResponseAsync());

                        grpConnect.Visibility = Visibility.Collapsed;
                        grpControls.Visibility = Visibility.Visible;
                        stkMessage.Visibility = Visibility.Visible;

                    }
                    else
                    {
                        await UpdateChat("Server port must be a round number.");
                        return;
                    }
                }
                else
                {
                    await UpdateChat("Server IP must be a valid IP address.");
                    return;
                }
            }
            catch (SocketException)
            {
                await UpdateChat("Server is not available. Please try another IP Address or Port");
                grpConnect.Visibility = Visibility.Visible;
                grpControls.Visibility = Visibility.Collapsed;
                stkMessage.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                lstChat.Items.Add($"Client Connect Error: {ex.Message}");
                grpConnect.Visibility = Visibility.Visible;
                grpControls.Visibility = Visibility.Collapsed;
                stkMessage.Visibility = Visibility.Collapsed;
            }
        }

        private async Task HandleServerResponseAsync()
        {
            try
            {
                if (client == null)
                    throw new Exception("Client is null");

                using var stream = client.GetStream();
                await UpdateChat("Connected to server!");

                while (client.Connected)
                {
                    byte[] lengthBytes = new byte[4];

                    int bytesRead = await stream.ReadAsync(lengthBytes, 0, lengthBytes.Length);
                    if (bytesRead == 0)
                    {
                        throw new Exception("Disconnected from server.");
                    }

                    int messageLength = BitConverter.ToInt32(lengthBytes, 0);
                    if (messageLength <= 0)
                    {
                        continue;
                    }

                    byte[] buffer = new byte[messageLength];
                    int totalBytesRead = 0;

                    while (totalBytesRead < messageLength)
                    {
                        int chunkSize = await stream.ReadAsync(buffer, totalBytesRead, messageLength - totalBytesRead);
                        if (chunkSize == 0)
                        {
                            throw new Exception("Disconnected from server.");
                        }
                        totalBytesRead += chunkSize;
                    }

                    string builtMessage = Encoding.ASCII.GetString(buffer, 0, totalBytesRead);

                    if (builtMessage.StartsWith("ERROR: Username already exists"))
                    {
                        await UpdateChat(usernameUniqueError);
                        grpConnect.Visibility = Visibility.Visible;
                        grpControls.Visibility = Visibility.Collapsed;
                        stkMessage.Visibility = Visibility.Collapsed;
                        client.Close();
                        return;
                    }

                    await UpdateChat(builtMessage);
                }
            }
            catch (IOException)
            {
                if (isDisconnecting)
                {
                    await UpdateChat("Disconnected from server.");
                }
                else
                {
                    await UpdateChat("Server uenxpectedly closed down. You have been disconnected.");
                }
                
                grpConnect.Visibility = Visibility.Visible;
                grpControls.Visibility = Visibility.Collapsed;
                stkMessage.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                await UpdateChat(ex.Message);
                grpConnect.Visibility = Visibility.Visible;
                grpControls.Visibility = Visibility.Collapsed;
                stkMessage.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnDisconnect_Click(object sender, EventArgs e)
        {
            if (client == null)
                return;

            client.Close();
            isDisconnecting = true;
            grpConnect.Visibility = Visibility.Visible;
            grpControls.Visibility = Visibility.Collapsed;
            stkMessage.Visibility = Visibility.Collapsed;
            txtBuffer.Text = "1024";
            txtBufferError.Visibility = Visibility.Collapsed;
        }

        private async void BtnSend_Click(object sender, EventArgs e)
        {
            string message = string.Empty;
            try
            {
                message = txtMessage.Text;

                if (string.IsNullOrEmpty(message))
                    return;

                byte[] messageBytes = Encoding.ASCII.GetBytes(message);
                byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);

                if (client != null && Stream != null)
                {
                    Stream.Write(lengthBytes, 0, lengthBytes.Length);
                    Stream.Write(messageBytes, 0, messageBytes.Length);
                }

                await UpdateChat(FormatMessage(message, username, false));
            }
            catch (Exception ex)
            {
                lstChat.Items.Add($"Client Send Error: {ex.Message}");
            }
            finally
            {
                MoveChatPosition();

                txtMessage.Clear();
                txtMessage.Focus();
            }
        }
    }
}