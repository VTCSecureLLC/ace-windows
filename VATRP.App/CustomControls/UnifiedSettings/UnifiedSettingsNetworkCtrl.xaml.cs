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
            UseStunServerCheckbox.IsChecked = App.CurrentAccount.EnableSTUN;
            StunServerTextBox.Text = App.CurrentAccount.STUNAddress;
            StunServerPortTextBox.Text = App.CurrentAccount.STUNPort.ToString();

            UseIceServerCheckbox.IsChecked = App.CurrentAccount.EnableICE;
            IceServerTextBox.Text = App.CurrentAccount.ICEAddress;
            IceServerPortTextBox.Text = App.CurrentAccount.ICEPort.ToString();

            foreach (TextBlock textBlock in MediaEncryptionComboBox.Items)
            {
                if (textBlock.Text.Equals(App.CurrentAccount.MediaEncryption))
                {
                    MediaEncryptionComboBox.SelectedItem = textBlock;
                }
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
                    if (App.CurrentAccount.EnableICE == true)
                    {
                        App.CurrentAccount.EnableICE = false;
                        UseIceServerCheckbox.IsChecked = false;
                    }
                }
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
            }
        }
        public void OnStunServerChanged(Object sender, RoutedEventArgs args)
        {
            string newStunServer = StunServerTextBox.Text;
            if (string.IsNullOrEmpty(newStunServer))
            {
                string oldStunServer = App.CurrentAccount.STUNAddress;
                StunServerTextBox.Text = oldStunServer;
            }
            else
            {
                App.CurrentAccount.STUNAddress = newStunServer;
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
            }
        }

        public void OnStunServerPortChanged(Object sender, RoutedEventArgs args)
        {
            string newStunServerPort = StunServerPortTextBox.Text;
            if (string.IsNullOrEmpty(newStunServerPort))
            {
                string oldStunServerPort = App.CurrentAccount.STUNPort.ToString();
                StunServerPortTextBox.Text = oldStunServerPort;
            }
            else
            {
                ushort port = 0;
                ushort.TryParse(newStunServerPort, out port);
                if (port < 1 || port > 65535)
                {
                    MessageBox.Show("Incorrect STUN port", "ACE", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                } 
                App.CurrentAccount.STUNPort = port;
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
                    if (App.CurrentAccount.EnableSTUN == true)
                    {
                        App.CurrentAccount.EnableSTUN = false;
                        UseStunServerCheckbox.IsChecked = false;
                    }
                }
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
            }
        }
        public void OnIceServerChanged(Object sender, RoutedEventArgs args)
        {
            string newIceServer = IceServerTextBox.Text;
            if (string.IsNullOrEmpty(newIceServer))
            {
                string oldIceServer = App.CurrentAccount.ICEAddress;
                IceServerTextBox.Text = oldIceServer;
            }
            else
            {
                App.CurrentAccount.ICEAddress = newIceServer;
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
            }
        }

        public void OnIceServerPortChanged(Object sender, RoutedEventArgs args)
        {
            string newIceServerPort = IceServerPortTextBox.Text;
            if (string.IsNullOrEmpty(newIceServerPort))
            {
                string oldIceServerPort = App.CurrentAccount.ICEPort.ToString();
                IceServerPortTextBox.Text = oldIceServerPort;
            }
            else
            {
                ushort port = 0;
                ushort.TryParse(newIceServerPort, out port);
                if (port < 1 || port > 65535)
                {
                    MessageBox.Show("Incorrect ICE port", "ACE", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                App.CurrentAccount.ICEPort = port;
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
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
