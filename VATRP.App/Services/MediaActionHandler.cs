using System.Windows;
using System.Windows.Controls;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using VATRP.Core.Services;
using VATRP.LinphoneWrapper.Enums;

namespace com.vtcsecure.ace.windows.Services
{
    internal static class MediaActionHandler
    {
        internal static void MakeAudioCall(string remoteUri)
        {
            ILinphoneService _linphoneService = ServiceManager.Instance.LinphoneService;
            

            if (MainWindow.RegistrationState != LinphoneRegistrationState.LinphoneRegistrationOk)
            {
                MessageBox.Show("Not Registered. Please register first", "ACE", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            if (MainWindow.RegistrationState != LinphoneRegistrationState.LinphoneRegistrationOk)
            {
                MessageBox.Show("Not Registered. Please register first", "ACE", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            _linphoneService.MakeCall(remoteUri, false, ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.USE_RTT, true));
        }

        internal static bool MakeVideoCall(string remoteUri)
        {
            ILinphoneService _linphoneService = ServiceManager.Instance.LinphoneService;

            if (!_linphoneService.CanMakeVideoCall())
            {
                MessageBox.Show("Video call not supported yet.", "ACE", MessageBoxButton.OK,
                   MessageBoxImage.Warning);
                return false;
            }

            if (MainWindow.RegistrationState != LinphoneRegistrationState.LinphoneRegistrationOk)
            {
                MessageBox.Show("Not Registered. Please register first", "ACE", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return false;
            }

            _linphoneService.MakeCall(remoteUri, true, ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.USE_RTT, true));
            return true;
        }

        
        internal static void StartChat(MainWindow wnd, string chatGUID, string displayName, string remoteUri)
        {
           
       }
        
    }
}
