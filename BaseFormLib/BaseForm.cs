using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BaseFormLib
{
    public class BaseForm : Window
    {
        private ListBox? chatList;
        public static readonly int standardBufferSize = 1024;
        public static readonly int minBufferSize = 1;
        public static readonly int maxBufferSize = 8192;
        private int bufferSize = standardBufferSize;
        public static readonly string ipPattern = @"^((25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])\.){3}(25[0-5]|2[0-4][0-9]|1[0-9]{2}|[1-9]?[0-9])$";
        public static readonly string serverShuttingDown = "Server is shutting down.";
        public static readonly string usernameError = "ERROR: Username already exists";
        public static readonly int minPort = 1;
        public static readonly int maxPort = 65535;

        public ListBox? ChatList
        {
            get => chatList;
            set => chatList = value;
        }

        public int BufferSize
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

        public async Task UpdateChat(string message)
        {
            if (chatList == null)
                return;

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                chatList.Items.Add(message);
                MoveChatPosition();
            });
        }

        public async Task HandleBufferClick(TextBox txtBuffer, TextBlock txtBufferError)
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
            await UpdateChat($"Buffer size updated to: {size}.");
        }
    }

}
