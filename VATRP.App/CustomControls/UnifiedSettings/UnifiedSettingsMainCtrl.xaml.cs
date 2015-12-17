using com.vtcsecure.ace.windows.Services;
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
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.CustomControls.UnifiedSettings
{
    /// <summary>
    /// Interaction logic for UnifiedSettingsMainCtrl.xaml
    /// </summary>
    // ToDo VATRP-985: Unified Settings: Make it so that if the edit boxes are done editing the data validates/updates immediately
    public partial class UnifiedSettingsMainCtrl : BaseUnifiedSettingsPanel
    {
        public event UnifiedSettings_EnableSettings ShowSettingsUpdate;

        public UnifiedSettingsMainCtrl()
        {
            InitializeComponent();
            Title = "Settings";
            this.Loaded += UnifiedSettingsMainCtrl_Loaded;
            
        }

        void UnifiedSettingsMainCtrl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        public void Initialize()
        {
            if (App.CurrentAccount != null)
            {
                UserIdTextBox.Text = App.CurrentAccount.AuthID;
                UserNameTextBox.Text = App.CurrentAccount.Username;
                PasswordTextBox.Password = App.CurrentAccount.Password;
                DomainTextBox.Text = App.CurrentAccount.ProxyHostname;
                ProxyTextBox.Text = Convert.ToString(App.CurrentAccount.ProxyPort);
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
                        string test = selectedItem.Text;
                        break;
                    }
                }
            }

            this.AutoAnswerCheckBox.IsChecked = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AUTO_ANSWER, false);
            this.AvpfCheckbox.IsChecked = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AVPF_ON, true);
        }

        #region SettingsLevel
        public override void ShowDebugOptions(bool show)
        {
            System.Windows.Visibility visibleSetting = System.Windows.Visibility.Collapsed;
            if (show)
            {
                visibleSetting = System.Windows.Visibility.Visible;
            }
            DebugMenuLabel.Visibility = visibleSetting;
            ReleaseCoreButton.Visibility = visibleSetting;
            ClearCacheButton.Visibility = visibleSetting;
            BatteryAlertButton.Visibility = visibleSetting;
            AutoAnswerLabel.Visibility = visibleSetting;
            AutoAnswerCheckBox.Visibility = visibleSetting;
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

            OutboundProxyLabel.Visibility = visibleSetting;
            OutboundProxyCheckbox.Visibility = visibleSetting;

            AvpfLabel.Visibility = visibleSetting;
            AvpfCheckbox.Visibility = visibleSetting;

            PreferencesLabel.Visibility = visibleSetting;

            EnableVideoLabel.Visibility = visibleSetting;
            EnableVideoCheckBox.Visibility = visibleSetting;

            EnableRTTLabel.Visibility = visibleSetting;
            EnableRTTCheckBox.Visibility = visibleSetting;

            AudioButton.Visibility = visibleSetting;
            AudioButtonLabel.Visibility = visibleSetting;

            VideoButton.Visibility = visibleSetting;
            VideoButtonLabel.Visibility = visibleSetting;

            CallButton.Visibility = visibleSetting;
            CallButtonLabel.Visibility = visibleSetting;

            NetworkButton.Visibility = visibleSetting;
            NetworkButtonLabel.Visibility = visibleSetting;

            // not yet specified for windows
            AdvancedButton.Visibility = System.Windows.Visibility.Collapsed;//visibleSetting;
            AdvancedButtonLabel.Visibility = System.Windows.Visibility.Collapsed;//visibleSetting;

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
            ClearAccountButton.Visibility = visibleSetting;

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

        #region Settings Menu
        private void OnGeneralSettings(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("GeneralSettings Clicked");
            OnContentChanging(UnifiedSettingsContentType.GeneralContent);
        }

        private void OnAudioVideo(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("AudioVideo Clicked");
            OnContentChanging(UnifiedSettingsContentType.AudioVideoContent);
        }

        private void OnTheme(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Theme Clicked");
            OnContentChanging(UnifiedSettingsContentType.ThemeContent);
        }

        private void OnText(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("OnText Clicked");
            OnContentChanging(UnifiedSettingsContentType.TextContent);
        }

        private void OnSummary(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Summary Clicked");
            OnContentChanging(UnifiedSettingsContentType.SummaryContent);
        }
        #endregion

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

        private void OnClearAccount(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Clear Account Clicked");
            if (MessageBox.Show("Launching the Wizard will delete any existing proxy configuration. Are you sure you want to proceed?", "Clear Account", MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.ClearAccount);
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
                
                App.CurrentAccount.ProxyPort = port;
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
                }
                if (ValueChanged(App.CurrentAccount.RegistrationPassword, PasswordTextBox.Password))
                { 
                    App.CurrentAccount.RegistrationPassword = PasswordTextBox.Password;
                    isChanged = true;
                }
                //App.CurrentAccount.Transport = (string)TransportValueLabel.Content; // saved when changed
                if (isChanged)
                {
                    OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.RegistrationChanged);
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
                
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.RegistrationChanged);
            }
        }


        // ToDo VATRP-985 - Liz E. - not sure where the outbound proxy setting lives
        private void OnOutboundProxy(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Outbound Proxy Clicked");
            if (App.CurrentAccount == null)
                return;
            bool enabled = OutboundProxyCheckbox.IsChecked ?? false;
            App.CurrentAccount.UseOutboundProxy = enabled;
            ServiceManager.Instance.SaveAccountSettings();
        }
        private void OnAvpf(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("AVPF Clicked");
            bool enabled = AvpfCheckbox.IsChecked ?? false;
            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AVPF_ON, enabled);
            ServiceManager.Instance.ConfigurationService.SaveConfig();
            ServiceManager.Instance.ApplyAVPFChanges();
        }
        #endregion

        #region Preferences
        private void OnEnableVideo(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Enable Video Clicked");
            if (App.CurrentAccount != null)
            {
                bool enabled = EnableVideoCheckBox.IsChecked ?? false;
                App.CurrentAccount.EnableVideo = enabled;
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.VideoPolicyChanged);
            }                        
        }

        private void OnEnableRTT(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Enable Real Time Text Call Clicked");
            bool enabled = EnableRTTCheckBox.IsChecked ?? false;
            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.USE_RTT, enabled);
            ServiceManager.Instance.ConfigurationService.SaveConfig();
        }

        // Liz E. - the spreadsheet calls for an rtt checkbox - using that instead.
        /*
        private void OnTextPreferences(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Text Preferences Clicked");
            OnContentChanging(UnifiedSettingsContentType.TextContent);
        }
         * */

        private void OnAudioPreferences(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Audio Preferences Clicked");
            OnContentChanging(UnifiedSettingsContentType.AudioSettingsContent);
        }

        private void OnVideoPreferences(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Video Preferences Clicked");
            OnContentChanging(UnifiedSettingsContentType.VideoSettingsContent);
        }

        private void OnCallPreferences(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Call Preferences Clicked");
            OnContentChanging(UnifiedSettingsContentType.CallSettingsContent);
        }

        private void OnNetworkPreferences(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Network Preferences Clicked");
            OnContentChanging(UnifiedSettingsContentType.NetworkSettingsContent);
        }

        private void OnAdvancedPreferences(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Advanced Preferences Clicked");
            OnContentChanging(UnifiedSettingsContentType.AdvancedSettingsContent);
        }
        #endregion

        #region DebugMenu
        private void OnReleaseCore(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Release Core Clicked");
        }

        private void OnClearCache(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Clear Cache Clicked");
        }

        private void OnBatteryAlert(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Battery Alert Clicked");
        }

        private void OnAutoAnswerCall(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Auto Answer Call Clicked");
            bool enabled = AutoAnswerCheckBox.IsChecked ?? false;
            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AUTO_ANSWER, enabled);
            ServiceManager.Instance.ConfigurationService.SaveConfig();

        }
        #endregion

    }
}
