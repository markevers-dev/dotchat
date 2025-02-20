using System.Net.Sockets;
using System.Net;
using System.Windows;
using BaseFormLib;
using System.Text;
using System.IO;

namespace ServerForm
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : BaseForm
    {
        // TODO: Add IP to Listen
        // TODO: Add IP/Port validation
        // TODO: Add Listening Details (Port, IP)
        // TODO: Add ConnectedClients Listbox

        private TcpListener? listener;
        private readonly Dictionary<string, TcpClient> connectedClients = [];
        private readonly string serverName = "Server";

        public MainWindow()
        {
            InitializeComponent();

            ChatList = lstChat;
            EnableTextFieldEnterControl(this.txtMessage, btnSend);
        }

        private static async Task<string> GetUsernameFromStream(Stream stream)
        {
            byte[] lengthBuffer = new byte[4];
            await stream.ReadAsync(lengthBuffer);
            int usernameLength = BitConverter.ToInt32(lengthBuffer, 0);
            byte[] usernameBuffer = new byte[usernameLength];
            await stream.ReadAsync(usernameBuffer);
            return Encoding.ASCII.GetString(usernameBuffer);
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

                    await UpdateChat($"Client attempting to connect with username: {username}...");

                    if (connectedClients.ContainsKey(username) || username == serverName)
                    {
                        byte[] errorMessage = Encoding.ASCII.GetBytes("ERROR: Username already exists");
                        await stream.WriteAsync(errorMessage);
                        client.Close();
                        lstChat.Items.Add($"Client with username '{username}' already exists. Aborting connection...");
                        return;
                    }

                    connectedClients[username] = client;
                    string userConnectedMessage = $"Client '{username}' connected! Welcome!";
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
                    await UpdateChat($"Error accepting client: {ex.Message}");
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

                if (IPAddress.TryParse(txtServerIP.Text, out IPAddress ip))
                {
                    if (ip.AddressFamily != AddressFamily.InterNetwork)
                    {
                        txtServerIPError.Visibility = Visibility.Visible;
                    }

                    if (int.TryParse(txtServerPort.Text, out int port))
                    {
                        if (port < 1 || port > 65535)
                        {
                            txtServerPortError.Visibility = Visibility.Visible;
                            return;
                        }

                        listener = new(ip, port);
                        listener.Start();

                        await UpdateChat("Listening for a client...");

                        grpStart.Visibility = Visibility.Collapsed;

                        _ = Task.Run(() => AcceptClientsAsync());

                        grpControls.Visibility = Visibility.Visible;
                        stkMessage.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        txtServerPortError.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    txtServerIPError.Visibility = Visibility.Visible;
                }
            }
            catch (SocketException ex)
            {
                await UpdateChat($"Socket Error: {ex.Message}");
                txtServerInUse.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                lstChat.Items.Add($"Server Error: {ex.Message}");
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
                    if (connectedClients.TryGetValue(client, out TcpClient? tcpClient))
                    {
                        try
                        {
                            if (tcpClient.Connected)
                            {
                                byte[] message = Encoding.ASCII.GetBytes("Server is shutting down.");
                                NetworkStream stream = tcpClient.GetStream();

                                if (stream.CanWrite)
                                {
                                    await stream.WriteAsync(BitConverter.GetBytes(message.Length), 0, 4);
                                    await stream.WriteAsync(message, 0, message.Length);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            lstChat.Items.Add($"Unexpected error with {client}: {ex.Message}");
                        }
                        finally
                        {
                            tcpClient.Close();
                        }
                    }
                    connectedClients.Remove(client);
                }

                stkMessage.Visibility = Visibility.Collapsed;
                grpControls.Visibility = Visibility.Collapsed;
                grpStart.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                lstChat.Items.Add($"Server Error: {ex.Message}");
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
                NetworkStream stream = client.GetStream();

                while (client.Connected)
                {
                    byte[] lengthBytes = new byte[4];

                    int bytesRead = await stream.ReadAsync(lengthBytes, 0, lengthBytes.Length);
                    if (bytesRead == 0)
                    {
                        throw new Exception("User disconnected.");
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
                            throw new Exception("User disconnected.");
                        }
                        totalBytesRead += chunkSize;
                    }

                    string username = FindUsernameByClient(client);
                    string message = Encoding.ASCII.GetString(buffer, 0, totalBytesRead);
                    await UpdateChat(FormatMessage(message, username, true));
                    await BroadcastMessage(message, username);
                }
            }
            catch (Exception)
            {
                string username = FindUsernameByClient(client);
                await UpdateChat($"User {username} disconnected");
                connectedClients.Remove(username);
                client.Close();
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
            await UpdateChat($"Buffer size updated to: {size}");
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
                    await stream.WriteAsync(messageBytes);
                }
                catch
                {
                    disconnectedClients.Add(kvp.Key);
                }
            }

            foreach (var client in disconnectedClients)
            {
                connectedClients.Remove(client);
                await UpdateChat($"User {client} disconnected.");
            }
        }

        private async Task HandleMessage(string message, string senderUsername)
        {
            await UpdateChat(FormatMessage(message, senderUsername, false));
            await BroadcastMessage(FormatMessage(message, senderUsername, true), senderUsername);
        }
        
    }
}