
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BaseFormLib
{
    public class BaseForm : Window
    {
        private TcpClient? client;
        private NetworkStream? stream;
        private Thread? thread;
        private ListBox? chatList;
        private int? bufferSize = 1024;

        public TcpClient? Client
        {
            get => client;
            set => client = value;
        }

        public NetworkStream? Stream
        {
            get => stream;
            set => stream = value;
        }

        public Thread? BaseThread
        {
            get => thread;
            set => thread = value;
        }

        public ListBox? ChatList
        {
            get => chatList;
            set => chatList = value;
        }

        public int? BufferSIze
        {
            get => bufferSize;
            set => bufferSize = value;
        }

        public static string FormatMessage(string message, string username, bool isIncoming = true)
        {
            string inOutSymbol = isIncoming ? " >> " : " << ";
            DateTime dateTime = DateTime.Now;
            
            return dateTime.ToString() + ": " + username + inOutSymbol + message;
        }

        public void MoveChatPosition()
        {
            if (chatList == null)
                return;

            chatList.ScrollIntoView(chatList.Items[chatList.Items.Count - 1]);

            if (chatList.Items.Count > 100)
            {
                chatList.Items.RemoveAt(0);
            }
        }

        public static void EnableTextFieldEnterControl(TextBox textField, Button button)
        {
            textField.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }
            };
        }

        public async Task UpdateChat(string message, bool isIncoming = true)
        {
            if (chatList == null)
                return;

            string formattedMessage = isIncoming ? ">> " + message : "<< " + message;

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                chatList.Items.Add(formattedMessage);
                MoveChatPosition();
            });
        }

        public async void ReceiveData()
        {
            try
            {
                if (client == null)
                    throw new Exception("Client is null");

                using var stream = client.GetStream();
                await UpdateChat("Connected!", true);

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

                    await UpdateChat(message, true);
                }

                byte[] byeMessage = Encoding.ASCII.GetBytes("bye");
                await stream.WriteAsync(byeMessage, 0, byeMessage.Length);

                client.Close();
                await UpdateChat("Connection closed.", true);
            }
            catch (Exception ex)
            {
                await UpdateChat($"Error: {ex.Message}", true);
            }
        }

    }

}
