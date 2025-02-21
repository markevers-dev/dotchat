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
        private string? username;
        private TcpClient? client;
        private NetworkStream? stream;
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
        public void CloseConnection()
        {
            try
            {
                isDisconnecting = true;
                stream?.Close();
                stream?.Dispose();
                client?.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client disconnect error: {ex.Message}");
            }
            finally
            {
                stream = null;
                client = null;
            }
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
                txtServerPort.Focus();
        }

        private void TxtServerPort_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                txtUsername.Focus();
        }

        private void ResetUIForConnecting()
        {
            txtUsernameError.Visibility = Visibility.Collapsed;
            grpControls.Visibility = Visibility.Collapsed;
            grpConnect.Visibility = Visibility.Visible;
            stkMessage.Visibility = Visibility.Collapsed;
            txtServerConnectError.Visibility = Visibility.Collapsed;
            txtServerPortError.Visibility = Visibility.Collapsed;
            txtServerIPError.Visibility = Visibility.Collapsed;
        }

        private async void BtnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                isDisconnecting = false;
                ResetUIForConnecting();

                username = txtUsername.Text;
                if (!IsUsernameValid(username))
                {
                    txtUsernameError.Visibility = Visibility.Visible;
                    return;
                }

                if (!IPAddress.TryParse(txtServerIP.Text, out IPAddress? serverIP) || !Regex.IsMatch(txtServerIP.Text, ipPattern))
                {
                    txtServerIPError.Visibility = Visibility.Visible;
                    return;
                }

                if (!int.TryParse(txtServerPort.Text, out int port) || port < minPort || port > maxPort)
                {
                    txtServerPortError.Visibility = Visibility.Visible;
                    return;
                }

                client = new();
                await client.ConnectAsync(serverIP.ToString(), port);
                stream = client.GetStream();

                BufferSize = standardBufferSize;
                await SendUsernameToServer();
                _ = Task.Run(() => HandleServerResponseAsync());
            }
            catch (SocketException)
            {
                ShowConnectionError();
            }
            catch (Exception)
            {
                ShowConnectionError();
            }
        }

        private void ShowConnectionError()
        {
            txtServerConnectError.Visibility = Visibility.Visible;
            grpConnect.Visibility = Visibility.Visible;
            grpControls.Visibility = Visibility.Collapsed;
            stkMessage.Visibility = Visibility.Collapsed;
        }

        private async Task SendUsernameToServer()
        {
            if (stream == null || string.IsNullOrEmpty(username))
                return;

            byte[] usernameBytes = Encoding.ASCII.GetBytes(username);
            byte[] lengthBytes = BitConverter.GetBytes(usernameBytes.Length);

            stream.Write(lengthBytes, 0, lengthBytes.Length);

            int totalBytesSent = 0;
            while (totalBytesSent < usernameBytes.Length)
            {
                int chunkSize = Math.Min(BufferSize, usernameBytes.Length - totalBytesSent);
                await stream.WriteAsync(usernameBytes, totalBytesSent, chunkSize);
                totalBytesSent += chunkSize;
            }
        }

        private async Task TurnOnConnectControls()
        {
            await Dispatcher.InvokeAsync(() =>
            {
                grpConnect.Visibility = Visibility.Visible;
                grpControls.Visibility = Visibility.Collapsed;
                stkMessage.Visibility = Visibility.Collapsed;
                txtBuffer.Text = $"{standardBufferSize}";
                BufferSize = standardBufferSize;
                txtBufferError.Visibility = Visibility.Collapsed;
            });
        }

        private async void BtnBuffer_Click(object sender, EventArgs e)
        {
            await HandleBufferClick(txtBuffer, txtBufferError);
        }

        private async Task HandleServerResponseAsync()
        {
            try
            {
                if (client == null || stream == null) return;

                while (client.Connected)
                {
                    byte[] lengthBytes = new byte[4];

                    int bytesRead = await stream.ReadAsync(lengthBytes, 0, lengthBytes.Length);
                    if (bytesRead == 0)
                        throw new Exception("Disconnected from server.");

                    int messageLength = BitConverter.ToInt32(lengthBytes, 0);
                    if (messageLength <= 0) continue;

                    byte[] buffer = new byte[BufferSize];
                    using MemoryStream messageStream = new();

                    int totalBytesRead = 0;
                    while (totalBytesRead < messageLength)
                    {
                        int chunkSize = await stream.ReadAsync(buffer, 0, Math.Min(BufferSize, messageLength - totalBytesRead));
                        if (chunkSize == 0)
                            throw new Exception("Disconnected from server.");

                        messageStream.Write(buffer, 0, chunkSize);
                        totalBytesRead += chunkSize;
                    }

                    string builtMessage = Encoding.ASCII.GetString(messageStream.ToArray());

                    if (builtMessage.StartsWith(usernameError))
                    {
                        await UpdateChat(usernameUniqueError);
                        await TurnOnConnectControls();
                        CloseConnection();
                        return;
                    }

                    if (builtMessage.StartsWith(serverShuttingDown))
                    {
                        CloseConnection();
                        await UpdateChat(builtMessage);
                        await UpdateChatOnDisconnect();
                        await TurnOnConnectControls();
                        return;
                    }

                    await UpdateChat(builtMessage);
                    await ShowChatControls();
                }
            }
            catch (Exception)
            {
                await UpdateChatOnDisconnect();
                await TurnOnConnectControls();
            }
        }

        private async Task UpdateChatOnDisconnect()
        {
            if (isDisconnecting)
                await UpdateChat("Disconnected from server.");
            else
                await UpdateChat("Server closed down uenxpectedly. You have been disconnected.");
        }

        private async Task ShowChatControls()
        {
            await Dispatcher.InvokeAsync(() =>
            {
                grpConnect.Visibility = Visibility.Collapsed;
                grpControls.Visibility = Visibility.Visible;
                stkMessage.Visibility = Visibility.Visible;
            });
        }
        private async void BtnDisconnect_Click(object sender, EventArgs e)
        {
            if (client == null || !client.Connected)
                return;

            await TurnOnConnectControls();
            CloseConnection();
        }

        private async void BtnSend_Click(object sender, EventArgs e)
        {
            try
            {
                string message = txtMessage.Text;
                if (string.IsNullOrEmpty(message)) return;

                byte[] messageBytes = Encoding.ASCII.GetBytes(message);
                byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);

                if (client != null && stream != null && !string.IsNullOrEmpty(username))
                {
                    await stream.WriteAsync(lengthBytes, 0, lengthBytes.Length);

                    int totalBytesSent = 0;
                    while (totalBytesSent < messageBytes.Length)
                    {
                        int chunkSize = Math.Min(BufferSize, messageBytes.Length - totalBytesSent);
                        await stream.WriteAsync(messageBytes, totalBytesSent, chunkSize);
                        totalBytesSent += chunkSize;
                    }

                    await UpdateChat(FormatMessage(message, username, false));
                }
            }
            catch (Exception)
            {
                await UpdateChat("An error occurred while trying to send the message. Please try again later!");
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