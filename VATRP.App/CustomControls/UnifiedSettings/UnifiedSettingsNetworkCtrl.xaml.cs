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
    /// Interaction logic for UnifiedSettingsNetworkCtrl.xaml
    /// </summary>
    public partial class UnifiedSettingsNetworkCtrl : BaseUnifiedSettingsPanel
    {
        public UnifiedSettingsNetworkCtrl()
        {
            InitializeComponent();
        }

        public override void Initialize()
        {
            base.Initialize();
            if (App.CurrentAccount == null)
                return;
            if (App.CurrentAccount.EnableICE && App.CurrentAccount.EnableSTUN)
                App.CurrentAccount.EnableICE = false; // normalization

            UseStunServerCheckbox.IsChecked = App.CurrentAccount.EnableSTUN;
            StunServerTextBox.Text = App.CurrentAccount.STUNAddress;
            UseIceServerCheckbox.IsChecked = App.CurrentAccount.EnableICE;

            foreach (TextBlock textBlock in MediaEncryptionComboBox.Items)
            {
                if (textBlock.Text.Equals(App.CurrentAccount.MediaEncryption))
                {
                    MediaEncryptionComboBox.SelectedItem = textBlock;
                }
            }

            AdaptiveRateCheckbox.IsChecked = App.CurrentAccount.EnableAdaptiveRate;
            UploadBandwidthTextBox.Text = App.CurrentAccount.UploadBandwidth.ToString();
            DownloadBandwidthTextBox.Text = App.CurrentAccount.DownloadBandwidth.ToString();
            QoSCheckbox.IsChecked = App.CurrentAccount.EnableQualityOfService;
        }

        private void OnAdaptiveRateChecked(object sender, RoutedEventArgs e)
        {
            bool enabled = AdaptiveRateCheckbox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.EnableAdaptiveRate)
            {
                App.CurrentAccount.EnableAdaptiveRate = enabled;
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
            }
        }

        private void OnQoSChecked(object sender, RoutedEventArgs e)
        {
            bool enabled = QoSCheckbox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.EnableQualityOfService)
            {
                App.CurrentAccount.EnableQualityOfService = enabled;
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
            }
        }

        private void OnStunServerChecked(object sender, RoutedEventArgs e)
        {
            bool enabled = UseStunServerCheckbox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.EnableSTUN)
            {
                App.CurrentAccount.EnableSTUN = enabled;
                if (enabled)
                {
                    App.CurrentAccount.EnableICE = false;
                    UseIceServerCheckbox.IsChecked = App.CurrentAccount.EnableICE;
                }

                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
            }
        }
		
        public void OnStunServerChanged(Object sender, RoutedEventArgs args)
        {
            string newStunServer = StunServerTextBox.Text;
            // VATRP-1949: removed check for empty stun server. However - maybe we want a test here so that if the user has
            //  Stun Server checkbox enabled we prompt the user if the value does not look like a valid address?
            if (App.CurrentAccount != null)
            {
                App.CurrentAccount.STUNAddress = newStunServer;
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
            }
        }

        private void OnIceServerChecked(object sender, RoutedEventArgs e)
        {
            bool enabled = UseIceServerCheckbox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.EnableICE)
            {
                App.CurrentAccount.EnableICE = enabled;
                if (enabled)
                {
                    App.CurrentAccount.EnableSTUN = false;
                    UseStunServerCheckbox.IsChecked = App.CurrentAccount.EnableSTUN;
                }
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
            }
        }

        public void OnUploadBandwidthChanged(Object sender, RoutedEventArgs args)
        {
            string newBandwidth = UploadBandwidthTextBox.Text;
            if (!string.IsNullOrEmpty(newBandwidth))
            {
                int bandwidth = 0;
                if (int.TryParse(newBandwidth, out bandwidth))
                {
                    App.CurrentAccount.UploadBandwidth = bandwidth;
                    OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
                }
            }
        }
        public void OnDownloadBandwidthChanged(Object sender, RoutedEventArgs args)
        {
            string newBandwidth = DownloadBandwidthTextBox.Text;
            if (!string.IsNullOrEmpty(newBandwidth))
            {
                int bandwidth = 0;
                if (int.TryParse(newBandwidth, out bandwidth))
                {
                    App.CurrentAccount.DownloadBandwidth = bandwidth;
                    OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
                }
            }
        }

        #region NotYetSpecifiedForWindows
        private void OnEdgeOptimization(object sender, RoutedEventArgs e)
        {
            bool enabled = EdgeOptimizationCheckbox.IsChecked ?? false;
            // Placeholder - not yet indicated for Windows

        }

        private void OnWifiOnly(object sender, RoutedEventArgs e)
        {
            bool enabled = WifiOnlyCheckbox.IsChecked ?? false;
            // Placeholder - not yet indicated for Windows

        }

        private void OnRandomPort(object sender, RoutedEventArgs e)
        {
            bool enabled = RandomPortCheckbox.IsChecked ?? false;
            // Placeholder - not yet indicated for Windows

        }

        public void OnAudioPortsChanged(Object sender, RoutedEventArgs args)
        {
            string newAudioPorts = AudioPortsTextBox.Text;
            // Placeholder - not yet indicated for Windows
            //            if (string.IsNullOrEmpty(newAudioPorts))
            //            {
            //                string oldAudioPorts = App.CurrentAccount.Username;
            //                AudioPortsTextBox.Text = oldAudioPorts;
            //            }
        }

        public void OnVideoPortsChanged(Object sender, RoutedEventArgs args)
        {
            string newVideoPorts = VideoPortsTextBox.Text;
            // Placeholder - not yet indicated for Windows

            //            if (string.IsNullOrEmpty(newVideoPorts))
            //            {
            //                string oldVideoPorts = App.CurrentAccount.Username;
            //                VideoPortsTextBox.Text = oldVideoPorts;
            //            }
        }

        private void OnIPv6(object sender, RoutedEventArgs e)
        {
            bool enabled = IPv6Checkbox.IsChecked ?? false;
            // Placeholder - not yet indicated for Windows

        }
        private void OnMediaEncryptionChanged(object sender, RoutedEventArgs e)
        {
            TextBlock valueTB = (TextBlock)MediaEncryptionComboBox.SelectedItem;
            string value = valueTB.Text;
            if (App.CurrentAccount != null)
            {
                App.CurrentAccount.MediaEncryption = value;
                // update media settings.
                ServiceManager.Instance.ApplyMediaSettingsChanges();
                ServiceManager.Instance.SaveAccountSettings();
            }

        }
        private void OnPushNotifications(object sender, RoutedEventArgs e)
        {
            bool enabled = PushNotificationsCheckbox.IsChecked ?? false;
            // Placeholder - not yet indicated for Windows

        }
        #endregion
    }
}
