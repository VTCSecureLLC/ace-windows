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
            UseStunServerCheckbox.IsChecked = App.CurrentAccount.EnubleSTUN;
            StunServerTextBox.Text = App.CurrentAccount.STUNAddress;
            StunServerPortTextBox.Text = App.CurrentAccount.STUNPort.ToString();
        }

        private void OnStunServerChecked(object sender, RoutedEventArgs e)
        {
            bool enabled = UseStunServerCheckbox.IsChecked ?? false;
            if (enabled != App.CurrentAccount.EnubleSTUN)
            {
                App.CurrentAccount.EnubleSTUN = enabled;
                OnAccountChangeRequested(Enums.ACEMenuSettingsUpdateType.NetworkSettingsChanged);
            }
        }
        public void OnStunServerChanged(Object sender, RoutedEventArgs args)
        {
            string newStunServer = StunServerTextBox.Text;
            if (string.IsNullOrEmpty(newStunServer))
            {
                string oldStunServer = App.CurrentAccount.Username;
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

        #region NotYetSpecifiedForWindows
        private void OnEdgeOptimization(object sender, RoutedEventArgs e)
        {
            bool enabled = EdgeOptimizationCheckbox.IsChecked ?? false;

        }

        private void OnWifiOnly(object sender, RoutedEventArgs e)
        {
            bool enabled = WifiOnlyCheckbox.IsChecked ?? false;

        }

        private void OnRandomPort(object sender, RoutedEventArgs e)
        {
            bool enabled = RandomPortCheckbox.IsChecked ?? false;

        }

        public void OnAudioPortsChanged(Object sender, RoutedEventArgs args)
        {
            string newAudioPorts = AudioPortsTextBox.Text;
            //            if (string.IsNullOrEmpty(newAudioPorts))
            //            {
            //                string oldAudioPorts = App.CurrentAccount.Username;
            //                AudioPortsTextBox.Text = oldAudioPorts;
            //            }
        }

        public void OnVideoPortsChanged(Object sender, RoutedEventArgs args)
        {
            string newVideoPorts = VideoPortsTextBox.Text;
            //            if (string.IsNullOrEmpty(newVideoPorts))
            //            {
            //                string oldVideoPorts = App.CurrentAccount.Username;
            //                VideoPortsTextBox.Text = oldVideoPorts;
            //            }
        }

        private void OnIPv6(object sender, RoutedEventArgs e)
        {
            bool enabled = IPv6Checkbox.IsChecked ?? false;

        }
        private void OnMediaEncryptionChanged(object sender, RoutedEventArgs e)
        {
        }
        private void OnPushNotifications(object sender, RoutedEventArgs e)
        {
            bool enabled = PushNotificationsCheckbox.IsChecked ?? false;

        }
        #endregion
    }
}
