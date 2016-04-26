using com.vtcsecure.ace.windows.Services;
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

namespace com.vtcsecure.ace.windows.CustomControls.UnifiedSettings
{
    /// <summary>
    /// Interaction logic for AccountSettings.xaml
    /// </summary>
    public partial class AccountSettings : BaseUnifiedSettingsPanel
    {
        public event UnifiedSettings_EnableSettings ShowSettingsUpdate;

        public AccountSettings()
        {
            InitializeComponent();
            this.Loaded += AccountSettings_Loaded;
        }

        void AccountSettings_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        public override void Initialize()
        {
            base.Initialize();
            if (App.CurrentAccount != null)
            {
                UserIdTextBox.Text = App.CurrentAccount.AuthID;
                UserNameTextBox.Text = App.CurrentAccount.Username;
                PasswordTextBox.Password = App.CurrentAccount.Password;
                DomainTextBox.Text = App.CurrentAccount.ProxyHostname;
                ProxyTextBox.Text = Convert.ToString(App.CurrentAccount.ProxyPort);
                if (App.CurrentAccount.UseOutboundProxy && string.IsNullOrEmpty(OutboundProxyTextBox.Text))
                    OutboundProxyTextBox.Text = App.CurrentAccount.ProxyHostname;

                OutboundProxyTextBox.Text = App.CurrentAccount.OutboundProxyAddress;
                string transport = App.CurrentAccount.Transport;
                CardDAVServerTextBox.Text = App.CurrentAccount.CardDavServerPath;
                CardDAVRealmTextBox.Text = App.CurrentAccount.CardDavRealm;

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
                        string test = selectedItem.Text;
                        break;
                    }
                }
                VideoMailUriTextBox.Text = App.CurrentAccount.VideoMailUri;
            }
        }

        #region SettingsLevel
        public override void ShowDebugOptions(bool show)
        {
            System.Windows.Visibility visibleSetting = System.Windows.Visibility.Collapsed;
            if (show)
            {
                visibleSetting = System.Windows.Visibility.Visible;
            }
            ClearSettingsButton.Visibility = visibleSetting;
        }

        public override void ShowAdvancedOptions(bool show)
        {
            System.Windows.Visibility visibleSetting = System.Windows.Visibility.Collapsed;
            if (show)
            {
                visibleSetting = System.Windows.Visibility.Visible;
            }
            UserIdLabel.Visibility = visibleSetting;
            UserIdTextBox.Visibility = visibleSetting;

            ProxyLabel.Visibility = visibleSetting;
            ProxyTextBox.Visibility = visibleSetting;

            TransportLabel.Visibility = visibleSetting;
            TransportComboBox.Visibility = visibleSetting;

            OutboundProxyTextBox.Visibility = visibleSetting;
            OutboundProxyLabel.Visibility = visibleSetting;
        }

        public override void ShowSuperOptions(bool show)
        {
            base.ShowSuperOptions(show);
            System.Windows.Visibility visibleSetting = System.Windows.Visibility.Collapsed;
            if (show)
            {
                visibleSetting = System.Windows.Visibility.Visible;
            }
            RunWizardButton.Visibility = visibleSetting;
            ClearSettingsButton.Visibility = visibleSetting;

            PasswordLabel.Visibility = visibleSetting;
            PasswordTextBox.Visibility = visibleSetting;

        }
        #endregion

        private bool IsTransportChanged()
        {
            if (App.CurrentAccount == null)
                return false;

            var transportText = TransportComboBox.SelectedItem as TextBlock;
            string transportString = transportText.Text;
            if ((string.IsNullOrWhiteSpace(transportString) && !string.IsNullOrWhiteSpace(App.CurrentAccount.Transport)) ||
                (!string.IsNullOrWhiteSpace(transportString) && string.IsNullOrWhiteSpace(App.CurrentAccount.Transport)))
                return true;
            if ((!string.IsNullOrWhiteSpace(transportString) && !string.IsNullOrWhiteSpace(App.CurrentAccount.Transport)) &&
                (!transportString.Equals(App.CurrentAccount.Transport)))
                return true;

            return false;
        }


        #region SIP Account
        private void OnRunAssistant(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Run Assistant Clicked");
            if (MessageBox.Show("Launching the Wizard will delete any existing proxy configuration. Are you sure you want to proceed?", "Run Wizard", MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                // run the wizard
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.RunWizard);
            }
        }

        private void OnClearSettings(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Clear Settings Clicked");
            if (MessageBox.Show("Launching the Wizard will delete any existing proxy configuration. Are you sure you want to proceed?", "Clear Settings", MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.ClearSettings);
            }
        }

        private void OnSaveAccount(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount == null)
                return;
            StringBuilder errorMessage = new StringBuilder("");

            if (string.IsNullOrWhiteSpace(UserNameTextBox.Text))
            {
                errorMessage.Append("Please enter a user name.");
                //                MessageBox.Show("Incorrect login", "ACE", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (string.IsNullOrWhiteSpace(PasswordTextBox.Password))
            {
                errorMessage.Append("Please enter a password.");
                //                MessageBox.Show("Empty password is not allowed", "ACE", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (string.IsNullOrWhiteSpace(DomainTextBox.Text))
            {
                errorMessage.Append("Please enter a SIP Server Address.");
                //                MessageBox.Show("Incorrect SIP Server Address", "ACE", MessageBoxButton.OK, MessageBoxImage.Error);
            }


            ushort port = 0;

            ushort.TryParse(ProxyTextBox.Text, out port);
            if (port < 1 || port > 65535)
            {
                errorMessage.Append("Please enter a valid SIP Server Port.");
                //                MessageBox.Show("Incorrect SIP Server Port", "ACE", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            if (!string.IsNullOrEmpty(errorMessage.ToString()))
            {
                MessageBox.Show(errorMessage.ToString(), "ACE", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (App.CurrentAccount != null)
            {
                bool isChanged = false;
                bool resyncContacts = false;
                if (App.CurrentAccount.ProxyPort != port)
                {
                    App.CurrentAccount.ProxyPort = port;
                    isChanged = true;
                }
                if (ValueChanged(App.CurrentAccount.AuthID, UserIdTextBox.Text))
                {
                    App.CurrentAccount.AuthID = UserIdTextBox.Text;
                    isChanged = true;
                }
                if (ValueChanged(App.CurrentAccount.Username, UserNameTextBox.Text))
                {
                    App.CurrentAccount.Username = UserNameTextBox.Text;
                    // let the UI reflect the change.
                    OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.UserNameChanged);
                    isChanged = true;
                }
                if (ValueChanged(App.CurrentAccount.Password, PasswordTextBox.Password))
                {
                    App.CurrentAccount.Password = PasswordTextBox.Password;
                    isChanged = true;
                }
                if (ValueChanged(App.CurrentAccount.ProxyHostname, DomainTextBox.Text))
                {
                    App.CurrentAccount.ProxyHostname = DomainTextBox.Text;
                    isChanged = true;
                }
                if (ValueChanged(App.CurrentAccount.RegistrationUser, UserNameTextBox.Text))
                {
                    App.CurrentAccount.RegistrationUser = UserNameTextBox.Text;
                    isChanged = true;
                    resyncContacts = true;
                }
                if (ValueChanged(App.CurrentAccount.RegistrationPassword, PasswordTextBox.Password))
                {
                    App.CurrentAccount.RegistrationPassword = PasswordTextBox.Password;
                    isChanged = true;
                    resyncContacts = true;
                }
                if (ValueChanged(App.CurrentAccount.CardDavServerPath, CardDAVServerTextBox.Text))
                {
                    App.CurrentAccount.CardDavServerPath = CardDAVServerTextBox.Text;
                    resyncContacts = true;
                }
                if (ValueChanged(App.CurrentAccount.CardDavRealm, CardDAVRealmTextBox.Text))
                {
                    App.CurrentAccount.CardDavRealm = CardDAVRealmTextBox.Text;
                    resyncContacts = true;
                }
                //App.CurrentAccount.Transport = (string)TransportValueLabel.Content; // saved when changed
                if (isChanged)
                {
                    OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.RegistrationChanged);
                }
                if (resyncContacts)
                {
                    OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.CardDavConfigChanged);
                }
            }
        }

        private bool ValueChanged(string oldString, string newString)
        {
            if ((!string.IsNullOrEmpty(newString) && !string.IsNullOrEmpty(oldString)) &&
                !newString.Equals(oldString))
            {
                return true;
            }
            return false;
        }

        public void OnUserNameChanged(Object sender, RoutedEventArgs args)
        {
            if ((App.CurrentAccount == null) || !this.IsVisible)
                return;
            string newUserName = UserNameTextBox.Text;
            if (string.IsNullOrEmpty(newUserName))
            {
                string oldUserName = App.CurrentAccount.Username;
                UserNameTextBox.Text = oldUserName;
            }
        }

        public void OnPasswordChanged(Object sender, RoutedEventArgs args)
        {
            if ((App.CurrentAccount == null) || !this.IsVisible)
                return;
            string newPassword = PasswordTextBox.Password;
            if (string.IsNullOrEmpty(newPassword))
            {
                string oldPassword = App.CurrentAccount.Password;
                PasswordTextBox.Password = oldPassword;
            }
        }
        public void OnDomainChanged(Object sender, RoutedEventArgs args)
        {
            if ((App.CurrentAccount == null) || !this.IsVisible)
                return;
            string newDomain = this.DomainTextBox.Text;
            if (string.IsNullOrEmpty(newDomain))
            {
                string oldDomain = App.CurrentAccount.ProxyHostname;
                this.DomainTextBox.Text = oldDomain;
            }
        }

        public void OnProxyPortChanged(Object sender, RoutedEventArgs args)
        {
            if ((App.CurrentAccount == null) || !this.IsVisible)
                return;
            string newProxyPort = ProxyTextBox.Text;
            if (string.IsNullOrEmpty(newProxyPort))
            {
                int oldProxyPort = App.CurrentAccount.ProxyPort;
                ProxyTextBox.Text = Convert.ToString(oldProxyPort);
            }
        }

        private void OnTransportChanged(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Transport Clicked");
            if (App.CurrentAccount == null)
                return;
            if (IsTransportChanged())
            {
                var transportText = TransportComboBox.SelectedItem as TextBlock;
                string transportString = transportText.Text;
                App.CurrentAccount.Transport = transportString;
                if (transportString.ToUpper().Equals("TCP"))
                {
                    App.CurrentAccount.ProxyPort = 25060;
                }
                else if (transportString.ToUpper().Equals("TLS"))
                {
                    App.CurrentAccount.ProxyPort = 25061;
                }
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.RegistrationChanged);
            }
        }

        public void OnOutboundProxyChanged(Object sender, RoutedEventArgs args)
        {
            if ((App.CurrentAccount == null) || !this.IsVisible)
                return;
            string newProxy = this.OutboundProxyTextBox.Text.Trim();
            if (string.Compare(newProxy, App.CurrentAccount.OutboundProxyAddress, StringComparison.InvariantCultureIgnoreCase) != 0)
            {
                if (App.CurrentAccount.UseOutboundProxy)
                {
                    if (string.IsNullOrEmpty(newProxy))
                        App.CurrentAccount.UseOutboundProxy = false;

                    App.CurrentAccount.OutboundProxyAddress = newProxy;
                    OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.RegistrationChanged);
                }
            }
        }

        public void OnCardDAVServerChanged(Object sender, RoutedEventArgs args)
        {
            if ((App.CurrentAccount == null) || !this.IsVisible)
                return;
            if (App.CurrentAccount.CardDavServerPath != CardDAVServerTextBox.Text)
            {
                App.CurrentAccount.CardDavServerPath = CardDAVServerTextBox.Text;
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.CardDavConfigChanged);
            }
        }

        public void OnCardDAVRealmChanged(Object sender, RoutedEventArgs args)
        {
            if ((App.CurrentAccount == null) || !this.IsVisible)
                return;
            if (App.CurrentAccount.CardDavRealm != CardDAVRealmTextBox.Text)
            {
                App.CurrentAccount.CardDavRealm = CardDAVRealmTextBox.Text;
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.CardDavConfigChanged);
            }
        }

        private void OnProxyChangedByUser(object sender, KeyEventArgs e)
        {
            App.CurrentAccount.UseOutboundProxy = true;
        }

        #endregion


        #region VoiceMail Uri & MWI
        private void OnVideoMailUriChanged(Object sender, RoutedEventArgs args)
        {
            Console.WriteLine("VideoMail URI Changed");
            if (App.CurrentAccount == null)
                return;
            string oldVideoMailUri = App.CurrentAccount.VideoMailUri;
            string newVideoMailUri = VideoMailUriTextBox.Text;
            if (string.IsNullOrEmpty(newVideoMailUri))
            {
                VideoMailUriTextBox.Text = oldVideoMailUri;
            }
            else
            {
                if (!string.IsNullOrEmpty(newVideoMailUri))
                {
                    try
                    {
                        App.CurrentAccount.VideoMailUri = newVideoMailUri;
                        ServiceManager.Instance.SaveAccountSettings();
                    }
                    catch (Exception)
                    {
                        //TODO: ADD logging handler this class
                    }
                }
            }
        }

        private void MWIUriTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("MWI URI Changed");
            if (App.CurrentAccount == null)
                return;
            string newmwiUri = MWIUriTextBox.Text;
            try
            {
                App.CurrentAccount.MWIUri = newmwiUri;
                ServiceManager.Instance.SaveAccountSettings();

                // Subscribe for video mail
                ServiceManager.Instance.LinphoneService.SubscribeForVideoMWI(newmwiUri);
            }
            catch (Exception)
            {
                //TODO: ADD logging handler this class
            }
        }
        #endregion
    }
}
