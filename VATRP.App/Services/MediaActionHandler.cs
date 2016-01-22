using System.Windows;
using System.Windows.Controls;
using VATRP.Core.Extensions;
using VATRP.Core.Interfaces;
using VATRP.Core.Model;
using VATRP.Core.Services;
using VATRP.LinphoneWrapper.Enums;

namespace com.vtcsecure.ace.windows.Services
{
    internal static class MediaActionHandler
    {
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
            bool muteMicrophone = false;
            bool muteSpeaker = false;
            bool enableVideo = false;
            if (App.CurrentAccount != null)
            {
                muteMicrophone = App.CurrentAccount.MuteMicrophone;
                muteSpeaker = App.CurrentAccount.MuteSpeaker;
                enableVideo = App.CurrentAccount.EnableVideo;
            }

            var target = remoteUri;
            string un, host;
            int port;
            VATRPCall.ParseSipAddress(remoteUri, out un, out host, out port);
            if (!host.NotBlank())
            {
                target = string.Format("{0}@{1}", un, App.CurrentAccount.ProxyHostname);
            }
            // update video policy settings prior to making a call
            _linphoneService.MakeCall(target, true, ServiceManager.Instance.ConfigurationService.Get(Configuration.ConfSection.GENERAL,
                Configuration.ConfEntry.USE_RTT, true), muteMicrophone, muteSpeaker, enableVideo, ServiceManager.Instance.LocationString);
            return true;
        }
       
    }
}
