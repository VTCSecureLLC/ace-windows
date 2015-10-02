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
using VATRP.App.Interfaces;
using VATRP.App.Services;
using VATRP.Core.Model;

namespace VATRP.App.CustomControls
{
    /// <summary>
    /// Interaction logic for ProviderLoginScreen.xaml
    /// </summary>
    public partial class SipSettingsCtrl : ISettings
    {
        public SipSettingsCtrl()
        {
            InitializeComponent();
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
            foreach (var item in TransportBox.Items)
            {
                var s = item as TextBlock;
                if (s != null && s.Text.Contains(App.CurrentAccount.Transport))
                {
                    TransportBox.SelectedItem = item;
                    break;
                }
            }
            
        }

        #region ISettings

        public bool IsChanged()
        {
            if (App.CurrentAccount == null)
                return false;

            if (LoginBox.Text != App.CurrentAccount.Username)
                return true;
            if ( PasswdBox.Password != App.CurrentAccount.Password)
                return true;
            if (HostnameBox.Text != App.CurrentAccount.ProxyHostname)
                return true;
            ushort port = 0;
            ushort.TryParse(HostPortBox.Text, out port);

            if (port != App.CurrentAccount.ProxyPort && port != 0)
                return true;

            var s = TransportBox.SelectedItem as TextBlock;
            if (s != null && s.Text != App.CurrentAccount.Transport)
            {
                return true;
            }

            return false;
        }
        
        private string GetTransport(String s){
            if(s == "Unencrypted (TCP)"){
                return "TCP";
            }
            
            if(s == "Encrypted (TLS"){
                return "TLS";
            }
            return "TCP";
        }

        public bool Save()
        {
            if (App.CurrentAccount == null)
                return false;
            
            if (string.IsNullOrWhiteSpace(LoginBox.Text))
            {
                MessageBox.Show("Incorrect login", "ACE", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(PasswdBox.Password))
            {
                MessageBox.Show("Empty password is not allowed", "ACE", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(HostnameBox.Text))
            {
                MessageBox.Show("Incorrect SIP Server Address", "ACE", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            
            ushort port = 0;

            ushort.TryParse(HostPortBox.Text, out port);
            if (port < 1 || port > 65535)
            {
                MessageBox.Show("Incorrect SIP Server Port", "ACE", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            App.CurrentAccount.ProxyPort = port;
            App.CurrentAccount.Username = LoginBox.Text;
            App.CurrentAccount.Password = PasswdBox.Password;
            App.CurrentAccount.ProxyHostname = HostnameBox.Text;
            App.CurrentAccount.RegistrationUser = LoginBox.Text;
            App.CurrentAccount.RegistrationPassword = PasswdBox.Password;
            var s = TransportBox.SelectedItem as TextBlock;
            if (s != null )
            {
                App.CurrentAccount.Transport = GetTransport(s.Text);
            }
            return true;
        }

        #endregion
    }
}
