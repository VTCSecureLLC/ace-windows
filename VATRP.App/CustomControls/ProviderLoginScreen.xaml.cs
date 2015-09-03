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
using VATRP.App.Services;
using VATRP.Core.Model;

namespace VATRP.App.CustomControls
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
                MessageBox.Show("Please fill username field", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string passwd = PasswdBox.Password;
            if (string.IsNullOrEmpty(passwd))
            {
                MessageBox.Show("Please fill password field", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(HostnameBox.Text))
            {
                MessageBox.Show("Please fill SIP server address field", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrEmpty(HostPortBox.Text))
            {
                MessageBox.Show("Please fill SIP server port field", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ushort port;
            if (!ushort.TryParse(HostPortBox.Text, out port) )
            {
                MessageBox.Show("Invalid SIP server port", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var config = ServiceManager.Instance.ConfigurationService;
            if (config == null)
                return;
            App.CurrentAccount.Username = LoginBox.Text;
            App.CurrentAccount.Password = PasswdBox.Password;
            App.CurrentAccount.ProxyHostname = HostnameBox.Text;
            App.CurrentAccount.ProxyPort = port;
            App.CurrentAccount.RememberPassword = RememberPasswordBox.IsChecked ?? false;

            App.CurrentAccount.RegistrationPassword = PasswdBox.Password;
            App.CurrentAccount.RegistrationUser = LoginBox.Text;
            App.CurrentAccount.AutoLogin = AutoLoginBox.IsChecked ?? false;

            ServiceManager.Instance.AccountService.Save();
            if (ServiceManager.Instance.UpdateLinphoneConfig())
            {
                if (ServiceManager.Instance.LinphoneSipService.Start(true))
                    ServiceManager.Instance.LinphoneSipService.Register();
            }
            else
            {
                MessageBox.Show("Failed to update settings", "VATRP", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null)
                return;
            var config = ServiceManager.Instance.ConfigurationService;
            if (config == null)
                return;
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

        private void PasswdBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordHint.Visibility = string.IsNullOrEmpty(PasswdBox.Password)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }
}
