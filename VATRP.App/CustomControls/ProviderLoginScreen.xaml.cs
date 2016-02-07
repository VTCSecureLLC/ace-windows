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
using com.vtcsecure.ace.windows.Utilities;
using System.Collections.ObjectModel;

namespace com.vtcsecure.ace.windows.CustomControls
{
    /// <summary>
    /// Interaction logic for ProviderLoginScreen.xaml
    /// </summary>
    public partial class ProviderLoginScreen : UserControl
    {

        #region Members
        public ObservableCollection<VATRPServiceProvider> ProviderList { get; private set; }
        private readonly MainWindow _mainWnd;
        #endregion

        public ProviderLoginScreen(MainWindow theMain)
        {
            _mainWnd = theMain;
            InitializeComponent();
            ProviderList = new ObservableCollection<VATRPServiceProvider>();
            Initialize();
            this.DataContext = this;
        }

        public void Initialize()
        {
            InitializeToProvider("ACE");
        }

        public void InitializeToProvider(string providerName)
        {
            List<VATRPServiceProvider> serviceProviderList = ServiceManager.Instance.ProviderService.GetProviderListFullInfo();
            // sort the list
            serviceProviderList.Sort((a, b) =>  a.Label.CompareTo(b.Label));
            ProviderList.Clear();
            foreach (VATRPServiceProvider provider in serviceProviderList)
            {
                if (provider.Address == "_nologo")
                    continue;
                ProviderList.Add(provider);
            }
            // VATRP1271 - TODO - add a check to ensure that this has not changed prior to doing anything further.
            VATRPServiceProvider serviceProvider = ServiceManager.Instance.ProviderService.FindProviderLooseSearch(providerName);
            if (serviceProvider != null)
            {
                ProviderComboBox.SelectedItem = serviceProvider;
                HostnameBox.Text = serviceProvider.Address;
            }

        }

        public void InitializeToAccount(VATRPAccount account)
        {
            if (account != null)
            {
                LoginBox.Text = account.Username;
                this.AuthIDBox.Text = account.AuthID;
                //this.HostnameBox.Text = account.ProxyHostname;
                InitializeToProvider(account.ProxyHostname);
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
            string userName = LoginBox.Text;
            if (string.IsNullOrWhiteSpace(userName))
            {
                MessageBox.Show("Please fill username field", "ACE", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string password = PasswdBox.Password;
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please fill password field", "ACE", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //VATRPServiceProvider provider = (VATRPServiceProvider)ProviderComboBox.SelectedItem; 
            //if (provider != null)
            //{
            //    HostnameBox.Text = provider.Address; //Don't change field value during login click
            //}
            string address = HostnameBox.Text;
            ACEConfig config = ConfigLookup.LookupConfig(address, userName, password);
            if ((config == null) || (config.configStatus == ACEConfigStatusType.LOGIN_UNAUTHORIZED))
            {
                // login failed
                MessageBox.Show("The login Failed. Please enter a valid user name and password.", "Valid Login Required");
                return;
            }
            // VATRP-1271: to do - handle ACEConfigStatusType Appropriately. For the moment (during devel & debug) show the resulting message to the user.
            if (config.configStatus != ACEConfigStatusType.LOGIN_SUCCEESSFUL)
            {
                // there was some sort of error - expose information to user for now. Once ready, log and handle. In some cases we will still want a specific message
                string message;
                switch (config.configStatus)
                {
                        // ToDo note : the text here is a little bit different for each message - enough to let the developer know what to look for
                        //   without being too much for the user. Once we have codes worked out we can use codes in our messages that will help
                        //   in customer support.
                    case ACEConfigStatusType.CONNECTION_FAILED: message = "Unable to obtain configuration information from the server.";
                        break;
                    case ACEConfigStatusType.SRV_RECORD_NOT_FOUND: message = "The SRV Record was not found.";
                        break;
                    case ACEConfigStatusType.UNABLE_TO_PARSE : message = "Unable to parse the configuration information.";
                        break;
                    default:
                        message = "An error occured while obtaining the configuration. Status Type=" + config.configStatus.ToString();
                        break;
                }
                MessageBox.Show(message, "Error Obtaining Configuration Status");
                //return;  //Continue attempting manual registration if configuration failed
                Login_old();
            }
            else { 
                // otherwise the login was valid, proceed
                // 
                if (string.IsNullOrEmpty(config.sip_auth_password) || string.IsNullOrEmpty(config.sip_auth_username))
                {
                    config.sip_auth_username = userName;
                    config.sip_auth_password = password;
                }

                var account = ServiceManager.Instance.AccountService.FindAccount(config.sip_auth_username, config.sip_register_domain);//, HostnameBox.Text);
                if (account != null)
                {
                    App.CurrentAccount = account;
                }
                else
                {
                    ServiceManager.Instance.AccountService.AddAccount(App.CurrentAccount);
                }
                // VATRP-1899: This is a quick and dirty solution for POC. It will be funational, but not the end implementation we will want.
                //  This will ultimately be set by the configuration resources from Ace Connect.
                if (config.sip_auth_username.Equals("agent_1"))
                {
                    config.user_is_agent = true;
                }
                else
                {
                    config.user_is_agent = false;
                }
                

                //Allow user override of transport 
                var transportText = TransportComboBox.SelectedItem as TextBlock;
                if (transportText != null && config.sip_register_transport.ToLower() != transportText.Text.ToLower())
                {
                    config.sip_register_transport = transportText.Text;
                }
                //Allow user override of proxy port
                ushort port;
                if (ushort.TryParse(HostPortBox.Text, out port) && config.sip_register_port != port)
                {
                    config.sip_register_port = port;
                }
                config.UpdateVATRPAccountFromACEConfig(App.CurrentAccount);
                App.CurrentAccount.AutoLogin = this.AutoLoginBox.IsChecked ?? false;
                UpdateConfigServiceFromACEConfig(config);
                ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                    Configuration.ConfEntry.ACCOUNT_IN_USE, App.CurrentAccount.AccountID);
                ServiceManager.Instance.AccountService.Save();
                ServiceManager.Instance.RegisterNewAccount(App.CurrentAccount.AccountID);
            }
        }
        private void UpdateConfigServiceFromACEConfig(ACEConfig config)
        {
            if (config != null)
            {
                ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                    Configuration.ConfEntry.USE_RTT, config.enable_rtt);
            }
        }
        private void LoginCmd_Click_old(object sender, RoutedEventArgs e)
        {
            Login_old();
        }

        private void Login_old()
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

        public void OnProviderChanged(object sender, SelectionChangedEventArgs args)
        {
            // VATRP1271 - TODO - add a check to ensure that this has not changed prior to doign anything further.
            VATRPServiceProvider provider = (VATRPServiceProvider)ProviderComboBox.SelectedItem;//ServiceManager.Instance.ProviderService.FindProvider(providerName);
            if (provider != null)
            {
                HostnameBox.Text = provider.Address;
            }
        }


    }
}
