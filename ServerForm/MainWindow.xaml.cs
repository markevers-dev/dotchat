using System.Net.Sockets;
using System.Net;
using System.Windows;
using BaseFormLib;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace ServerForm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : BaseForm
    {
        // TODO: Add IP/Port validation
        // TODO: Make it actually use the fkn BufferSize
        // TODO: Fix form min height

        private TcpListener? listener;
        private readonly Dictionary<string, TcpClient> connectedClients = [];
        private readonly string serverName = "Server";

        public MainWindow()
        {
            InitializeComponent();

            ChatList = lstChat;
            EnableTextFieldEnterControl(this.txtMessage, btnSend);
            EnableTextFieldEnterControl(this.txtServerPort, btnListen);
            EnableTextFieldEnterControl(this.txtBuffer, btnBuffer);
        }

        private async Task<string> GetUsernameFromStream(Stream stream)
        {
            byte[] lengthBuffer = new byte[4];
            await stream.ReadAsync(lengthBuffer);
            int usernameLength = BitConverter.ToInt32(lengthBuffer, 0);

            byte[] buffer = new byte[BufferSize];
            using MemoryStream usernameStream = new();

            int totalBytesRead = 0;
            while (totalBytesRead < usernameLength)
            {
                int chunkSize = await stream.ReadAsync(buffer, 0, Math.Min(BufferSize, usernameLength - totalBytesRead));
                if (chunkSize == 0)
                {
                    throw new Exception("User disconnected.");
                }

                usernameStream.Write(buffer, 0, chunkSize);
                totalBytesRead += chunkSize;
            }

            return Encoding.ASCII.GetString(usernameStream.ToArray());
        }

        private async Task AcceptClientsAsync()
        {
            while (listener != null)
            {
                try
                {
                    if (listener == null)
                        break;

                    TcpClient client = await listener.AcceptTcpClientAsync();
                    NetworkStream stream = client.GetStream();

                    string username = await GetUsernameFromStream(stream);

                    await UpdateChat($"User attempting to connect with username: {username}...");

                    if (connectedClients.ContainsKey(username) || username == serverName)
                    {
                        await BroadcastMessageToSingleClient(usernameError, client);

                        client.Close();
                        await UpdateChat($"User with username '{username}' already exists. Aborting connection...");
                        continue;
                    }

                    connectedClients[username] = client;
                    string userConnectedMessage = $"User {username} connected! Welcome!";
                    await BroadcastMessage(userConnectedMessage, serverName);
                    await UpdateChat(userConnectedMessage);

                    _ = Task.Run(() => ReceiveData(client));
                }
                catch (SocketException)
                {
                    await UpdateChat("Server has shut down.");
                    grpControls.Visibility = Visibility.Visible;
                    stkMessage.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    await UpdateChat("An unexpected error occurred while trying to accept a user.");
                }
            }
        }

        private async void BtnSend_Click(object sender, EventArgs e)
        {
            string message = txtMessage.Text;

            if (string.IsNullOrWhiteSpace(message))
                return;

            _ = Task.Run(() => BroadcastMessage(FormatMessage(message, serverName, true), serverName));
            await UpdateChat(FormatMessage(message, serverName, false));

            txtMessage.Text = string.Empty;
        }

        private void SetServerErrors(bool isActive)
        {
            if (isActive)
            {
                txtServerPortError.Visibility = Visibility.Visible;
                txtServerIPError.Visibility = Visibility.Visible;
                txtServerInUse.Visibility = Visibility.Visible;
            }
            else
            {
                txtServerPortError.Visibility = Visibility.Collapsed;
                txtServerIPError.Visibility = Visibility.Collapsed;
                txtServerInUse.Visibility = Visibility.Collapsed;
            }
        }

        private async void BtnListen_Click(object sender, EventArgs e)
        {
            try
            {
                SetServerErrors(false);

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

                        listener = new(serverIP, port);
                        listener.Start();

                        await UpdateChat("Listening for a client...");

                        grpStart.Visibility = Visibility.Collapsed;

                        _ = Task.Run(() => AcceptClientsAsync());

                        grpControls.Visibility = Visibility.Visible;
                        stkMessage.Visibility = Visibility.Visible;
            }
            catch (SocketException)
            {
                txtServerInUse.Visibility = Visibility.Visible;
            }
            catch (Exception)
            {
                await UpdateChat("An error occurred while trying to start up the Server. Please try again later!");
                grpControls.Visibility = Visibility.Collapsed;
                stkMessage.Visibility = Visibility.Collapsed;
                grpStart.Visibility = Visibility.Visible;
            }
        }

        private async void BtnStopListen_Click(object sender, EventArgs e)
        {
            if (listener == null)
                return;

            try
            {
                await UpdateChat("Stopping server...");

                listener.Stop();
                listener = null;

                List<string> clientsToRemove = new(connectedClients.Keys);
                foreach (string client in clientsToRemove)
                {
                    if (connectedClients.TryGetValue(client, out TcpClient? tcpClient) && tcpClient.Connected)
                    {
                        await BroadcastMessageToSingleClient(serverShuttingDown, tcpClient);
                    }
                    connectedClients.Remove(client);
                }

                stkMessage.Visibility = Visibility.Collapsed;
                grpControls.Visibility = Visibility.Collapsed;
                grpStart.Visibility = Visibility.Visible;
            }
            catch (Exception)
            {
                await UpdateChat("An error occurred while trying to stop the Server. Please try again later!");
                grpControls.Visibility = Visibility.Visible;
                stkMessage.Visibility = Visibility.Visible;
                grpStart.Visibility = Visibility.Collapsed;
            }
        }

        private string FindUsernameByClient(TcpClient client)
        {
            foreach (var kvp in connectedClients)
            {
                if (kvp.Value == client)
                {
                    return kvp.Key;
                }
            }
            return string.Empty;
        }

        private async Task ReceiveData(TcpClient client)
        {
            try
            {
                using NetworkStream stream = client.GetStream();
                byte[] lengthBytes = new byte[4];   

                while (client.Connected && listener != null)
                {
                    int bytesRead = await stream.ReadAsync(lengthBytes);
                    if (bytesRead == 0)
                    {
                        throw new Exception("User disconnected.");
                    }

                    int messageLength = BitConverter.ToInt32(lengthBytes, 0);
                    if (messageLength <= 0)
                    {
                        continue;
                    }

                    byte[] buffer = new byte[BufferSize];
                    int totalBytesRead = 0;
                    using MemoryStream messageStream = new();

                    while (totalBytesRead < messageLength)
                    {
                        int chunkSize = await stream.ReadAsync(buffer, 0, Math.Min(BufferSize, messageLength - totalBytesRead));
                        if (chunkSize == 0)
                        {
                            throw new Exception("User disconnected.");
                        }

                        messageStream.Write(buffer, 0, chunkSize);
                        totalBytesRead += chunkSize;
                    }

                    string username = FindUsernameByClient(client);
                    string message = FormatMessage(Encoding.ASCII.GetString(messageStream.ToArray()), username, true);
                    await UpdateChat(message);
                    await BroadcastMessage(message, username);
                }
            }
            catch (Exception)
            {
                string username = FindUsernameByClient(client);

                if (listener != null)
                {
                    string disconnectMessage = $"User {username} disconnected.";
                    await BroadcastMessage(disconnectMessage, serverName);
                    await UpdateChat(disconnectMessage);
                }

                connectedClients.Remove(username);
                client.Close();
            }
        }

        private async void BtnBuffer_Click(object sender, EventArgs e)
        {
           await HandleBufferClick(txtBuffer, txtBufferError);
        }

        private async Task BroadcastMessageToSingleClient(string message, TcpClient client)
        {
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);
            byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);

            try
            {
                NetworkStream stream = client.GetStream();
                await stream.WriteAsync(lengthBytes);

                int totalBytesSent = 0;
                while (totalBytesSent < messageBytes.Length)
                {
                    int chunkSize = Math.Min(BufferSize, messageBytes.Length - totalBytesSent);
                    await stream.WriteAsync(messageBytes, totalBytesSent, chunkSize);
                    totalBytesSent += chunkSize;
                }
            }
            catch
            {
                connectedClients.Remove(FindUsernameByClient(client));
            }
        }

        private async Task BroadcastMessage(string message, string senderUsername)
        {
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);
            byte[] lengthBytes = BitConverter.GetBytes(messageBytes.Length);

            List<string> disconnectedClients = [];

            foreach (var kvp in connectedClients)
            {
                if (kvp.Key == senderUsername) continue;

                try
                {
                    NetworkStream stream = kvp.Value.GetStream();
                    await stream.WriteAsync(lengthBytes);

                    int totalBytesSent = 0;
                    while (totalBytesSent < messageBytes.Length)
                    {
                        int chunkSize = Math.Min(BufferSize, messageBytes.Length - totalBytesSent);
                        await stream.WriteAsync(messageBytes, totalBytesSent, chunkSize);
                        totalBytesSent += chunkSize;
                    }
                }
                catch
                {
                    disconnectedClients.Add(kvp.Key);
                }
            }

            foreach (var client in disconnectedClients)
            {
                connectedClients.Remove(client);
            }
        }

        private void TxtServerIP_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
                txtServerPort.Focus();
        }
    }
}