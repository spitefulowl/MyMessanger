using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

using CoreClient;
using CoreData;

namespace MessageClient
{
    class User
    {
        public string Name { get; set; }
    }
    class MessageWrap
    {
        public MessageWrap() { }
        public MessageWrap(MessageWrap wrap)
        {
            From = wrap.From;
            Date = wrap.Date;
            Message = wrap.Message;
        }
        public string From { get; set; }
        public string Date { get; set; }
        public string Message { get; set; }
    }
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainMessageWindow.Visibility = Visibility.Hidden;
            AuthWindow auth = new AuthWindow(this);
            auth.ShowDialog();
            if (Client == null)
            {
                Close();
            }
            else
            {
                SessionBox.Text = Client.GetSession().ToString();
                Dialogs = new Dictionary<string, List<MessageWrap>>();
                Client.StartReceiving();
                Task.Run(() => StartHandling());
            }
        }
        private void StartHandling()
        {
            while (true)
            {
                Thread.Sleep(1500);
                while(Client.Items.Count > 0)
                {
                    Data MyData = null;
                    if (Client.Items.TryDequeue(out MyData))
                    {
                        if (!Dialogs.ContainsKey(MyData.Name))
                        {
                            Dialogs[MyData.Name] = new List<MessageWrap>();
                            UserList.Dispatcher.BeginInvoke(new Action(delegate ()
                            {
                                UserList.Items.Add(new User() { Name = MyData.Name });
                            }));
                        }
                        Dialogs[MyData.Name].Add(new MessageWrap() { From = MyData.Name, Date = DateTime.Now.ToString(), Message = MyData.Message });
                        MessageGrid.Dispatcher.BeginInvoke(new Action(delegate ()
                        {
							if (SelectedTarget == MyData.Name)
					            MessageGrid.Items.Add(new MessageWrap() { From = MyData.Name, Date = DateTime.Now.ToString(), Message = MyData.Message });
                        }));
                    }
                }
            }
        }

        private void MessageGrid_BeginningEdit_1(object sender, DataGridBeginningEditEventArgs e)
        {
            e.Cancel = true;
        }

        private void MyMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (Client != null)
            {
                if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    Data packedData = new Data() { Name = Client.GetUsername(), Message = MyMessage.Text, Target = SelectedTarget };
                    MessageWrap wrap = new MessageWrap() { From = "Me", Date = DateTime.Now.ToString(), Message = MyMessage.Text };
                    MessageGrid.Items.Add(wrap);
                    Dialogs[SelectedTarget].Add(wrap);
                    Client.SendMessage(packedData);
                    MyMessage.Clear();
                    e.Handled = true;
                }
                if (!e.Handled && e.Key == Key.Enter)
                {
                    MyMessage.AppendText("\n");
                    MyMessage.CaretIndex = MyMessage.Text.Length;
                }
            }
        }
        private void UpdateMessageGrid(string target)
        {
            List<MessageWrap> messageWraps = Dialogs[target];
            foreach(MessageWrap message in messageWraps)
            {
                MessageGrid.Items.Add(new MessageWrap(message));
            }
        }
        private void UpdateTarget(string target)
        {
            SelectedTarget = target;
            TargetBox.Text = target;
            MessageGrid.Items.Clear();
            if (Dialogs.ContainsKey(target))
            {
                UpdateMessageGrid(target);
            }
            else
            {
                Dialogs[target] = new List<MessageWrap>();
                UserList.Items.Add(new User() { Name = target });
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UpdateTarget(TargetUser.Text);
            TargetUser.Clear();
        }
        public CoreClient.MessageClient Client = null;
        private string SelectedTarget = string.Empty;
        private Dictionary<string, List<MessageWrap>> Dialogs = null;

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string selected_name = ((sender as ListViewItem).Content as User).Name;
            UpdateTarget(selected_name);
        }
    }
}
