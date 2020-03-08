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
using System.Windows.Shapes;
using System.Net;

using CoreData;
using CoreClient;

namespace MessageClient
{
    /// <summary>
    /// Логика взаимодействия для AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        public AuthWindow(MainWindow mainWindow)
        {
            MessageWindow = mainWindow;
            InitializeComponent();
            IsAuth = false;
        }

        private void AuthWindow1_Closed(object sender, EventArgs e)
        {
            if (!IsAuth)
            {
                MessageWindow.Close();
            }
        }
        private bool IsFilled()
        {
            return LoginBox.Text.Length > 0 && ServerBox.Text.Length > 0 && PortBox.Text.Length > 0;
        }
        private void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsFilled())
            {
                MessageBox.Show("Not all fields was filled");
                return;
            }
            IPAddress addr = IPAddress.Parse(ServerBox.Text);
            try
            {
                AuthForm authForm = null;
                if (SessionBox.Text.Length > 0)
                {
                    authForm = new AuthForm(LoginBox.Text, Convert.ToInt32(SessionBox.Text));
                }
                else
                {
                    authForm = new AuthForm(LoginBox.Text);
                }
                CoreClient.MessageClient messageClient = new CoreClient.MessageClient(addr, Convert.ToInt32(PortBox.Text), authForm);
                messageClient.Authorize();
                IsAuth = true;
                MessageWindow.Client = messageClient;
                MessageWindow.Visibility = Visibility.Visible;
                Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private bool IsAuth;
        private MainWindow MessageWindow;
    }
}
