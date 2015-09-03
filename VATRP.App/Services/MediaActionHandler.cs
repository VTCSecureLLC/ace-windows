using System.Windows;
using System.Windows.Controls;
using VATRP.Core.Model;
using VATRP.Core.Services;
using VATRP.LinphoneWrapper.Enums;

namespace VATRP.App.Services
{
    internal static class MediaActionHandler
    {
        internal static void MakeAudioCall(string remoteUri)
        {
            LinphoneService _linphoneService = ServiceManager.Instance.LinphoneSipService;
            

            if (MainWindow.RegistrationState != LinphoneRegistrationState.LinphoneRegistrationOk)
            {
                MessageBox.Show("Not Registered. Please register first", "VATRP", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            if (MainWindow.RegistrationState != LinphoneRegistrationState.LinphoneRegistrationOk)
            {
                MessageBox.Show("Not Registered. Please register first", "VATRP", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            _linphoneService.MakeCall(remoteUri, false);
        }

        internal static void ReceiveCall()
        {
           
        }

        internal static void MakeVideoCall(string remoteUri)
        {
            LinphoneService _linphoneService = ServiceManager.Instance.LinphoneSipService;

            if (!_linphoneService.CanMakeVideoCall())
            {
                MessageBox.Show("Video call not supported yet.", "VATRP", MessageBoxButton.OK,
                   MessageBoxImage.Warning);
                return;
            }

            if (MainWindow.RegistrationState != LinphoneRegistrationState.LinphoneRegistrationOk)
            {
                MessageBox.Show("Not Registered. Please register first", "VATRP", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            _linphoneService.MakeCall(remoteUri, true);
        }

        
        internal static void StartChat(MainWindow wnd, string chatGUID, string displayName, string remoteUri)
        {
           
       }
        
    }
}
