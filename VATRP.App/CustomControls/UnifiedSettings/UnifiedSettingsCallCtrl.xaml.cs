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
using VATRP.Core.Model;

namespace com.vtcsecure.ace.windows.CustomControls.UnifiedSettings
{
    /// <summary>
    /// Interaction logic for UnifiedSettingsCallCtrl.xaml
    /// </summary>
    public partial class UnifiedSettingsCallCtrl : BaseUnifiedSettingsPanel
    {
        public UnifiedSettingsCallCtrl()
        {
            InitializeComponent();
        }

        public override void Initialize()
        {
            base.Initialize();
            this.CallPrefixTextBox.Text = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL, 
                 Configuration.ConfEntry.CALL_DIAL_PREFIX, "");
            this.CallSubstituteCheckBox.IsChecked = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                 Configuration.ConfEntry.CALL_DIAL_ESCAPE_PLUS, false);

            this.SendSipInfoDTMFCheckBox.IsChecked = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                 Configuration.ConfEntry.DTMF_SIP_INFO, false);
        }

        #region ShowSettingsLevel
        public override void ShowAdvancedOptions(bool show)
        {
            base.ShowAdvancedOptions(show);
            System.Windows.Visibility visibleSetting = System.Windows.Visibility.Collapsed;
            if (show)
            {
                visibleSetting = System.Windows.Visibility.Visible;
            }
            VoiceMailUriLabel.Visibility = visibleSetting;
            VoiceMailUriTextBox.Visibility = visibleSetting;
        }
        public override void ShowDebugOptions(bool show)
        {
            base.ShowDebugOptions(show);
            System.Windows.Visibility visibleSetting = System.Windows.Visibility.Collapsed;
            if (show)
            {
                visibleSetting = System.Windows.Visibility.Visible;
            }
            SendInbandDTMFLabel.Visibility = visibleSetting;
            SendInbandDTMFCheckBox.Visibility = visibleSetting;

            SendSipInfoDTMFLabel.Visibility = visibleSetting;
            SendSipInfoDTMFCheckBox.Visibility = visibleSetting;
        }
        public override void ShowSuperOptions(bool show)
        {
            base.ShowSuperOptions(show);
            System.Windows.Visibility visibleSetting = System.Windows.Visibility.Collapsed;
            if (show)
            {
                visibleSetting = System.Windows.Visibility.Visible;
            }

            CallPrefixLabel.Visibility = visibleSetting;
            CallPrefixTextBox.Visibility = visibleSetting;
        }
        #endregion

        private void OnCallPrefixChanged(Object sender, RoutedEventArgs args)
        {
            Console.WriteLine("Prefix Changed");
            string newCallPrefix = this.CallPrefixTextBox.Text;
            string oldCallPrefix = ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.CALL_DIAL_PREFIX, "");
            if (!newCallPrefix.Equals(oldCallPrefix))
            {
                ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                    Configuration.ConfEntry.CALL_DIAL_PREFIX, newCallPrefix);
                ServiceManager.Instance.ConfigurationService.SaveConfig();
            }
        }

        private void OnCallSubstituteEscapePlus(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Call Substitute Clicked");
            bool enabled = this.CallSubstituteCheckBox.IsChecked ?? false;
            if (enabled != ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL, Configuration.ConfEntry.CALL_DIAL_ESCAPE_PLUS, false))
            {
                ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL, Configuration.ConfEntry.CALL_DIAL_ESCAPE_PLUS, enabled);
                ServiceManager.Instance.ConfigurationService.SaveConfig();
            }
        }

        private void OnSendInbandDTMF(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Send Inband DTMF Clicked");

        }

        private void OnSendSipInfoDTMF(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Send SIP INFO DTMF Clicked");
            bool enabled = this.SendSipInfoDTMFCheckBox.IsChecked ?? false;
            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.DTMF_SIP_INFO, enabled);
            ServiceManager.Instance.ConfigurationService.SaveConfig();
        }

        private void OnVoiceMailUriChanged(Object sender, RoutedEventArgs args)
        {
            Console.WriteLine("VoiceMail URI Changed");
            if ((App.CurrentAccount == null) || !this.IsVisible)
                return;
        }

        private void OnRepeatCallNotification(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Repeat Call Notification Clicked");

        }
    }
}
