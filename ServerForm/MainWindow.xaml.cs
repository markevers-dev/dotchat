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
        private TcpListener? listener;
        private readonly Dictionary<string, TcpClient> connectedClients = [];
        private readonly string serverName = "Server";

        public MainWindow()
        {
            InitializeComponent();
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

                    byte[] lengthBuffer = new byte[4];
                    await stream.ReadAsync(lengthBuffer);
                    int usernameLength = BitConverter.ToInt32(lengthBuffer, 0);
                    byte[] usernameBuffer = new byte[usernameLength];
                    await stream.ReadAsync(usernameBuffer);
                    string username = Encoding.ASCII.GetString(usernameBuffer);

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        lstChat.Items.Add($"Client attempting to connect with username: {username}...");
                    });

                    if (connectedClients.ContainsKey(username) || username == serverName)
                    {
                        byte[] errorMessage = Encoding.ASCII.GetBytes("ERROR: Username already exists");
                        await stream.WriteAsync(errorMessage, 0, errorMessage.Length);
                        client.Close();
                        lstChat.Items.Add($"Client with username '{username}' already exists. Aborting connection...");
                        return;
                    }

                    connectedClients[username] = client;
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        lstChat.Items.Add($"Client '{username}' connected.");
                    });

                    _ = Task.Run(() => HandleClientAsync(client, username));
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        lstChat.Items.Add($"Error accepting client: {ex.Message}");
                    });
                }
            }
        }

        private async void BtnSend_Click(object sender, EventArgs e)
        {
            if (connectedClients.Count == 0)
            {
                lstChat.Items.Add("No clients connected.");
                return;
            }

            string message = txtMessage.Text;

            if (string.IsNullOrWhiteSpace(message))
                return;

            _ = Task.Run(() => BroadcastMessage(message, serverName));
            await UpdateChat($"{serverName}: {message}", false);

            txtMessage.Text = string.Empty;
        }

        private void BtnListen_Click(object sender, EventArgs e)
        {
            try
            {
                txtServerPortError.Visibility = Visibility.Collapsed;

                if (int.TryParse(txtServerPort.Text, out int port))
                {
                    if (port < 1 && port > 65535)
                    {
                        txtServerPortError.Visibility = Visibility.Visible;
                        return;
                    }

                    listener = new(IPAddress.Any, port);
                    listener.Start();

                    lstChat.Items.Add("Listening for a client...");

                    grpStart.Visibility = Visibility.Collapsed;

                    Task.Run(() => AcceptClientsAsync());

                    BaseThread = new(ReceiveData);
                    BaseThread.Start();

                    grpControls.Visibility = Visibility.Visible;
                    stkMessage.Visibility = Visibility.Visible;
                }
                else
                {
                    txtServerPortError.Visibility = Visibility.Visible;
                }
            }
            catch (SocketException ex)
            {
                lstChat.Items.Add("The chosen IP address or Port is already in use. Please try a different combination!");
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
                lstChat.Items.Add("Stopping server...");

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
                        catch (IOException ex)
                        {
                            lstChat.Items.Add($"I/O Exception for {client}: {ex.Message}");
                        }
                        catch (ObjectDisposedException)
                        {
                            lstChat.Items.Add($"Client {client} stream already closed.");
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

                grpControls.Visibility = Visibility.Collapsed;
                grpStart.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                lstChat.Items.Add($"Server Error: {ex.Message}");
                grpControls.Visibility = Visibility.Visible;
                grpStart.Visibility = Visibility.Collapsed;
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
                    await stream.WriteAsync(lengthBytes, 0, lengthBytes.Length);
                    await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
                }
                catch
                {
                    disconnectedClients.Add(kvp.Key);
                }
            }

            foreach (var client in disconnectedClients)
            {
                connectedClients.Remove(client);
                await UpdateChat($"{client} disconnected.", true);
            }
        }

        private async Task HandleClientAsync(TcpClient client, string username)
        {
            try
            {
                using NetworkStream stream = client.GetStream();
                await UpdateChat($"{username} connected!", false);

                byte[] lengthBuffer = new byte[4];

                while (true)
                {
                    int bytesRead = await stream.ReadAsync(lengthBuffer, 0, lengthBuffer.Length);
                    if (bytesRead == 0) break;

                    int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
                    byte[] messageBytes = new byte[messageLength];

                    bytesRead = await stream.ReadAsync(messageBytes, 0, messageLength);
                    if (bytesRead == 0) break;

                    string message = Encoding.ASCII.GetString(messageBytes, 0, bytesRead);
                    if (message.ToLower() == "bye") break;

                    await BroadcastMessage($"{username}: {message}", username);
                }

                connectedClients.Remove(username);
                await UpdateChat($"{username} disconnected.", true);
                client.Close();
            }
            catch (Exception ex)
            {
                await UpdateChat($"Error handling {username}: {ex.Message}", true);
            }
        }
    }
}