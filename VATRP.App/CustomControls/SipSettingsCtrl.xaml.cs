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
                if (s != null && s.Text == App.CurrentAccount.Transport)
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
                return false;

            var s = TransportBox.SelectedItem as TextBlock;
            if (s != null && s.Text != App.CurrentAccount.Transport)
            {
                return true;
            }
            return true;
        }

        public bool Save()
        {
            if (App.CurrentAccount == null)
                return false;
            App.CurrentAccount.Username = LoginBox.Text;
            App.CurrentAccount.Password = PasswdBox.Password;
            App.CurrentAccount.ProxyHostname = HostnameBox.Text;
            ushort port = 0;

            ushort.TryParse(HostPortBox.Text, out port);
            App.CurrentAccount.ProxyPort = port;
            App.CurrentAccount.RegistrationUser = LoginBox.Text;
            App.CurrentAccount.RegistrationPassword = PasswdBox.Password;
            var s = TransportBox.SelectedItem as TextBlock;
            if (s != null )
            {
                App.CurrentAccount.Transport = s.Text;
            }
            return true;
        }

        #endregion
    }
}
