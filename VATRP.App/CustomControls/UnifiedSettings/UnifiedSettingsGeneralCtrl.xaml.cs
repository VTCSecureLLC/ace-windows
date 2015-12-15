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

namespace com.vtcsecure.ace.windows.CustomControls.UnifiedSettings
{
    /// <summary>
    /// Interaction logic for UnifiedSettingsGeneralCtrl.xaml
    /// </summary>
    public partial class UnifiedSettingsGeneralCtrl : BaseUnifiedSettingsPanel
    {
        public UnifiedSettingsGeneralCtrl()
        {
            InitializeComponent();
            Title = "General";
            this.Loaded += UnifiedSettingsGeneralCtrl_Loaded;
        }

        // ToDo - VATRP98populate when we know where the settings are stored
        private void UnifiedSettingsGeneralCtrl_Loaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        public void Initialize()
        {
        }

        //
        private void OnStartOnBoot(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Start at Boot Clicked");
            bool enabled = this.StartAtBootCheckbox.IsChecked ?? false;
           // ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
             //   Configuration.ConfEntry.AUTO_ANSWER, enabled);
        }
        private void OnWifiOnly(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Auto Answer Call Clicked");
            bool enabled = WifiOnlyCheckBox.IsChecked ?? false;
         //   ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
           //     Configuration.ConfEntry.AUTO_ANSWER, enabled);
        }
        private void OnSipEncryption(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("SIP Encryption Clicked");
            bool enabled = SipEncryptionCheckbox.IsChecked ?? false;
//            ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
  //              Configuration.ConfEntry.AUTO_ANSWER, enabled);
        }
        private void OnAutoAnswerAfterNotification(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Auto Answer After Notification Clicked");
            bool enabled = AutoAnswerAfterNotificationCheckBox.IsChecked ?? false;
            //ServiceManager.Instance.ConfigurationService.Set(Configuration.ConfSection.GENERAL,
              //  Configuration.ConfEntry.AUTO_ANSWER, enabled);
        }

    }
}
