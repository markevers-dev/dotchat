using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BaseFormLib
{
    public class BaseForm : Window
    {
        private NetworkStream? stream;
        private ListBox? chatList;
        private int bufferSize = 1024;
        public static readonly int minBufferSize = 1;
        public static readonly int maxBufferSize = 8192;

        public NetworkStream? Stream
        {
            get => stream;
            set => stream = value;
        }

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
    }

}
