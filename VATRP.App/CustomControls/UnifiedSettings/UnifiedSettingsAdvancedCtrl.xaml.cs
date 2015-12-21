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
    /// Interaction logic for UnifiedSettingsAdvancedCtrl.xaml
    /// </summary>
    public partial class UnifiedSettingsAdvancedCtrl : BaseUnifiedSettingsPanel
    {
        public UnifiedSettingsAdvancedCtrl()
        {
            InitializeComponent();
        }

        private void OnDebugMode(object sender, RoutedEventArgs e)
        {
            // Placeholder - not yet indicated for Windows
        }

        private void OnClearLogs(object sender, RoutedEventArgs e)
        {
            // Placeholder - not yet indicated for Windows
        }
        private void OnSendLogs(object sender, RoutedEventArgs e)
        {
            // Placeholder - not yet indicated for Windows
        }

        private void OnPersistentNotifier(object sender, RoutedEventArgs e)
        {
            // Placeholder - not yet indicated for Windows
        }
        private void OnEnableAnimations(object sender, RoutedEventArgs e)
        {
            // Placeholder - not yet indicated for Windows
        }

        public void OnSharingServerURLChanged(Object sender, RoutedEventArgs args)
        {
            // Placeholder - not yet indicated for Windows
        }

        public void OnRemoteProvisioningChanged(Object sender, RoutedEventArgs args)
        {
            // Placeholder - not yet indicated for Windows
        }

        public void OnSIPExpireChanged(Object sender, RoutedEventArgs args)
        {
            // Placeholder - not yet indicated for Windows
        }

        public void OnFileSharingServerURLChanged(Object sender, RoutedEventArgs args)
        {
            // Placeholder - not yet indicated for Windows
        }

    }
}
