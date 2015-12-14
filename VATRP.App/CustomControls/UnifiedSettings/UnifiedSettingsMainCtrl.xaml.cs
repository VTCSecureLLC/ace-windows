﻿using com.vtcsecure.ace.windows.Services;
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

        public UnifiedSettingsMainCtrl()
        {
            InitializeComponent();
            Title = "Settings";
            this.Loaded += UnifiedSettingsMainCtrl_Loaded;
            
        }

        void UnifiedSettingsMainCtrl_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.CurrentAccount != null)
            {
                UserIdTextBox.Text = App.CurrentAccount.AuthID;
                UserNameTextBox.Text = App.CurrentAccount.Username;
                PasswordTextBox.Password = App.CurrentAccount.Password;
                DomainTextBox.Text = App.CurrentAccount.ProxyHostname;
                ProxyTextBox.Text = Convert.ToString(App.CurrentAccount.ProxyPort);
                TransportValueLabel.Content = App.CurrentAccount.Transport;
            }

            this.AutoAnswerCheckBox.IsChecked = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AUTO_ANSWER, false);
            this.AvpfCheckbox.IsChecked = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AVPF_ON, true);


#if DEBUG
            DebugMenuLabel.Visibility = System.Windows.Visibility.Visible;
            ReleaseCoreButton.Visibility = System.Windows.Visibility.Visible;
            ClearCacheButton.Visibility = System.Windows.Visibility.Visible;
            BatteryAlertButton.Visibility = System.Windows.Visibility.Visible;
            AutoAnswerLabel.Visibility = System.Windows.Visibility.Visible;
            AutoAnswerCheckBox.Visibility = System.Windows.Visibility.Visible;
#endif

        }

        public override void SaveData()
        {
            if (App.CurrentAccount == null)
                return;
            
            if (string.IsNullOrWhiteSpace(UserNameTextBox.Text))
            {
                MessageBox.Show("Incorrect login", "ACE", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (string.IsNullOrWhiteSpace(PasswordTextBox.Password))
            {
                MessageBox.Show("Empty password is not allowed", "ACE", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (string.IsNullOrWhiteSpace(DomainTextBox.Text))
            {
                MessageBox.Show("Incorrect SIP Server Address", "ACE", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            ushort port = 0;

            ushort.TryParse(ProxyTextBox.Text, out port);
            if (port < 1 || port > 65535)
            {
                MessageBox.Show("Incorrect SIP Server Port", "ACE", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            if (App.CurrentAccount != null)
            {
                App.CurrentAccount.ProxyPort = port;
                App.CurrentAccount.AuthID = UserIdTextBox.Text;
                App.CurrentAccount.Username = UserNameTextBox.Text;
                App.CurrentAccount.Password = PasswordTextBox.Password;
                App.CurrentAccount.ProxyHostname = DomainTextBox.Text;
                App.CurrentAccount.RegistrationUser = UserNameTextBox.Text;
                App.CurrentAccount.RegistrationPassword = PasswordTextBox.Password;
                App.CurrentAccount.Transport = (string)TransportValueLabel.Content;
            }

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
            OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.ClearAccount);
        }

        public void OnUserNameChanged(Object sender, RoutedEventArgs args)
        {
            if (App.CurrentAccount == null)
                return;
            OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.UserNameChanged);
        } 

        private void OnTransport(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Transport Clicked");
        }


        // ToDo VATRP-985 - Liz E. - not sure where the outbound proxy setting lives
        private void OnOutboundProxy(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Outbound Proxy Clicked");
        }
        private void OnAvpf(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("AVPF Clicked");
            bool enabled = AvpfCheckbox.IsChecked ?? false;
            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.AVPF_ON, enabled);
            ServiceManager.Instance.ConfigurationService.SaveConfig();

        }
        private void OnMoreOptions(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("More Options Clicked");
            // changes visibility of the more options items:
            System.Windows.Visibility visibleSetting = System.Windows.Visibility.Collapsed;
            bool showMoreOptions = MoreOptionsCheckbox.IsChecked ?? false;
            if (showMoreOptions)
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

        }
        #endregion

        #region Preferences
        private void OnEnableVideo(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Enable Video Clicked");
            bool enabled = EnableVideoCheckBox.IsChecked ?? false;
            // ToDo: VATRP-985: where is the enable video setting?

            ServiceManager.Instance.ConfigurationService.SaveConfig();

        }

        private void OnTextPreferences(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Text Preferences Clicked");
        }

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
        }

        private void OnNetworkPreferences(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Network Preferences Clicked");
        }

        private void OnAdvancedPreferences(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Advanced Preferences Clicked");
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
