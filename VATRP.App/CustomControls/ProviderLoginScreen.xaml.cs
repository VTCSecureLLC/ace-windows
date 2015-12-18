using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using com.vtcsecure.ace.windows.Services;
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.CustomControls
{
    /// <summary>
    /// Interaction logic for ProviderLoginScreen.xaml
    /// </summary>
    public partial class ProviderLoginScreen : UserControl
    {

        #region Memebers
        
        private readonly MainWindow _mainWnd;
        #endregion

        public ProviderLoginScreen(MainWindow theMain)
        {
            _mainWnd = theMain;
            InitializeComponent();
        }

        public void InitializeToAccount(VATRPAccount account)
        {
            if (account != null)
            {
                LoginBox.Text = account.Username;
                this.AuthIDBox.Text = account.AuthID;
                this.HostnameBox.Text = account.ProxyHostname;
                this.HostPortBox.Text = account.ProxyPort.ToString();
                RememberPasswordBox.IsChecked = account.RememberPassword;
                AutoLoginBox.IsChecked = account.AutoLogin;
                string transport = App.CurrentAccount.Transport;
                if (string.IsNullOrWhiteSpace(transport))
                {
                    transport = "TCP";
                }
                foreach (var item in TransportComboBox.Items)
                {
                    var tb = item as TextBlock;
                    string itemString = tb.Text;
                    if (itemString.Equals(transport))
                    {
                        TransportComboBox.SelectedItem = item;
                        TextBlock selectedItem = TransportComboBox.SelectedItem as TextBlock;
                        if (selectedItem != null)
                        {
                            string test = selectedItem.Text;
                        }
                        break;
                    }
                }
            }
        }

        private void OnForgotpassword(object sender, RequestNavigateEventArgs e)
        {
            
        }

        private void OnRegister(object sender, RequestNavigateEventArgs e)
        {
            
        }

        private void LoginCmd_Click(object sender, RoutedEventArgs e)
        {

            string username = LoginBox.Text;
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Please fill username field", "ACE", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string passwd = PasswdBox.Password;
            if (string.IsNullOrEmpty(passwd))
            {
                MessageBox.Show("Please fill password field", "ACE", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(HostnameBox.Text))
            {
                MessageBox.Show("Please fill SIP server address field", "ACE", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(HostPortBox.Text))
            {
                MessageBox.Show("Please fill SIP server port field", "ACE", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ushort port;
            if (!ushort.TryParse(HostPortBox.Text, out port) )
            {
                MessageBox.Show("Invalid SIP server port", "ACE", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var account = ServiceManager.Instance.AccountService.FindAccount(LoginBox.Text, HostnameBox.Text);
            if (account != null)
            {
                App.CurrentAccount = account;
            }
            else
            {
                ServiceManager.Instance.AccountService.AddAccount(App.CurrentAccount);
            }

            App.CurrentAccount.AuthID = AuthIDBox.Text;
            App.CurrentAccount.Username = LoginBox.Text;
            App.CurrentAccount.Password = PasswdBox.Password;
            App.CurrentAccount.ProxyHostname = HostnameBox.Text;
            App.CurrentAccount.ProxyPort = port;
            App.CurrentAccount.RememberPassword = RememberPasswordBox.IsChecked ?? false;

            App.CurrentAccount.RegistrationPassword = PasswdBox.Password;
            App.CurrentAccount.RegistrationUser = LoginBox.Text;
            App.CurrentAccount.AutoLogin = AutoLoginBox.IsChecked ?? false;

            var transportText = TransportComboBox.SelectedItem as TextBlock;
            if (transportText != null)
                App.CurrentAccount.Transport = transportText.Text;

            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
    Configuration.ConfEntry.ACCOUNT_IN_USE, App.CurrentAccount.AccountID);
            ServiceManager.Instance.AccountService.Save();
            ServiceManager.Instance.RegisterNewAccount(App.CurrentAccount.AccountID);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null)
                return;
            var config = ServiceManager.Instance.ConfigurationService;
            if (config == null)
                return;
            AuthIDBox.Text = App.CurrentAccount.AuthID;
            LoginBox.Text = App.CurrentAccount.Username;
            PasswdBox.Password = App.CurrentAccount.Password;
            HostnameBox.Text = App.CurrentAccount.ProxyHostname;
            HostPortBox.Text = App.CurrentAccount.ProxyPort.ToString();
            RememberPasswordBox.IsChecked = App.CurrentAccount.RememberPassword;
            AutoLoginBox.IsChecked = App.CurrentAccount.AutoLogin;

            switch (App.CurrentAccount.AccountType)
            {
                case VATRPAccountType.VideoRelayService:
                    VatrpDefaultLabel.Content = "Select Default VRS Provider"; 
                    break;
                case VATRPAccountType.IP_Relay:
                    VatrpDefaultLabel.Content = "Select Default IP-Relay Provider";
                    break;
                case VATRPAccountType.IP_CTS:
                    VatrpDefaultLabel.Content = "Select Default IP-CTS Provider";
                    break;
            }
        }
    }
}
