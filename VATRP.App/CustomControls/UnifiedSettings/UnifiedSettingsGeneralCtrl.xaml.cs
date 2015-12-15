using Microsoft.Win32;
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
            // intialize start on boot:
            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            string applicationName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            if (registryKey.GetValue(applicationName) == null)
            {
                // the application is not set to run at startup
                StartAtBootCheckbox.IsChecked = false;
            }
            else
            {
                // the application is set to run at startup
                StartAtBootCheckbox.IsChecked = true;
            }
        }

        //
        private void OnStartOnBoot(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Start at Boot Clicked");

            string applicationName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

            bool enabled = this.StartAtBootCheckbox.IsChecked ?? false;
            if (enabled)
            {
                string startupPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    key.SetValue(applicationName, "\"" + startupPath + "\"");
                }
            }
            else
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    key.DeleteValue(applicationName, false);
                }
            }
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
